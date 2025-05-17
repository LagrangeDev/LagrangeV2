using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Milky.Core;
using Lagrange.Milky.Implementation.Api.System;
using Lagrange.Milky.Implementation.Common.Api.Params;
using Lagrange.Milky.Implementation.Common.Api.Results;
using Lagrange.Milky.Implementation.Entity;

namespace Lagrange.Milky.Utility;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = [typeof(ByteArrayConverter)])]

[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonObject))]

[JsonSerializable(typeof(BotKeystore))]
[JsonSerializable(typeof(BotQrCodeInfo))]

[JsonSerializable(typeof(PcSignerRequest))]
[JsonSerializable(typeof(PcSignerResponse))]
[JsonSerializable(typeof(AndroidSignerRequest))]

// Api Handler Param
[JsonSerializable(typeof(EmptyParam))]
[JsonSerializable(typeof(CachedParam))]
[JsonSerializable(typeof(GetFriendInfoParam))]
[JsonSerializable(typeof(GetGroupInfoParam))]
[JsonSerializable(typeof(GetGroupMemberListParam))]
[JsonSerializable(typeof(GetGroupMemberInfoParam))]
// Api Handler Result
[JsonSerializable(typeof(ApiFailedResult))]
[JsonSerializable(typeof(ApiOkResult<GetLoginInfoResult>))]
[JsonSerializable(typeof(ApiOkResult<IEnumerable<Friend>>))]
[JsonSerializable(typeof(ApiOkResult<Friend>))]
[JsonSerializable(typeof(ApiOkResult<IEnumerable<Group>>))]
[JsonSerializable(typeof(ApiOkResult<Group>))]
[JsonSerializable(typeof(ApiOkResult<IEnumerable<GroupMember>>))]
[JsonSerializable(typeof(ApiOkResult<GroupMember>))]
public partial class MilkyJsonContext : JsonSerializerContext;

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

public static class JsonHelper
{
    public static T? Deserialize<T>(string json) where T : class =>
        JsonSerializer.Deserialize(json, typeof(T), MilkyJsonContext.Default) as T;

    public static object? Deserialize(Type type, string json) =>
        JsonSerializer.Deserialize(json, type, MilkyJsonContext.Default);

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, typeof(T), MilkyJsonContext.Default);

    public static byte[] SerializeToUtf8Bytes<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, typeof(T), MilkyJsonContext.Default);

    public static byte[] SerializeToUtf8Bytes(Type type, object value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, type, MilkyJsonContext.Default);
}