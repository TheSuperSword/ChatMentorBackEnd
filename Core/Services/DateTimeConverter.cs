using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatMentor.Backend.Core.Services;

public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            if (DateTime.TryParse(reader.GetString(), out var result))
                return result;

        return DateTime.MinValue; // Handle default value or throw an exception as needed
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}