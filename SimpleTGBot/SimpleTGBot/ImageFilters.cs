using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SimpleTGBot
{
    class ImageFilters
    {
        /// <summary>
        /// Инвертирует цвета
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="chatId"></param>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static async Task Invert_Colors(TelegramBotClient bot, Chat chatId, string imagePath)
        {
            using (Image image = Image.Load(imagePath))
            {
                image.Mutate(x => x.Invert());
                image.Save(imagePath);
            }

            await using (var outputStream = File.OpenRead(imagePath))
                await bot.SendPhoto(chatId, outputStream);

            // Очистка временных файлов
            File.Delete(imagePath);
        }

        /// <summary>
        /// Меняет размер изображения
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="chatId"></param>
        /// <param name="imagePath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static async Task ChangingSize(TelegramBotClient bot, Chat chatId, string imagePath, int width, int height)
        {
            using (Image image = Image.Load(imagePath))
            {
                image.Mutate(x => x.Resize(width, height));
                image.SaveAsPng(imagePath);
            }

            await using (var outputStream = File.OpenRead(imagePath))
                await bot.SendPhoto(chatId, outputStream);

            // Очистка временных файлов
            File.Delete(imagePath);
        }

        /// <summary>
        /// Чёрно белый фильтр
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="chatId"></param>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static async Task BWFilter(TelegramBotClient bot, Chat chatId, string imagePath)
        {
            using (Image image = Image.Load(imagePath))
            {
                image.Mutate(x => x.Grayscale());
                image.SaveAsPng(imagePath);
            }
           
            await using (var outputStream = File.OpenRead(imagePath))
                await bot.SendPhoto(chatId, outputStream);

            // Очистка временных файлов
            File.Delete(imagePath);
        }

        /// <summary>
        /// Увеличение контраста
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="chatId"></param>
        /// <param name="imagePath"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static async Task ContrastFilter(TelegramBotClient bot, Chat chatId, string imagePath, float power)
        {
            using (Image image = Image.Load(imagePath))
            {
                image.Mutate(x => x.Contrast(power));
                image.SaveAsPng(imagePath);
            }

            await using (var outputStream = File.OpenRead(imagePath))
                await bot.SendPhoto(chatId, outputStream);

            // Очистка временных файлов
            File.Delete(imagePath);
        }

        /// <summary>
        /// Увеличение яркости
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="chatId"></param>
        /// <param name="imagePath"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static async Task BrightnessFilter(TelegramBotClient bot, Chat chatId, string imagePath, float power)
        {
            using (Image image = Image.Load(imagePath))
            {
                image.Mutate(x => x.Brightness(power));
                image.SaveAsPng(imagePath);
            }

            await using (var outputStream = File.OpenRead(imagePath))
                await bot.SendPhoto(chatId, outputStream);

            // Очистка временных файлов
            File.Delete(imagePath);
        }

        /// <summary>
        /// Добавить виньетку
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="chatId"></param>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static async Task VignetteFilter(TelegramBotClient bot, Chat chatId, string imagePath)
        {
            using (Image image = Image.Load(imagePath))
            {
                image.Mutate(x => x.Vignette());
                image.SaveAsPng(imagePath);
            }

            await using (var outputStream = File.OpenRead(imagePath))
                await bot.SendPhoto(chatId, outputStream);

            // Очистка временных файлов
            File.Delete(imagePath);
        }

        /// <summary>
        /// Усиление красного канала
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="chatId"></param>
        /// <param name="imagePath"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static async Task RedFilter(TelegramBotClient bot, Chat chatId, string imagePath, byte power)
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(imagePath))
            {
                for (int y = 0; y < image.Height; y++)
                    for (int x = 0; x < image.Width; x++)
                    {
                        Rgba32 pixel = image[x, y];
                        image[x, y] = new Rgba32((byte)Math.Min(pixel.R + power, 255), pixel.G, pixel.B, pixel.A);
                    }

                image.SaveAsPng(imagePath);
            }

            await using (var outputStream = File.OpenRead(imagePath))
                await bot.SendPhoto(chatId, outputStream);

            // Очистка временных файлов
            File.Delete(imagePath);
        }

        /// <summary>
        /// Усиление зелёного канала
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="chatId"></param>
        /// <param name="imagePath"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static async Task GreenFilter(TelegramBotClient bot, Chat chatId, string imagePath, byte power)
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(imagePath))
            {
                for (int y = 0; y < image.Height; y++)
                    for (int x = 0; x < image.Width; x++)
                    {
                        Rgba32 pixel = image[x, y];
                        image[x, y] = new Rgba32(pixel.R, (byte)Math.Min(pixel.G + power, 255), pixel.B, pixel.A);
                    }

                image.SaveAsPng(imagePath);
            }

            await using (var outputStream = File.OpenRead(imagePath))
                await bot.SendPhoto(chatId, outputStream);

            // Очистка временных файлов
            File.Delete(imagePath);
        }

        /// <summary>
        /// Усиление синего канала
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="chatId"></param>
        /// <param name="imagePath"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static async Task BlueFilter(TelegramBotClient bot, Chat chatId, string imagePath, byte power)
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(imagePath))
            {
                for (int y = 0; y < image.Height; y++)
                    for (int x = 0; x < image.Width; x++)
                    {
                        Rgba32 pixel = image[x, y];
                        image[x, y] = new Rgba32(pixel.R, pixel.G, (byte)Math.Min(pixel.B + power, 255), pixel.A);
                    }

                image.SaveAsPng(imagePath);
            }

            await using (var outputStream = File.OpenRead(imagePath))
                await bot.SendPhoto(chatId, outputStream);

            // Очистка временных файлов
            File.Delete(imagePath);
        }
    }
}
