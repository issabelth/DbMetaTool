namespace DbMetaTool.Helpers
{
    public static class ScriptsHelper
    {

        /// <summary>
        /// Sprawdza czy w skrypce nie występują niebezpieczne polecenia.
        /// </summary>
        public static bool CheckScriptSafety(string script, out string errorInfo)
        {
            Console.WriteLine("Sprawdzam skrypt...");

            errorInfo = string.Empty;

            if (script.Contains("DROP ") ||
                script.Contains("TRUNCATE "))
            {
                errorInfo += $"{Environment.NewLine}[UPDATE ERROR] Halo, ale tak to nie można :D";
            }
            if (script.Contains("UPDATE") &&
                !script.Contains("WHERE"))
            {
                errorInfo += $"{Environment.NewLine}[UPDATE ERROR] Podejrzane zachowanie- próba UPDATE bez klauzuli WHERE. Zablokowano.";
            }
            if (script.Contains("DELETE") &&
                !script.Contains("WHERE"))
            {
                errorInfo += $"{Environment.NewLine}[UPDATE ERROR] Podejrzane zachowanie- próba DELETE bez klauzuli WHERE. Zablokowano.";
            }
            if (script.Contains("SET OFFLINE"))
            {
                errorInfo += $"{Environment.NewLine}[UPDATE ERROR] Podejrzane zachowanie- próba odłączenia bazy. Zablokowano.";
            }
            if (script.Contains("EXECUTE BLOCK"))
            {
                errorInfo += $"{Environment.NewLine}[UPDATE ERROR] Ryzykowne zachowanie- ryzko np. nieskończonej pętli. Zablokowano.";
            }

            if (!string.IsNullOrWhiteSpace(errorInfo))
            {
                return false;
            }
            return true;
        }
    }
}