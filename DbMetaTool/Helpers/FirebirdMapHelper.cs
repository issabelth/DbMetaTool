namespace DbMetaTool.Helpers
{
    public static class FirebirdMapHelper
    {

        public static string MapFirebirdType(string typeCode, int length)
        {
            return MapFirebirdType(typeCode, length.ToString());
        }

        public static string MapFirebirdType(string typeCode, string length)
        {
            if (string.IsNullOrWhiteSpace(typeCode))
            {
                throw new Exception("Pusty typ kolumny!");
            }

            typeCode = typeCode.Replace("RDB$", "");

            if (typeCode.StartsWith("VARYING"))
            {
                return $"VARCHAR({length})";
            }
            else if (typeCode.StartsWith("CSTRING"))
            {
                return $"CHAR({length})";
            }
            else if (typeCode.StartsWith("LONG"))
            {
                return "INTEGER";
            }

            return typeCode.Trim() switch
            {
                "0" => "INTEGER",
                "7" => "SMALLINT",
                "8" => "INTEGER",
                "10" => "FLOAT",
                "14" => $"CHAR({length})",
                "16" => "BIGINT",
                "27" => "DOUBLE PRECISION",
                "35" => "TIMESTAMP",
                "37" => $"VARCHAR({length})",
                _ => "VARCHAR(255)" // domyślny fallback
            };
        }

    }
}