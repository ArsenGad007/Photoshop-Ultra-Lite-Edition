using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Console;
using static SimpleTGBot.ImageFilters;
using static SimpleTGBot.Logging;
using static SimpleTGBot.DataSave;

/// Базовые настройки бота
using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(File.ReadAllText("token.txt"), cancellationToken: cts.Token);
var me = await bot.GetMe();

/// Удаляет все (предыдущие) изображения в директории
foreach (string extension in new string[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" })
    foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), $"*{extension}"))
        File.Delete(file);

/// Добавляем обработчики
bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

WriteLine($"@{me.Username} is running... Press Enter to terminate");
ReadLine();
cts.Cancel(); // Останавливает бота

/// Метод, который обрабатывает ошибки
async Task OnError(Exception exception, HandleErrorSource source) => ErrorWriteLog(exception.ToString());

/// Метод, который обрабатывает кнопки
async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query })
    {
        await AddValueJSON(query.From.Id, UserParam.NumFilter, int.Parse(query.Data));

        if (await GetValueJSON<int>(query.From.Id, UserParam.NumFilter) == 0)
        {
            await AddValueJSON(query.From.Id, UserParam.FlagFilters, true);
            await AddValueJSON(query.From.Id, UserParam.NumFilter, -1);

            await bot.SendMessage(query.Message.Chat, "Приходите ещё :). Напишите /filters для продолжения");
            return;
        }

        WriteLog($"Пользователь с id {query.From.Id} выбрал фильтр №{await GetValueJSON<int>(query.From.Id, UserParam.NumFilter)}");
        await bot.SendMessage(query.Message.Chat, $"Выбран фильтр №{await GetValueJSON<int>(query.From.Id, UserParam.NumFilter)}");
        await bot.SendMessage(query.Message.Chat, $"Отправьте ваше фото");
    }
}

