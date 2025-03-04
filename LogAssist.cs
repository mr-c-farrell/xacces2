namespace XAccess2
{
    public static class LogAssist
    {
        public static string LogsDirectory = "C:\\IisLogs\\XAccess\\";
        public static void LogError(Exception ex, string errorSource = "")
        {
            string FilePath = string.Format(LogsDirectory + "XAccessLog_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
            using (StreamWriter sw = new StreamWriter(FilePath, true))
            {
                sw.WriteLine(string.Format("{0} {1} {2} \nStackTrace:\n{3}", DateTime.Now.ToString("HH:mm:ss"), errorSource, ex.Message, ex.StackTrace));
            }
        }

        public static void LogMessage(string dataToLog, string logSource = "")
        {
            string FilePath = string.Format(LogsDirectory + "XAccessErrorsLog_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
            using (StreamWriter sw = new StreamWriter(FilePath, true))
            {
                sw.WriteLine(string.Format("{0} {1} {2}", DateTime.Now.ToString("HH:mm:ss"), logSource, @dataToLog));
            }
        }
    }
}
