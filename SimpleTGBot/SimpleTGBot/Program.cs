using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.Console;
using static SimpleTGBot.ImageFilters;

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(File.ReadAllText("token.txt"), cancellationToken: cts.Token);
var me = await bot.GetMe();

// Глобальные переменные
bool flag_filters = true;
bool flag_nums = true;
int num_filter = 0;

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
        flag_filters = true;
        flag_nums = true;
        num_filter = 0;

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

    if (flag_filters || (flag_nums && msg.Text is not { } message))
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
        num_filter = int.Parse(msg.Text);
        flag_nums = false;
    }

    if (msg.Photo == null)
    {
        await bot.SendMessage(msg.Chat, "Отправьте ваше фото");
        return;
    }

    var fileId = msg.Photo.Last().FileId;
    var tgFile = await bot.GetFile(fileId);

    var inputPath = $"input_{msg.Chat.Id}_{msg.MessageId}.png";
    var outputPath = $"output_{msg.Chat.Id}_{msg.MessageId}.png";

    await using (var stream = new FileStream(inputPath, FileMode.Create))
    {
        await bot.DownloadFile(tgFile, stream);
    }

    switch (num_filter)
    {
        case 0:

            break;
        case 1:
            await Invert_Colors(bot, msg.Chat, inputPath, outputPath);
            break;
        case 2:

            break;
        case 3:

            break;
        case 4:

            break;
        case 5:
            break;
        case 6:

            break;
        case 7:

            break;
        case 8:

            break;
        case 9:

            break;
    }
}
