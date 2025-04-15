using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.Console;
using static SimpleTGBot.ImageFilters;
using static SimpleTGBot.Logging;

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(File.ReadAllText("token.txt"), cancellationToken: cts.Token);
var me = await bot.GetMe();

// Глобальные переменные
bool flag_filters = true;   // Флаг на срабатывание /filters
int num_filter = -1;        // Хранит номер фильтра
int img_width = 0;          // Хранит ширину изображения (нужен для фильтра "Изменение размера")
int img_height = 0;         // Хранит высоту изображения (нужен для фильтра "Изменение размера")
int choose_weight = -1;     // Хранит выбранную ширину изображения (нужен для фильтра "Изменение размера")
int choose_height = -1;     // Хранит выбранную высоту изображения (нужен для фильтра "Изменение размера")
int user_input = -1;        // Хранит ответ пользователя
string image_name = "";     // Хранит имя изображения

/// Удаляет все (предыдущие) изображения в директории
foreach (string extension in new string[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" })
    foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), $"*{extension}"))
        File.Delete(file);

bot.OnError += OnError;
bot.OnMessage += OnMessage;

WriteLine($"@{me.Username} is running... Press Enter to terminate");
ReadLine();
cts.Cancel(); // Остановить бота

/// Обнуляет все глобальные переменные
void AllGlobalVarDefault()
{
    flag_filters = true;   
    num_filter = -1;       
    img_width = 0;         
    img_height = 0;        
    choose_weight = -1;    
    choose_height = -1;    
    user_input = -1;        
    image_name = "";
}

/// Метод, который обрабатывает ошибки
async Task OnError(Exception exception, HandleErrorSource source) => WriteLine(exception); 

