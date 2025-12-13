using System.Data;
using System.Text;

namespace DbMetaTool.Builders
{
    public class SqlMetadataBuilder : BaseMetadataBuilder
    {

        public override string Build()
        {
            var sqlBuilder = new StringBuilder();

            foreach (var domain in _domains)
            {
                string nullClause = domain.IsNullable ? "" : " NOT NULL";
                string defaultClause = string.IsNullOrWhiteSpace(domain.DefaultValue) ? "" : $" {domain.DefaultValue}";

                sqlBuilder.AppendLine($"CREATE DOMAIN {domain.Name} AS {domain.Type}{defaultClause}{nullClause};");
            }

            foreach (var table in _tables)
            {
                sqlBuilder.AppendLine($"CREATE TABLE {table.Name} (");

                var columns = table.Columns.Select(c =>
                {
                    string nullClause = c.IsNullable ? "" : " NOT NULL";
                    return $"    {c.Name} {c.DomainName}{nullClause}";
                });

                sqlBuilder.AppendLine(string.Join(",\n", columns));
                sqlBuilder.AppendLine(");");
            }

            foreach (var procedure in _procedures)
            {
                //sqlBuilder.AppendLine("SET TERM ^ ;");
                string paramsList = procedure.Parameters.Any()
                    ? $"({string.Join(", ", procedure.Parameters)})"
                    : "";
                sqlBuilder.AppendLine($"CREATE OR ALTER PROCEDURE {procedure.Name} {paramsList}");
                sqlBuilder.AppendLine("AS");
                // Nie dodajemy BEGIN/END, ponieważ znajdują się już w procedure.Source
                sqlBuilder.AppendLine(procedure.Source.TrimEnd(';'));
                //sqlBuilder.AppendLine("^");
                //sqlBuilder.AppendLine("SET TERM ; ^");
            }

            return sqlBuilder.ToString();
        }

    }
}