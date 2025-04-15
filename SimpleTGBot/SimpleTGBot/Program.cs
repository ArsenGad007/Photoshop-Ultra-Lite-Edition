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
bool flag_filters = true;   // Флаг на срабатывание /filters
bool flag_image = true;     // Флаг на получение фото
bool flag_width = true;     // Флаг на получение ширины изображения (нужен для фильтра "Изменение размера")
bool flag_height = false;   // Флаг на получение высоты изображения (нужен для фильтра "Изменение размера")
int num_filter = 0;         // Хранит номера фильтра
int img_weight = 0;         // Хранит ширину изображения (нужен для фильтра "Изменение размера")
int img_height = 0;         // Хранит высоту изображения (нужен для фильтра "Изменение размера")
int choose_weight = 0;      // Хранит выбранную ширину изображения (нужен для фильтра "Изменение размера")
int choose_height = 0;      // Хранит выбранную высоту изображения (нужен для фильтра "Изменение размера")
float intensity = 1f;       // Хранит значение интенсивности (нужен для фильтров "Увеличение контраста" и "Увеличение яркости")
byte power = 0;             // Хранит значения силы (нужен для фильтров "Усиление ... канала")
string image_name = "";     // Хранит имя изображения

bot.OnError += OnError;
bot.OnMessage += OnMessage;

WriteLine($"@{me.Username} is running... Press Enter to terminate");
ReadLine();
cts.Cancel(); // Остановить бота

/// Обнуляет все глобальные переменные
void AllGlobalVarDefault()
{
    flag_filters = true;      
    flag_image = true;     
    flag_width = true;     
    flag_height = false;    
    num_filter = 0;         
    img_weight = 0;         
    img_height = 0;
}

/// Метод, который обрабатывает ошибки
async Task OnError(Exception exception, HandleErrorSource source) => WriteLine(exception); 

/// Метод, который обрабатывает полученные сообщения
async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text == "/start")
    {
        AllGlobalVarDefault();
        await bot.SendMessage(msg.Chat, $"Приветсвую {msg?.From.FirstName ?? "пользователь"}! Я вам помогу сделать базовую обработку фото. Чтобы выбрать фильтры напишите /filters"); 
        return;
    }

    if (msg.Text == "/filters")
    {
        AllGlobalVarDefault();
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

    if (flag_filters || ((!flag_image) || num_filter == 0) && msg.Text is not { } message)
    {
        await bot.SendMessage(msg.Chat, "Я вас не понимаю.");
        return;
    }

    if (num_filter == 0)
    {
        if (!new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" }.Contains(msg.Text))
        {
            await bot.SendMessage(msg.Chat, "Напишите цифру от 0 до 9");
            return;
        }
        num_filter = int.Parse(msg.Text);       
    }

    if (flag_image && num_filter != 0)
    {
        if(msg.Photo == null)
        {
            await bot.SendMessage(msg.Chat, "Отправьте ваше фото");
            return;
        }

        flag_image = false;

        var fileId = msg.Photo.Last().FileId;
        var tgFile = await bot.GetFile(fileId);

        image_name = $"image{msg.Chat.Id}_{msg.MessageId}.png";

        await using (var stream = new FileStream(image_name, FileMode.Create))
            await bot.DownloadFile(tgFile, stream);

        // Получаем ширину и высоту
        using (var image = SixLabors.ImageSharp.Image.Load(image_name))
            (img_weight, img_height) = (image.Width, image.Height);
    }

    switch (num_filter)
    {
        case 0:
            flag_filters = true;
            num_filter = 0;

            await bot.SendMessage(msg.Chat, "Приходите ещё :)");
            return;

        case 1:
            await Invert_Colors(bot, msg.Chat, image_name);
            break;

        case 2:
            if (flag_width)
            {
                flag_width = false;
                flag_height = true;
                await bot.SendMessage(msg.Chat, $"Введите ширину (от 10 до {img_weight})");
                return;
            }

            if (msg.Text.Any(x => !char.IsDigit(x)) || int.Parse(msg.Text) < 10 || int.Parse(msg.Text) > (flag_height ? img_weight : img_height))
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от 10 до {(flag_height ? img_weight : img_height)}");
                return;
            }

            if (flag_height)
            {
                flag_height = false;
                choose_weight = int.Parse(msg.Text);
                await bot.SendMessage(msg.Chat, $"Введите высоту (от 10 до {img_height})");
                return;
            }

            choose_height = int.Parse(msg.Text);
            double aspectRatio = (double)choose_weight / choose_height;
            if (aspectRatio > 20 || aspectRatio < 0.05) 
            {
                flag_height = true;
                await bot.SendMessage(msg.Chat, "Недопустимое соотношение сторон для отправки в телеграмм. Попробуйте другие размеры");
                await bot.SendMessage(msg.Chat, $"Введите ширину (от 10 до {img_weight})");
                return;
            }
            
            flag_width = true;
            flag_height = false;          

            await ChangingSize(bot, msg.Chat, image_name, choose_weight, choose_height);
            break;

        case 3:
            await BWFilter(bot, msg.Chat, image_name);
            break;

        case 4:
            
            await ContrastFilter(bot, msg.Chat, image_name, 1.5f);
            break;

        case 5:
            await BrightnessFilter(bot, msg.Chat, image_name, 1.5f);
            break;

        case 6:
            await VignetteFilter(bot, msg.Chat, image_name);
            break;

        case 7:
            await RedFilter(bot, msg.Chat, image_name, 255);
            break;

        case 8:
            await GreenFilter(bot, msg.Chat, image_name, 255);
            break;

        case 9:
            await BlueFilter(bot, msg.Chat, image_name, 255);
            break;
    }

    flag_image = true;
    await bot.SendMessage(msg.Chat, "Снова отправьте фото или выберите другой фильтр /filters");
}
