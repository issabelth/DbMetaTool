namespace DbMetaTool
{
    public static class DbConnection
    {
        public static string? GetConnectionString()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(baseDirectory, "ConnectionString.txt");

            if (!Directory.Exists(filePath))
            {
                Console.WriteLine($"Ścieżka {filePath} nie istnieje! " +
                    $"Czy na pewno dodałeś swój plik ConnectionString.txt zgodnie z instrukcją w README?");
                return null;
            }

            string content = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine($"Plik ConnectionString.txt jest pisty! " +
                    $"Czy na pewno uzupełniłeś plik ConnectionString.txt zgodnie z instrukcją w README?");
                return null;
            }

            return content;
        }
    }
}