namespace SimpleTGBot
{
    class Logging
    {
        public static void WriteLog(string logMessage)
        {      
            using (var sw = new StreamWriter("log.txt", true))
                sw.WriteLine($"{DateTime.Now} Message: {logMessage}");
            Console.WriteLine($"{DateTime.Now} Message: {logMessage}");
        }

        public static void ErrorWriteLog(string logMessage)
        {
            using (var sw = new StreamWriter("log.txt", true))
                sw.WriteLine($"{DateTime.Now} Error: {logMessage}");
            Console.WriteLine($"{DateTime.Now} Error: {logMessage}");
        }
    }
}
