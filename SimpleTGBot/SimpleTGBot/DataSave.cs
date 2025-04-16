using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleTGBot
{
    class DataSave  
    {
        public enum UserParam { FlagFilters, NumFilter, ImgWidth, ImgHeight, ChooseWidth, ChooseHeight, UserInput, ImageName }
        private static readonly string FilePath = "users.json";

        /// <summary>
        /// Чтение всех пользователей из JSON
        /// </summary>
        /// <returns></returns>
        private static async Task<Dictionary<long, Person>> ReadUsers()
        {
            if (!File.Exists(FilePath))
                return new();
            return JsonSerializer.Deserialize<Dictionary<long, Person>>(await File.ReadAllTextAsync(FilePath)) ?? new();
        }

        /// <summary>
        /// Сохранение всех пользователей в JSON
        /// </summary>
        /// <param name="users"></param>
        private static async Task SaveUsers(Dictionary<long, Person> users)
        {
            await File.WriteAllTextAsync(FilePath, JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>
        /// Добавление нового пользователя
        /// </summary>
        /// <param name="id"></param>
        public static async Task InitUserJSON(long id)
        {
            var users = await ReadUsers();
            users[id] = new Person();
            await SaveUsers(users);
        }

        /// <summary>
        /// Добавление значения параметра пользователя
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException"></exception>
        public static async Task AddValueJSON<T>(long id, UserParam parameter, T value)
        {
            if (value == null)
                throw new ArgumentException();

            var users = await ReadUsers();
            if (!users.ContainsKey(id))
                users[id] = new Person();
                
            switch (parameter)
            {
                case UserParam.FlagFilters:
                    if (value is bool flag)
                        users[id].FlagFilters = flag;
                    break;
                case UserParam.NumFilter:
                    if (value is int num)
                        users[id].NumFilter = num;
                    break;
                case UserParam.ImgWidth:
                    if (value is int w)
                        users[id].ImgWidth = w;
                    break;
                case UserParam.ImgHeight:
                    if (value is int h)
                        users[id].ImgHeight = h;
                    break;
                case UserParam.ChooseWidth:
                    if (value is int cw)
                        users[id].ChooseWidth = cw;
                    break;
                case UserParam.ChooseHeight:
                    if (value is int ch)
                        users[id].ChooseHeight = ch;
                    break;
                case UserParam.UserInput:
                    if (value is int input)
                        users[id].UserInput = input;
                    break;
                case UserParam.ImageName:
                    if (value is string imgname)
                        users[id].ImageName = imgname;
                    break;
            }

            await SaveUsers(users);
        }

        /// <summary>
        /// Получение значения параметра пользователя
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static async Task<T> GetValueJSON<T>(long id, UserParam parameter)
        {
            var users = await ReadUsers();
            if (!users.ContainsKey(id))
                return default;

            object? result = parameter switch
            {
                UserParam.FlagFilters => users[id].FlagFilters,
                UserParam.NumFilter => users[id].NumFilter,
                UserParam.ImgWidth => users[id].ImgWidth,
                UserParam.ImgHeight => users[id].ImgHeight,
                UserParam.ChooseWidth => users[id].ChooseWidth,
                UserParam.ChooseHeight => users[id].ChooseHeight,
                UserParam.UserInput => users[id].UserInput,
                UserParam.ImageName => users[id].ImageName,
                _ => null
            };

            return result is T resultT ? resultT : default;
        }

        /// <summary>
        /// Удаление пользователя
        /// </summary>
        /// <param name="id"></param>
        public static async Task DeleteUserJSON(long id)
        {
            var users = await ReadUsers();
            if (users.Remove(id))
                await SaveUsers(users);
        }
    }
}
