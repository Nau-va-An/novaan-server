using System.Text.Json;

namespace Utils.Json
{
    public class CustomJson
    {
        public static string Stringify<T>(T input)
        {
            return JsonSerializer.Serialize<T>(input);
        }

        public static T? Deserialzie<T>(string input)
        {
            return JsonSerializer.Deserialize<T>(input);
        }
    }
}

