namespace DbMetaTool.ExtensionMethods
{
    public static class BoolExtensions
    {
        // Mam bałagan z tym not null, do refaktoringu
        public static string ToSqlScript(this bool isNullable)
        {
            return isNullable ? string.Empty : " NOT NULL";
        }
    }
}
