using DbMetaTool.Interfaces;
using FirebirdSql.Data.FirebirdClient;
using static DbMetaTool.DbMethods.ExportDbMethods;
using static DbMetaTool.Helpers.ScriptsHelper;

namespace DbMetaTool
{
    public static class Program
    {
        // Przykładowe wywołania:
        // DbMetaTool build-db --db-dir "C:\db\fb5" --scripts-dir "C:\scripts"
        // DbMetaTool export-scripts --connection-string "..." --output-dir "C:\out"
        // DbMetaTool update-db --connection-string "..." --scripts-dir "C:\scripts"
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                // [Iza]
                // Aby było bardziej user-friendly zamiast podawania connectionString można by go raz zdefiniować w pliku i go pobierać
                // (jak zrobiłam dla metody BuildDatabase),
                // lub w pliku zapisać dane do połączenia i użyć FirebirdSql.Data.FirebirdClient.FbConnectionStringBuilder()
                Console.WriteLine("Użycie:");
                Console.WriteLine("  build-db --db-dir <ścieżka> --scripts-dir <ścieżka>");
                Console.WriteLine("  export-scripts --connection-string <connStr> --output-dir <ścieżka>");
                Console.WriteLine("  update-db --connection-string <connStr> --scripts-dir <ścieżka>");
                return 1;
            }

            // [Iza]
            // Można by użyć ManyConsole do prostego podawania opcji poprzez HasOption
            try
            {
                var command = args[0].ToLowerInvariant();

                switch (command)
                {
                    case "build-db":
                        {
                            string dbDir = GetArgValue(args, "--db-dir");
                            string scriptsDir = GetArgValue(args, "--scripts-dir");

                            BuildDatabase(dbDir, scriptsDir);
                            // [Iza]
                            // Zwracałabym bool, czy się udało czy nie, aby nie pokazywać tego komunikatu jak był błąd.
                            Console.WriteLine("Baza danych została zbudowana pomyślnie.");
                            return 0;
                        }

                    case "export-scripts":
                        {
                            string connStr = GetArgValue(args, "--connection-string");
                            string outputDir = GetArgValue(args, "--output-dir");

                            ExportScripts(connStr, outputDir);
                            Console.WriteLine("Skrypty zostały wyeksportowane pomyślnie.");
                            return 0;
                        }

                    case "update-db":
                        {
                            string connStr = GetArgValue(args, "--connection-string");
                            string scriptsDir = GetArgValue(args, "--scripts-dir");

                            UpdateDatabase(connStr, scriptsDir);
                            Console.WriteLine("Baza danych została zaktualizowana pomyślnie.");
                            return 0;
                        }

                    default:
                        Console.WriteLine($"Nieznane polecenie: {command}");
                        return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd: " + ex.Message);
                return -1;
            }
        }

        private static string GetArgValue(string[] args, string name)
        {
            int idx = Array.IndexOf(args, name);
            if (idx == -1 || idx + 1 >= args.Length)
                throw new ArgumentException($"Brak wymaganego parametru {name}");
            return args[idx + 1];
        }

