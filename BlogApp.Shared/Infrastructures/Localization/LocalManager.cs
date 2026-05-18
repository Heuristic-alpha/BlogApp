using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;

namespace BlogApp.Infrastructures.Localization
{
    public class LocalManager
    {
        public const string FileName = "Localization.json";
        public const string LocalizationFileURL = $"{Constants.HostAddress}/{FileName}";

        private string _serverLocalizationFilePath;
        private Dictionary<string, Payload> _dictionary;
        private JsonSerializerOptions _jsonSerializerOptions;
        private object _dictionaryLock;

        // Base Ctor
        public LocalManager(ILogger<LocalManager> logger)
        {
            _serverLocalizationFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", FileName);
            _dictionary = new Dictionary<string, Payload>();
            _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
            {
                WriteIndented = true,
            };
            _dictionaryLock = new object();

            logger.LogInformation($"ServerLocalizationFile Path is at [{_serverLocalizationFilePath}]");
        }

        public async Task SaveAsync()
        {
            await SaveToFileAsync(_serverLocalizationFilePath);
        }

        public async Task LoadAsync()
        {
            await LoadFromFileAsync(_serverLocalizationFilePath);
        }

        public async Task SaveToFileAsync(string path)
        {
            IEnumerable<Payload> payloads = _dictionary.Values;
            string json = JsonSerializer.Serialize(payloads, _jsonSerializerOptions);
            json = System.Text.RegularExpressions.Regex.Unescape(json);
            using FileStream fs = File.Create(path);
            using TextWriter tw = new StreamWriter(fs);
            await tw.WriteAsync(json);
        }

        public async Task LoadFromFileAsync(string path)
        {
            if (!File.Exists(path)) throw new DirectoryNotFoundException($"LoadFromFileAsync: cant found any directory at [{path}] to load data.");

            using FileStream fs = File.OpenRead(path);
            using TextReader tr = new StreamReader(fs);
            string json = await tr.ReadToEndAsync();

            await LoadFromJsonAsync(json);
        }

        public async Task LoadFromJsonAsync(string jsonPayload)
        {
            await Task.Run(() =>
            {
                Monitor.Enter(_dictionaryLock);
                LoadFromJson(jsonPayload);
                Monitor.Exit(_dictionaryLock);
            });
        }
        public void LoadFromJson(string jsonPayload)
        {
            IEnumerable<Payload> payloads = JsonSerializer.Deserialize<IEnumerable<Payload>>(jsonPayload, _jsonSerializerOptions) ?? throw new JsonException($"LoadFromJson: cant deserialize payload");
            _dictionary.Clear();
            foreach (Payload item in payloads)
            {
                // use ENG as key
                _dictionary[item.ENG] = item;
            }
        }

        public string? GetLocal(string key, Language language)
        {
            if (_dictionary.TryGetValue(key, out Payload payload))
            {
                return language switch
                {
                    Language.ENG => payload.ENG,
                    Language.FA => payload.FA,

                    _ => payload.ENG,
                };
            }
            return null;
        }

        public string GetLocalOrKey(string key, Language language)
        {
            string? value = GetLocal(key, language);
            if (value != null) return value;
            else return key;
        }

        /// <summary>
        /// Get direction for provided language ("rtl" or "ltr")
        /// </summary>
        /// <param name="language"></param>
        /// <returns>("rtl" or "ltr")</returns>
        public static string GetDirection(Language language)
        {
            switch (language)
            {
                case Language.ENG: return "ltr";
                case Language.FA: return "rtl";

                default:
                    goto case Language.ENG;

            }
        }

        private void InitDefaultValues()
        {
            _dictionary.Clear();
            _dictionary.Add("BlogApp", new Payload { ENG = "BlogApp", FA = "بلاگ اپ" });
            _dictionary.Add("UserName", new Payload { ENG = "UserName", FA = "نام کاربری" });
            _dictionary.Add("Email", new Payload { ENG = "Email", FA = "ایمیل" });
        }
    }
}
