using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;
using Lagrange.Core.Internal.Packets.Login;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Utility;

internal static partial class JsonHelper
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    
    [JsonSerializable(typeof(NTNewDeviceQrCodeRequest))]
    [JsonSerializable(typeof(NTNewDeviceQrCodeResponse))]
    [JsonSerializable(typeof(NTNewDeviceQrCodeQuery))]
    
    [JsonSerializable(typeof(DefaultAndroidBotSignProvider.ResponseRoot<DefaultAndroidBotSignProvider.SignResponse>))]
    [JsonSerializable(typeof(DefaultAndroidBotSignProvider.ResponseRoot<string>))]
    [JsonSerializable(typeof(DefaultBotSignProvider.Root))]
    [JsonSerializable(typeof(DefaultBotSignProvider.Response))]
    
    [JsonSerializable(typeof(JsonObject))]
    [JsonSerializable(typeof(MsgPush))]
    [JsonSerializable(typeof(LightApp))]
    private partial class CoreSerializerContext : JsonSerializerContext;
    
    public static T? Deserialize<T>(string json) where T : class => 
        JsonSerializer.Deserialize(json, typeof(T), CoreSerializerContext.Default) as T;
    
    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, typeof(T), CoreSerializerContext.Default);
    
    public static ReadOnlyMemory<byte> SerializeToUtf8Bytes<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, typeof(T), CoreSerializerContext.Default);
}