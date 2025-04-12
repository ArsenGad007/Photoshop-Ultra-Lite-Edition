namespace SimpleTGBot;

public static class Program
{
    // Метод main немного видоизменился для асинхронной работы
    public static async Task Main(string[] args)
    {
        TelegramBot telegramBot = new TelegramBot();
        await telegramBot.Run();
    }
}