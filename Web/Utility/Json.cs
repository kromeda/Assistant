using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;


namespace Assistant.Web
{
    public static class Json
    {
        private static JsonSerializerOptions _jsonOptions;

        private static JsonSerializerOptions JsonOptions
        {
            get
            {
                if (_jsonOptions == null)
                {
                    _jsonOptions = GetOptions();
                }

                return _jsonOptions;
            }
        }

        public static bool TryDeserialize<T>(Stream stream, out T result)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(stream, JsonOptions);
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        public static Task Serialize<T>(Stream utf8Json, T value, CancellationToken cancellationToken)
        {
            return JsonSerializer.SerializeAsync(utf8Json, value, JsonOptions, cancellationToken);
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, options: JsonOptions);
        }

        public static JsonContent GetContent<T>(T value)
        {
            return JsonContent.Create(value, options: JsonOptions);
        }

        public static Task<T> ReadContent<T>(HttpContent content, CancellationToken cancellationToken)
        {
            return content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        }

        private static JsonSerializerOptions GetOptions()
        {
            return new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
            };
        }
    }
}