/// Метод, который обрабатывает полученные сообщения
async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text == "/start")
    {
        await InitUserJSON(msg.From.Id);
        await bot.SendMessage(msg.Chat, $"Приветсвую {msg.From.FirstName}! Я вам помогу сделать базовую обработку фото. Чтобы выбрать фильтры напишите /filters");
        WriteLog($"Пользователь с id {msg.ReplyToMessage} написал {msg.Text}");
        return;
    }

    if (msg.Text == "/filters")
    {
        WriteLog($"Пользователь с id {msg.From.Id} написал {msg.Text}");

        await AddValueJSON(msg.From.Id, UserParam.FlagFilters, false);

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] // Первый ряд кнопок
            {
                InlineKeyboardButton.WithCallbackData("1"),
                InlineKeyboardButton.WithCallbackData("2"),
                InlineKeyboardButton.WithCallbackData("3")
            },
            new[] // Второй ряд кнопок
            {
                InlineKeyboardButton.WithCallbackData("4"),
                InlineKeyboardButton.WithCallbackData("5"),
                InlineKeyboardButton.WithCallbackData("6")
            },
            new[] // Третий ряд кнопок
            {
                InlineKeyboardButton.WithCallbackData("7"),
                InlineKeyboardButton.WithCallbackData("8"),
                InlineKeyboardButton.WithCallbackData("9")
            },
            new[] // Четвертый ряд (только кнопка 0)
            {
                InlineKeyboardButton.WithCallbackData("0 - Выход", "0")
            }
        });

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
            "9) Усиление синего канала", replyMarkup: inlineKeyboard);
        return;
    }

    /// Сначала проверяет, что мы писали /filters. Затем проверяет был ли запрос на отправку изображения (или проверяет, что пользователь не выбирал номер фильтра). И наконец проверяет, что сообщение это не текст
    if (await GetValueJSON<bool>(msg.From.Id, UserParam.FlagFilters) ||
        (await GetValueJSON<string>(msg.From.Id, UserParam.ImageName) != "" || await GetValueJSON<int>(msg.From.Id, UserParam.NumFilter) == -1) &&
        msg.Text is not { } message)
    {
        await bot.SendMessage(msg.Chat, "Я вас не понимаю");
        return;
    }

    /// Проверяет, что мы не выбирали номер фильтра
    if (await GetValueJSON<int>(msg.From.Id, UserParam.NumFilter) == -1)
    {
        if (!new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" }.Contains(msg.Text))
        {
            await bot.SendMessage(msg.Chat, "Напишите цифру от 0 до 9");
            return;
        }

        await AddValueJSON(msg.From.Id, UserParam.NumFilter, int.Parse(msg.Text));

        if (await GetValueJSON<int>(msg.From.Id, UserParam.NumFilter) != 0)
        {
            await bot.SendMessage(msg.Chat, $"Выбран фильтр №{await GetValueJSON<int>(msg.From.Id, UserParam.NumFilter)}");
            WriteLog($"Пользователь с id {msg.From.Id} выбрал фильтр №{await GetValueJSON<int>(msg.From.Id, UserParam.NumFilter)}");
        }
    }

    /// Проверяет, что пользователь не отправлял изображение, и что он выбрал номер фильтра
    if (await GetValueJSON<string>(msg.From.Id, UserParam.ImageName) == "" &&
        await GetValueJSON<int>(msg.From.Id, UserParam.NumFilter) != 0)
    {
        if (msg.Photo == null)
        {
            await bot.SendMessage(msg.Chat, "Отправьте ваше фото");
            return;
        }
        WriteLog($"Пользователь с id {msg.From.Id} отправил фото");

        var fileId = msg.Photo.Last().FileId;
        var tgFile = await bot.GetFile(fileId);

        await AddValueJSON(msg.From.Id, UserParam.ImageName, $"image{msg.Chat.Id}_{msg.MessageId}.png");

        await using (var stream = new FileStream(await GetValueJSON<string>(msg.From.Id, UserParam.ImageName), FileMode.Create))
            await bot.DownloadFile(tgFile, stream);

        // Получаем ширину и высоту
        using (var image = SixLabors.ImageSharp.Image.Load(await GetValueJSON<string>(msg.From.Id, UserParam.ImageName)))
        {
            await AddValueJSON(msg.From.Id, UserParam.ImgWidth, image.Width);
            await AddValueJSON(msg.From.Id, UserParam.ImgHeight, image.Height);
        }

        if (await GetValueJSON<int>(msg.From.Id, UserParam.ImgWidth) < 10 ||
            await GetValueJSON<int>(msg.From.Id, UserParam.ImgHeight) < 10)
        {
            await AddValueJSON(msg.From.Id, UserParam.ImageName, "");
            await bot.SendMessage(msg.Chat, "Слишком маленькие размеры изображения");
            await bot.SendMessage(msg.Chat, "Отправьте другое фото");
            return;
        }
    }

    switch (await GetValueJSON<int>(msg.From.Id, UserParam.NumFilter))
    {
        case 0: // Выход
            await AddValueJSON(msg.From.Id, UserParam.FlagFilters, true);
            await AddValueJSON(msg.From.Id, UserParam.NumFilter, -1);

            WriteLog($"Пользователь с id {msg.From.Id} вышел");
            await bot.SendMessage(msg.Chat, "Приходите ещё :). Напишите /filters для продолжения");
            return;

        case 1: // Фильтр, который инвертирует цвета
            await Invert_Colors(bot, msg.Chat, await GetValueJSON<string>(msg.From.Id, UserParam.ImageName));
            break;

        case 2: // Фильтр, который изменяет размер
            if (await GetValueJSON<int>(msg.From.Id, UserParam.ChooseWidth) == -1)
            {
                await AddValueJSON(msg.From.Id, UserParam.ChooseWidth, 0);
                await bot.SendMessage(msg.Chat, $"Введите ширину (от 10 до {await GetValueJSON<int>(msg.From.Id, UserParam.ImgWidth)})");
                return;
            }

            if (!int.TryParse(msg.Text, out int user_input_1) || user_input_1 < 10 ||
                user_input_1 > (await GetValueJSON<int>(msg.From.Id, UserParam.ChooseHeight) == -1 ?
                await GetValueJSON<int>(msg.From.Id, UserParam.ImgWidth) :
                await GetValueJSON<int>(msg.From.Id, UserParam.ImgHeight)))
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от 10 до {(await GetValueJSON<int>(msg.From.Id, UserParam.ChooseHeight) == -1 ?
                    await GetValueJSON<int>(msg.From.Id, UserParam.ImgWidth) :
                    await GetValueJSON<int>(msg.From.Id, UserParam.ImgHeight))}");
                return;
            }

            await AddValueJSON(msg.From.Id, UserParam.UserInput, user_input_1);

            if (await GetValueJSON<int>(msg.From.Id, UserParam.ChooseHeight) == -1)
            {
                WriteLog($"Пользователь с id {msg.From.Id} ввёл ширину {await GetValueJSON<int>(msg.From.Id, UserParam.UserInput)}");
                await AddValueJSON(msg.From.Id, UserParam.ChooseHeight, 0);
                await AddValueJSON(msg.From.Id, UserParam.ChooseWidth, await GetValueJSON<int>(msg.From.Id, UserParam.UserInput));
                await bot.SendMessage(msg.Chat, $"Введите высоту (от 10 до {await GetValueJSON<int>(msg.From.Id, UserParam.ImgHeight)})");
                return;
            }

            WriteLog($"Пользователь с id {msg.From.Id} ввёл высоту {await GetValueJSON<int>(msg.From.Id, UserParam.UserInput)}");
            await AddValueJSON(msg.From.Id, UserParam.ChooseHeight, await GetValueJSON<int>(msg.From.Id, UserParam.UserInput));

            double aspectRatio = (double)await GetValueJSON<int>(msg.From.Id, UserParam.ChooseWidth) / await GetValueJSON<int>(msg.From.Id, UserParam.ChooseHeight);
            if (aspectRatio > 20 || aspectRatio < 0.05)
            {
                await AddValueJSON(msg.From.Id, UserParam.ChooseHeight, -1);
                await bot.SendMessage(msg.Chat, "Недопустимое соотношение сторон для отправки в телеграмм. Попробуйте другие размеры");
                await bot.SendMessage(msg.Chat, $"Введите ширину (от 10 до {await GetValueJSON<int>(msg.From.Id, UserParam.ImgWidth)})");
                return;
            }

            await ChangingSize(bot, msg.Chat,
                await GetValueJSON<string>(msg.From.Id, UserParam.ImageName),
                await GetValueJSON<int>(msg.From.Id, UserParam.ChooseWidth),
                await GetValueJSON<int>(msg.From.Id, UserParam.ChooseHeight));

            await AddValueJSON(msg.From.Id, UserParam.ChooseWidth, -1);
            await AddValueJSON(msg.From.Id, UserParam.ChooseHeight, -1);

            break;

        case 3: // ЧБ фильтр 
            await BWFilter(bot, msg.Chat, await GetValueJSON<string>(msg.From.Id, UserParam.ImageName));
            break;

        case 4: // Фильтр, который увеличивает контраст
            if (await GetValueJSON<int>(msg.From.Id, UserParam.UserInput) == -1)
            {
                await AddValueJSON(msg.From.Id, UserParam.UserInput, 0);
                await bot.SendMessage(msg.Chat, $"Введите интенсивность от -10 до 10 (0 - никаких изменений)");
                return;
            }

            if (!int.TryParse(msg.Text, out int user_input_2) || user_input_2 < -10 || user_input_2 > 10)
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от -10 до 10");
                return;
            }

            await AddValueJSON(msg.From.Id, UserParam.UserInput, user_input_2);
            WriteLog($"Пользователь с id {msg.From.Id} ввёл интенсивность {await GetValueJSON<int>(msg.From.Id, UserParam.UserInput)}");

            await ContrastFilter(bot, msg.Chat,
                await GetValueJSON<string>(msg.From.Id, UserParam.ImageName),
                await GetValueJSON<int>(msg.From.Id, UserParam.UserInput) > 0 ?
                1f + (float)await GetValueJSON<int>(msg.From.Id, UserParam.UserInput) / 10 :
                ((float)await GetValueJSON<int>(msg.From.Id, UserParam.UserInput) + 10) / 10);
            break;

        case 5: // Фильтр, который увеличивает яркость
            if (await GetValueJSON<int>(msg.From.Id, UserParam.UserInput) == -1)
            {
                await AddValueJSON(msg.From.Id, UserParam.UserInput, 0);
                await bot.SendMessage(msg.Chat, $"Введите интенсивность от -10 до 10 (0 - никаких изменений)");
                return;
            }

            if (!int.TryParse(msg.Text, out int user_input_3) || user_input_3 < -10 || user_input_3 > 10)
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от -10 до 10");
                return;
            }

            await AddValueJSON(msg.From.Id, UserParam.UserInput, user_input_3);
            WriteLog($"Пользователь с id {msg.From.Id} ввёл интенсивность {await GetValueJSON<int>(msg.From.Id, UserParam.UserInput)}");

            await BrightnessFilter(bot, msg.Chat,
                await GetValueJSON<string>(msg.From.Id, UserParam.ImageName),
                await GetValueJSON<int>(msg.From.Id, UserParam.UserInput) > 0 ?
                1f + (float)await GetValueJSON<int>(msg.From.Id, UserParam.UserInput) / 10 :
                ((float)await GetValueJSON<int>(msg.From.Id, UserParam.UserInput) + 10) / 10);
            break;

        case 6: // Фильтр, который добавляет виньетку
            await VignetteFilter(bot, msg.Chat, await GetValueJSON<string>(msg.From.Id, UserParam.ImageName));
            break;

        case 7: // Фильтр, который увеличивает красный тон
            if (await GetValueJSON<int>(msg.From.Id, UserParam.UserInput) == -1)
            {
                await AddValueJSON(msg.From.Id, UserParam.UserInput, 0);
                await bot.SendMessage(msg.Chat, $"Введите силу усиления от 0 до 255");
                return;
            }

            if (!int.TryParse(msg.Text, out int user_input_4) || user_input_4 < 0 || user_input_4 > 255)
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от 0 до 255");
                return;
            }

            await AddValueJSON(msg.From.Id, UserParam.UserInput, user_input_4);
            WriteLog($"Пользователь с id {msg.From.Id} ввёл силу усиления {await GetValueJSON<int>(msg.From.Id, UserParam.UserInput)}");

            await RedFilter(bot, msg.Chat, await GetValueJSON<string>(msg.From.Id, UserParam.ImageName), (byte)await GetValueJSON<int>(msg.From.Id, UserParam.UserInput));
            break;

        case 8: // Фильтр, который увеличивает зелёный тон
            if (await GetValueJSON<int>(msg.From.Id, UserParam.UserInput) == -1)
            {
                await AddValueJSON(msg.From.Id, UserParam.UserInput, 0);
                await bot.SendMessage(msg.Chat, $"Введите силу усиления от 0 до 255");
                return;
            }

            if (!int.TryParse(msg.Text, out int user_input_5) || user_input_5 < 0 || user_input_5 > 255)
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от 0 до 255");
                return;
            }

            await AddValueJSON(msg.From.Id, UserParam.UserInput, user_input_5);
            WriteLog($"Пользователь с id {msg.From.Id} ввёл силу усиления {await GetValueJSON<int>(msg.From.Id, UserParam.UserInput)}");

            await GreenFilter(bot, msg.Chat, await GetValueJSON<string>(msg.From.Id, UserParam.ImageName), (byte)await GetValueJSON<int>(msg.From.Id, UserParam.UserInput));
            break;

        case 9: // Фильтр, который увеличивает синий тон
            if (await GetValueJSON<int>(msg.From.Id, UserParam.UserInput) == -1)
            {
                await AddValueJSON(msg.From.Id, UserParam.UserInput, 0);
                await bot.SendMessage(msg.Chat, $"Введите силу усиления от 0 до 255");
                return;
            }

            if (!int.TryParse(msg.Text, out int user_input_6) || user_input_6 < 0 || user_input_6 > 255)
            {
                await bot.SendMessage(msg.Chat, $"Нужно ввести число от 0 до 255");
                return;
            }

            await AddValueJSON(msg.From.Id, UserParam.UserInput, user_input_6);
            WriteLog($"Пользователь с id {msg.From.Id} ввёл силу усиления {await GetValueJSON<int>(msg.From.Id, UserParam.UserInput)}");

            await BlueFilter(bot, msg.Chat, await GetValueJSON<string>(msg.From.Id, UserParam.ImageName), (byte)await GetValueJSON<int>(msg.From.Id, UserParam.UserInput));
            break;
    }

    await AddValueJSON(msg.From.Id, UserParam.UserInput, -1);
    await AddValueJSON(msg.From.Id, UserParam.ImageName, "");
    await bot.SendMessage(msg.Chat, "Снова отправьте фото или выберите другой фильтр /filters");
}
