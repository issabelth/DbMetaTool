namespace DbMetaTool
{
    public static class DbConnection
    {
        public static string GetConnectionString()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(baseDirectory, "ConnectionString.txt");
            string content = File.ReadAllText(filePath);

            return content;
        }
    }
}