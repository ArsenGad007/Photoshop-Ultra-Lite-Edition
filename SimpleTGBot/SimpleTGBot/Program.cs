using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using static System.Console;

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient("7994207689:AAEXq6H7rmQHQT4W6R8Y8h7sCVTRntjAx6w", cancellationToken: cts.Token);
var me = await bot.GetMe();

bool flag_filters = true;
bool flag_nums = true;

bot.OnError += OnError;
bot.OnMessage += OnMessage;

WriteLine($"@{me.Username} is running... Press Enter to terminate");
ReadLine();
cts.Cancel(); // Остановить бота

/// Метод, который обрабатывает ошибки
async Task OnError(Exception exception, HandleErrorSource source)
{
    WriteLine(exception); // just dump the exception to the console
}

/// Метод, который обрабатывает полученные сообщения
async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text == "/start")
    {
        await bot.SendMessage(msg.Chat, $"Приветсвую {msg.From.FirstName}! Я вам помогу сделать базовую обработку фото. Чтобы выбрать фильтры напишите /filters"); 
        return;
    }

    if (msg.Text == "/filters")
    {
        flag_filters = false;
        await bot.SendMessage(msg.Chat,
            "Напишите нужную цифру:\n" +
            "0) Выход\n" +
            "1) Инвертировать цвета\n" +
            "2) Изменение размера\n" +
            "3) Чб фильтр\n" +
            "4) Увеличение контраста\n" +
            "5) Увеличение яркости\n" +
            "6) Добавить виньетку\n" +
            "7) Усиление красного канала\n" +
            "8) Усиление зелёного канала\n" +
            "9) Усиление синего канала");
        return;
    }

    if (flag_nums && msg.Text is not { } message)
    {
        await bot.SendMessage(msg.Chat, "Я вас не понимаю");
        return;
    }

    if (flag_filters)
    {
        await bot.SendMessage(msg.Chat, "Я вас не понимаю");
        return;
    }

    if (flag_nums)
    {
        if (!new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" }.Contains(msg.Text))
        {
            await bot.SendMessage(msg.Chat, "Напишите цифру от 0 до 9");
            return;
        }
        flag_nums = false;
    }

    if (msg.Photo == null)
    {
        await bot.SendMessage(msg.Chat, "Отправьте ваше фото");
        return;
    }

    await bot.SendMessage(msg.Chat, "Фото ");
    var fileId = msg.Photo.Last().FileId;
    var tgFile = await bot.GetFile(fileId);

    await using var stream = File.Create("downloaded.png");
    await bot.DownloadFile(tgFile, stream);
    return;
}
