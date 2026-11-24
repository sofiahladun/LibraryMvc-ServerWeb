using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryMvc.Extensions
{
    public static class SessionExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Зберігає об'єкт у сесії у форматі JSON.
        /// </summary>
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            session.SetString(key, json);
        }

        /// <summary>
        /// Зчитує об'єкт із сесії або повертає default, якщо ключ відсутній.
        /// </summary>
        public static T? GetObject<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            return string.IsNullOrEmpty(json)
                ? default
                : JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        /// <summary>
        /// Перевіряє наявність збережених даних у сесії за ключем.
        /// </summary>
        public static bool HasKey(this ISession session, string key)
        {
            return !string.IsNullOrEmpty(session.GetString(key));
        }
    }
}