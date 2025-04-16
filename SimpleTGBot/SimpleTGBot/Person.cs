namespace SimpleTGBot
{
    /// <summary>
    /// Вспомогательный класс, который хранит стартовые значения всех параметров пользователя
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Флаг на срабатывание /filters
        /// </summary>
        public bool FlagFilters { get; set; } = true;

        /// <summary>
        /// Номер фильтра
        /// </summary>
        public int NumFilter { get; set; } = -1;

        /// <summary>
        /// Ширина изображения (нужен для фильтра "Изменение размера")
        /// </summary>
        public int ImgWidth { get; set; } = 0;

        /// <summary>
        /// Высота изображения (нужен для фильтра "Изменение размера")
        /// </summary>
        public int ImgHeight { get; set; } = 0;

        /// <summary>
        /// Выбранная пользователем ширина изображения (нужен для фильтра "Изменение размера")
        /// </summary>
        public int ChooseWidth { get; set; } = -1;

        /// <summary>
        /// Выбранная пользователем высота изображения (нужен для фильтра "Изменение размера")
        /// </summary>
        public int ChooseHeight { get; set; } = -1;

        /// <summary>
        /// Ответ пользователя
        /// </summary>
        public int UserInput { get; set; } = -1;

        /// <summary>
        /// Имя сохранённого изображения
        /// </summary>
        public string ImageName { get; set; } = "";
    }
}
