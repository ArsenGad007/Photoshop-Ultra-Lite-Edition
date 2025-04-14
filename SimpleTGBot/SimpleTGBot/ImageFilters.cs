using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SimpleTGBot
{
    class ImageFilters
    {
        public static async Task Invert_Colors(TelegramBotClient bot, Chat chatId, string inputPath, string outputPath)
        {
            using (Image image = Image.Load(inputPath))
            {
                image.Mutate(x => x.Invert());
                image.SaveAsPng(outputPath);
            }

            await using (var outputStream = File.OpenRead(outputPath))
            {
                await bot.SendPhoto(chatId, outputStream);
            }

            // Очистка временных файлов
            File.Delete(inputPath);
            File.Delete(outputPath);
        }

    }
}