/// Метод, который обрабатывает полученные сообщения
async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text == "/start")
    {
        AllGlobalVarDefault();
        await bot.SendMessage(msg.Chat, $"Приветсвую {msg.From.FirstName}! Я вам помогу сделать базовую обработку фото. Чтобы выбрать фильтры напишите /filters");
        WriteLog($"Пользователь с id {msg.From.Id} написал {msg.Text}");
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
        WriteLog($"Пользователь с id {msg.From.Id} написал {msg.Text}");
        return;
    }

    if (flag_filters || (image_name != "" || num_filter == -1) && msg.Text is not { } message)
    {
        await bot.SendMessage(msg.Chat, "Я вас не понимаю");
        return;
    }

    if (num_filter == -1)
    {
        if (!new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" }.Contains(msg.Text))
        {
            await bot.SendMessage(msg.Chat, "Напишите цифру от 0 до 9");
            return;
        }
        num_filter = int.Parse(msg.Text);       
    }

    if (image_name == "" && num_filter != 0)
    {
        if(msg.Photo == null)
        {
            await bot.SendMessage(msg.Chat, "Отправьте ваше фото");
            return;
        }

        var fileId = msg.Photo.Last().FileId;
        var tgFile = await bot.GetFile(fileId);

        image_name = $"image{msg.Chat.Id}_{msg.MessageId}.png";

        await using (var stream = new FileStream(image_name, FileMode.Create))
            await bot.DownloadFile(tgFile, stream);

        // Получаем ширину и высоту
        using (var image = SixLabors.ImageSharp.Image.Load(image_name))
            (img_width, img_height) = (image.Width, image.Height);

        if (img_width < 10 || img_height < 10)
        {
            image_name = "";
            await bot.SendMessage(msg.Chat, "Слишком маленькие размеры изображения");
            await bot.SendMessage(msg.Chat, "Отправьте другое фото");
            return;
        }
    }

    switch (num_filter)
    {
        case 0:
            flag_filters = true;
            num_filter = -1;

            await bot.SendMessage(msg.Chat, "Приходите ещё :)");
            return;

        case 1:
            await Invert_Colors(bot, msg.Chat, image_name);
            break;

        case 2:
            if (choose_weight == -1)
            {
                choose_weight = 0;
                await bot.SendMessage(msg.Chat, $"Введите ширину (от 10 до {img_width})");
                return;
            }

            if (!int.TryParse(msg.Text, out user_input) || user_input < 10 || user_input > (choose_height == -1 ? img_width : img_height))
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от 10 до {(choose_height == -1 ? img_width : img_height)}");
                return;
            }

            if (choose_height == -1)
            {
                choose_height = 0;
                choose_weight = user_input;
                await bot.SendMessage(msg.Chat, $"Введите высоту (от 10 до {img_height})");
                return;
            }

            choose_height = user_input;
            double aspectRatio = (double)choose_weight / choose_height;
            if (aspectRatio > 20 || aspectRatio < 0.05) 
            {
                choose_height = -1;
                await bot.SendMessage(msg.Chat, "Недопустимое соотношение сторон для отправки в телеграмм. Попробуйте другие размеры");
                await bot.SendMessage(msg.Chat, $"Введите ширину (от 10 до {img_width})");
                return;
            }

            await ChangingSize(bot, msg.Chat, image_name, choose_weight, choose_height);

            choose_weight = -1;
            choose_height = -1;

            break;

        case 3:
            await BWFilter(bot, msg.Chat, image_name);
            break;

        case 4:      
            if (user_input == -1)
            {
                user_input = 0;
                await bot.SendMessage(msg.Chat, $"Введите интенсивность от -10 до 10 (0 - никаких изменений)");
                return;
            }

            if (!int.TryParse(msg.Text, out user_input) || user_input < -10 || user_input > 10)
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от -10 до 10");
                return;
            }

            await ContrastFilter(bot, msg.Chat, image_name, user_input > 0 ? 1f + (float)user_input / 10 : ((float)user_input + 10) / 10);
            break;

        case 5:             
            if (user_input == -1)
            {
                user_input = 0;
                await bot.SendMessage(msg.Chat, $"Введите интенсивность от -10 до 10 (0 - никаких изменений)");
                return;
            }

            if (!int.TryParse(msg.Text, out user_input) || user_input < -10 || user_input > 10)
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от -10 до 10");
                return;
            }

            await BrightnessFilter(bot, msg.Chat, image_name, user_input > 0 ? 1f + (float)user_input / 10 : ((float)user_input + 10) / 10);
            break;

        case 6:
            await VignetteFilter(bot, msg.Chat, image_name);
            break;

        case 7:
            if (user_input == -1)
            {
                user_input = 0;
                await bot.SendMessage(msg.Chat, $"Введите силу усиления от 0 до 255");
                return;
            }

            if (!int.TryParse(msg.Text, out user_input) || user_input < 0 || user_input > 255)
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от 0 до 255");
                return;
            }

            await RedFilter(bot, msg.Chat, image_name, (byte)user_input);
            break;

        case 8:
            if (user_input == -1)
            {
                user_input = 0;
                await bot.SendMessage(msg.Chat, $"Введите силу усиления от 0 до 255");
                return;
            }

            if (!int.TryParse(msg.Text, out user_input) || user_input < 0 || user_input > 255)
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от 0 до 255");
                return;
            }

            await GreenFilter(bot, msg.Chat, image_name, (byte)user_input);
            break;

        case 9:
            if (user_input == -1)
            {
                user_input = 0;
                await bot.SendMessage(msg.Chat, $"Введите силу усиления от 0 до 255");
                return;
            }

            if (!int.TryParse(msg.Text, out user_input) || user_input < 0 || user_input > 255)
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от 0 до 255");
                return;
            }
            
            await BlueFilter(bot, msg.Chat, image_name, (byte)user_input);
            break;
    }

    user_input = -1;
    image_name = "";
    await bot.SendMessage(msg.Chat, "Снова отправьте фото или выберите другой фильтр /filters");
}
