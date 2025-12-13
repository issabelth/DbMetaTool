using DbMetaTool.Helpers;
using DbMetaTool.Interfaces;
using DbMetaTool.Models;
using FirebirdSql.Data.FirebirdClient;
using static DbMetaTool.ExtensionMethods.BoolExtensions;

namespace DbMetaTool.DbMethods
{
    public static class ExportDbMethods
    {

        public static void ExportDomains(string outputDirectory, string format, IMetadataBuilder builder, FbConnection conn, ref int counter)
        {
            string sql = @"
                SELECT  
                    RDB$FIELD_NAME AS DomainName,
                    RDB$FIELD_TYPE AS FieldType,
                    RDB$CHARACTER_LENGTH AS FieldLength,
                    RDB$DEFAULT_SOURCE AS DefaultValue,
                    RDB$NULL_FLAG AS NotNull
                FROM RDB$FIELDS
                WHERE RDB$FIELD_NAME NOT LIKE 'RDB$%'
                  AND RDB$FIELD_NAME NOT LIKE 'MON$%'
                  AND RDB$FIELD_NAME NOT LIKE 'SEC$%'";

            using var command = new FbCommand(sql, conn);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                builder.Reset();
                var domainName = reader["DomainName"]?.ToString()?.Trim();

                builder.AddDomain(new DomainModel
                {
                    Name = domainName,
                    Type = reader["FieldType"]?.ToString() ?? string.Empty,
                    Length = reader["FieldLength"] != DBNull.Value ? reader["FieldLength"].ToString() : string.Empty,
                    DefaultValue = reader["DefaultValue"] != DBNull.Value ? reader["DefaultValue"].ToString() : string.Empty,
                    IsNullable = reader["NotNull"] == DBNull.Value
                });

                var result = builder.Build();
                File.WriteAllText(Path.Combine(outputDirectory, $"{counter}_DOMAIN_{domainName}.{format.ToLower()}"), result);
                counter++;
            }
        }

        public static void ExportTables(string outputDirectory, string format, IMetadataBuilder builder, FbConnection conn, ref int counter)
        {
            // 1. Pobierz listę tabel
            using var cmdTables = new FbCommand(
                "SELECT RDB$RELATION_NAME " +
                "FROM RDB$RELATIONS " +
                "WHERE RDB$SYSTEM_FLAG = 0 " +
                "AND RDB$VIEW_BLR IS NULL",
                conn);

            using var readerTables = cmdTables.ExecuteReader();
            var tableNames = new List<string>();

            while (readerTables.Read())
            {
                tableNames.Add(readerTables["RDB$RELATION_NAME"]?.ToString()?.Trim() ?? string.Empty);
            }

            // 2. Pobierz kolumny dla każdej tabeli
            foreach (var tableName in tableNames.Where(n => !string.IsNullOrEmpty(n)))
            {
                builder.Reset();
                var table = new TableModel { Name = tableName };

                string sqlCols =
                            @"SELECT 
                        rf.RDB$FIELD_NAME, 
                        t.RDB$TYPE_NAME, 
                        f.RDB$CHARACTER_LENGTH, 
                        rf.RDB$NULL_FLAG
                    FROM RDB$RELATION_FIELDS rf
                    JOIN RDB$FIELDS f ON rf.RDB$FIELD_SOURCE = f.RDB$FIELD_NAME
                    JOIN RDB$TYPES t ON f.RDB$FIELD_TYPE = t.RDB$TYPE AND t.RDB$FIELD_NAME = 'RDB$FIELD_TYPE'
                    WHERE rf.RDB$RELATION_NAME = @tableName";

                using var cmdCols = new FbCommand(sqlCols, conn);
                cmdCols.Parameters.Add("@tableName", tableName);
                using var readerCols = cmdCols.ExecuteReader();

                while (readerCols.Read())
                {
                    string fieldName = readerCols["RDB$FIELD_NAME"]?.ToString()?.Trim() ?? "UNKNOWN_COLUMN";
                    string typeName = readerCols["RDB$TYPE_NAME"]?.ToString()?.Trim() ?? "VARCHAR";

                    // Bezpieczna konwersja długości
                    int length = readerCols["RDB$CHARACTER_LENGTH"] != DBNull.Value
                        ? Convert.ToInt32(readerCols["RDB$CHARACTER_LENGTH"])
                        : 0;

                    var domainName = FirebirdMapHelper.MapFirebirdType(typeName, length);

                    table.Columns.Add(new ColumnModel
                    {
                        Name = fieldName,
                        DomainName = domainName,
                        IsNullable = readerCols["RDB$NULL_FLAG"] == DBNull.Value
                    });
                }

                builder.AddTable(table);
                var result = builder.Build();
                File.WriteAllText(Path.Combine(outputDirectory, $"{counter}_TABLE_{table.Name}.{format.ToLower()}"), result);
                counter++;
            }
        }

