using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;

namespace Lagrange.Milky.Core;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = [typeof(ByteArrayConverter)])]

[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonObject))]

[JsonSerializable(typeof(BotKeystore))]
[JsonSerializable(typeof(BotQrCodeInfo))]

[JsonSerializable(typeof(PcSignerRequest))]
[JsonSerializable(typeof(PcSignerResponse))]
[JsonSerializable(typeof(AndroidSignerRequest))]

public partial class CoreJsonContext : JsonSerializerContext;

public class ByteArrayConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return Convert.FromHexString(reader.GetString() ?? string.Empty);
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Convert.ToHexString(value));
    }
}