        /// <summary>
        /// Buduje nową bazę danych Firebird 5.0 na podstawie skryptów.
        /// </summary>
        public static void BuildDatabase(string databaseDirectory, string scriptsDirectory, string format = "sql")
        {
            // TODO:
            // 1) Utwórz pustą bazę danych FB 5.0 w katalogu databaseDirectory.
            // 2) Wczytaj i wykonaj kolejno skrypty z katalogu scriptsDirectory
            //    (tylko domeny, tabele, procedury).
            // 3) Obsłuż błędy i wyświetl raport.

            if (string.IsNullOrWhiteSpace(databaseDirectory) ||
                string.IsNullOrWhiteSpace(scriptsDirectory))
            {
                Console.WriteLine("Nie podano wszystkich wymaganych parametrów!");
                return;
            }
            if (!Directory.Exists(scriptsDirectory))
            {
                Console.WriteLine("Katalog skryptów lub katalog bazy danych nie istnieje.");
                return;
            }

            if (!Directory.Exists(databaseDirectory))
            {
                Directory.CreateDirectory(databaseDirectory);
                Console.WriteLine($"Utworzono katalog: {databaseDirectory}");
            }

            // 1) Utwórz pustą bazę danych FB 5.0
            // Tworzymy Connection String na podstawie ścieżki dbPath

            string databasePath = Path.Combine(databaseDirectory, $"Database_{DateTime.Now:yyyyMMddHHmmss}.fdb");

            if (File.Exists(databasePath))
            {
                Console.WriteLine($"Baza danych w ścieżce {databasePath} już istnieje.");
                return;
            }

            Console.WriteLine($"Tworzenie bazy w {databasePath} ...");

            var connString = DbConnection.GetConnectionString() + databasePath;

            try
            {
                FbConnection.CreateDatabase(connString);
                Console.WriteLine("Baza została utworzona pomyślnie.");

                using (var conn = new FbConnection(connString))
                {
                    conn.Open();

                    var files = Directory.GetFiles(scriptsDirectory, $"*.{format}").OrderBy(f => f); // nazwy plików tworzyłam z numeracją

                    if (files == null ||
                        files.Count() <= 0)
                    {
                        Console.WriteLine($"Brak plików w formacie {format} w ścieżce {scriptsDirectory}");
                        return;
                    }

                    foreach (var file in files)
                    {
                        string sql = File.ReadAllText(file);

                        if (string.IsNullOrWhiteSpace(sql))
                        {
                            Console.WriteLine("Pusta zawartość pliku. Pominięto.");
                            continue;
                        }

                        using (var cmd = new FbCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                            Console.WriteLine($"[OK] Wykonano: {Path.GetFileName(file)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BŁĄD]: {ex.Message}");
            }
        }

        public static void ExportScripts(string connectionString, string outputDirectory, string format = "sql")
        {
            // TODO:
            // 1) Połącz się z bazą danych przy użyciu connectionString.
            // 2) Pobierz metadane domen, tabel (z kolumnami) i procedur.
            // 3) Wygeneruj pliki .sql / .json / .txt w outputDirectory.

            if (string.IsNullOrWhiteSpace(connectionString) ||
                string.IsNullOrWhiteSpace(outputDirectory))
            {
                Console.WriteLine("Nie podano wszystkich wymaganych parametrów!");
                return;
            }

            outputDirectory = Path.Combine(outputDirectory, $"Scripts_{format}_{DateTime.Now:yyyyMMddHHmmss}");

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // [Iza] Na szybko użyty builder, powinno być budowanie osobno od logiki.
            IMetadataBuilder builder = MetadataBuilderFactory.GetBuilder(format);
            builder.Reset();

            try
            {
                using (FbConnection conn = new FbConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Nie udało się nawiązać połączenia. Sprawdź swój connectionString.");
                        return;
                    }

                    int counter = 1; // żeby można było łatwo kolejkować pliki w razie potrzeby innego użycia buildera

                    // [Iza] tutaj jeszcze dużo do zrefaktorowania, ale już zostawiam, bo przesadzam :)
                    ExportDomains(outputDirectory, format, builder, conn, ref counter);
                    ExportTables(outputDirectory, format, builder, conn, ref counter);
                    ExportProcedures(outputDirectory, format, builder, conn, ref counter);
                }
                Console.WriteLine($"Eksport do formatu {format} zakończony.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Nie udało się wyeksportować do formatu {format}.{Environment.NewLine}" +
                    $"Treść błędu: {ex}");
            }
        }

        /// <summary>
        /// Aktualizuje istniejącą bazę danych Firebird 5.0 na podstawie skryptów.
        /// </summary>
        public static void UpdateDatabase(string connectionString, string scriptsDirectory, string format = "sql")
        {
            // TODO:
            // 1) Połącz się z bazą danych przy użyciu connectionString.
            // 2) Wykonaj skrypty z katalogu scriptsDirectory (tylko obsługiwane elementy).
            // 3) Zadbaj o poprawną kolejność i bezpieczeństwo zmian.

            if (string.IsNullOrWhiteSpace(connectionString) ||
                string.IsNullOrWhiteSpace(scriptsDirectory))
            {
                Console.WriteLine("Nie podano wszystkich wymaganych parametrów!");
                return;
            }
            if (!Directory.Exists(scriptsDirectory))
            {
                Console.WriteLine("Katalog skryptów nie istnieje.");
                return;
            }

            using (FbConnection conn = new FbConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    var files = Directory.GetFiles(scriptsDirectory, $"*.{format}").OrderBy(f => f); // nazwy plików tworzyłam z numeracją

                    if (files == null ||
                        files.Count() <= 0)
                    {
                        Console.WriteLine($"Brak plików w formacie {format} w ścieżce {scriptsDirectory}");
                        return;
                    }

                    foreach (var file in files)
                    {
                        // pomijam wybrane pliki
                        var fileName = Path.GetFileName(file);

                        if (fileName.Contains("DOMAIN"))
                        {
                            continue;
                        }

                        string script = File.ReadAllText(file);

                        if (string.IsNullOrWhiteSpace(script))
                        {
                            Console.WriteLine("Pusta zawartość pliku. Pominięto.");
                            continue;
                        }

                        string errorInfo;

                        if (!CheckScriptSafety(script, out errorInfo))
                        {
                            Console.WriteLine(errorInfo);
                            return;
                        }

                        using (FbCommand cmd = new FbCommand(script, conn))
                        {
                            cmd.ExecuteNonQuery();
                            Console.WriteLine($"[UPDATE SUCCESS] Wykonano: {Path.GetFileName(file)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[UPDATE ERROR] Wystąpił błąd podczas aktualizacji: {ex.Message}");
                }
            }
        }

    }
}