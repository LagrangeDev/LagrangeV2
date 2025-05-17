using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Api.System;
using Lagrange.Milky.Implementation.Common.Api.Params;
using Lagrange.Milky.Implementation.Common.Api.Results;
using Lagrange.Milky.Implementation.Entity;

namespace Lagrange.Milky.Implementation.Common;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]

// Raw JSON Types
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonObject))]

// The following types are used in API

[JsonSerializable(typeof(EmptyParam))]
[JsonSerializable(typeof(CachedParam))]
[JsonSerializable(typeof(ApiFailedResult))]

// /get_login_info
// [no input parameters]
[JsonSerializable(typeof(ApiOkResult<GetLoginInfoResult>))]

// /get_friend_list
// [no input parameters]
[JsonSerializable(typeof(ApiOkResult<IEnumerable<Friend>>))]

// /get_friend_info
[JsonSerializable(typeof(GetFriendInfoParam))]
[JsonSerializable(typeof(ApiOkResult<Friend>))]

// /get_group_list
// [no input parameters]
[JsonSerializable(typeof(ApiOkResult<IEnumerable<Group>>))]

// /get_group_info
[JsonSerializable(typeof(GetGroupInfoParam))]
[JsonSerializable(typeof(ApiOkResult<Group>))]

// /get_group_member_list
[JsonSerializable(typeof(GetGroupMemberListParam))]
[JsonSerializable(typeof(ApiOkResult<IEnumerable<GroupMember>>))]

// /get_group_member_info
[JsonSerializable(typeof(GetGroupMemberInfoParam))]
[JsonSerializable(typeof(ApiOkResult<GroupMember>))]
public partial class MilkyJsonContext : JsonSerializerContext;

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