        public static void ExportProcedures(string outputDirectory, string format, IMetadataBuilder builder, FbConnection conn, ref int counter)
        {
            builder.Reset();

            // Pobieramy nazwy i źródła
            string sqlProcs =
                "SELECT RDB$PROCEDURE_NAME, RDB$PROCEDURE_SOURCE " +
                "FROM RDB$PROCEDURES " +
                "WHERE RDB$SYSTEM_FLAG = 0"; // systemowe

            using var cmd = new FbCommand(sqlProcs, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                builder.Reset();
                var procName = reader["RDB$PROCEDURE_NAME"].ToString().Trim();

                var procedure = new ProcedureModel
                {
                    Name = procName,
                    Source = reader["RDB$PROCEDURE_SOURCE"].ToString().Trim(),
                    Parameters = new List<string>()
                };

                // Pobieramy parametry wejściowe (RDB$PARAMETER_TYPE = 0)
                string sqlParams = @"
                    SELECT 
                        pp.RDB$PARAMETER_NAME, 
                        t.RDB$TYPE_NAME, 
                        f.RDB$CHARACTER_LENGTH,
                        f.RDB$NULL_FLAG
                    FROM RDB$PROCEDURE_PARAMETERS pp
                    JOIN RDB$FIELDS f ON pp.RDB$FIELD_SOURCE = f.RDB$FIELD_NAME
                    JOIN RDB$TYPES t ON f.RDB$FIELD_TYPE = t.RDB$TYPE AND t.RDB$FIELD_NAME = 'RDB$FIELD_TYPE'
                    WHERE pp.RDB$PROCEDURE_NAME = @pName 
                      AND pp.RDB$PARAMETER_TYPE = 0
                    ORDER BY pp.RDB$PARAMETER_NUMBER";

                using var cmdParams = new FbCommand(sqlParams, conn);
                cmdParams.Parameters.Add("@pName", procName);
                using var readerParams = cmdParams.ExecuteReader();

                while (readerParams.Read())
                {
                    string? paramName = readerParams["RDB$PARAMETER_NAME"]?.ToString()?.Trim() ?? string.Empty;
                    string? paramType = readerParams["RDB$TYPE_NAME"]?.ToString()?.Trim() ?? string.Empty;
                    string? paramLength = readerParams["RDB$CHARACTER_LENGTH"]?.ToString()?.Trim() ?? string.Empty;
                    var domainName = FirebirdMapHelper.MapFirebirdType(paramType, paramLength);

                    var nullFlagValue = readerParams["RDB$NULL_FLAG"];
                    bool isNotNull = nullFlagValue != DBNull.Value && nullFlagValue != null && Convert.ToInt32(nullFlagValue) == 1;

                    // IsNullable to zaprzeczenie isNotNull
                    bool isNullable = !isNotNull;

                    procedure.Parameters.Add($"{paramName} {domainName}{isNullable.ToSqlScript()}");
                }

                builder.AddProcedure(procedure);

                var result = builder.Build();
                File.WriteAllText(Path.Combine(outputDirectory, $"{counter}_PROCEDURE_{procName}.{format.ToLower()}"), result);
                counter++;
            }
        }

    }
}
