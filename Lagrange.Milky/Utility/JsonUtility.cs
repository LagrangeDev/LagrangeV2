using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;
using Lagrange.Milky.Api.Handler.Debug;
using Lagrange.Milky.Api.Handler.File;
using Lagrange.Milky.Api.Handler.Friend;
using Lagrange.Milky.Api.Handler.Group;
using Lagrange.Milky.Api.Handler.Message;
using Lagrange.Milky.Api.Handler.System;
using Lagrange.Milky.Api.Result;
using Lagrange.Milky.Entity.Event;

namespace Lagrange.Milky.Utility;

public static partial class JsonUtility
{
    [JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]

    // BotContext
    [JsonSerializable(typeof(BotKeystore))]
    [JsonSerializable(typeof(BotAppInfo))]

    // Signer
    [JsonSerializable(typeof(PcSecSignRequest))]
    [JsonSerializable(typeof(PcSecSignResponse))]
    [JsonSerializable(typeof(PcSecSignResponseValue))]
    [JsonSerializable(typeof(AndroidSecSignRequest))]
    [JsonSerializable(typeof(AndroidEnergyRequest))]
    [JsonSerializable(typeof(AndroidDebugXwidRequest))]
    [JsonSerializable(typeof(AndroidSignerResponse<AndroidSecSignResponseData>))]
    [JsonSerializable(typeof(AndroidSignerResponse<string>))]

    // === api ===
    [JsonSerializable(typeof(ApiOkResult))]
    [JsonSerializable(typeof(ApiFailedResult))]
    // == system ==
    // get_login_info
    [JsonSerializable(typeof(GetLoginInfoResult))]
    // get_impl_info
    [JsonSerializable(typeof(GetImplInfoResult))]
    // get_user_profile
    [JsonSerializable(typeof(GetUserProfileParameter))]
    [JsonSerializable(typeof(GetUserProfileResult))]
    // get_friend_list
    [JsonSerializable(typeof(GetFriendListParameter))]
    [JsonSerializable(typeof(GetFriendListResult))]
    // get_friend_info
    [JsonSerializable(typeof(GetFriendInfoParameter))]
    [JsonSerializable(typeof(GetFriendInfoResult))]
    // get_group_list
    [JsonSerializable(typeof(GetGroupListParameter))]
    [JsonSerializable(typeof(GetGroupListResult))]
    // get_group_info
    [JsonSerializable(typeof(GetGroupInfoParameter))]
    [JsonSerializable(typeof(GetGroupInfoResult))]
    // get_group_member_list
    [JsonSerializable(typeof(GetGroupMemberListParameter))]
    [JsonSerializable(typeof(GetGroupMemberListResult))]
    // get_group_member_info
    [JsonSerializable(typeof(GetGroupMemberInfoParameter))]
    [JsonSerializable(typeof(GetGroupMemberInfoResult))]
    // get_cookies
    [JsonSerializable(typeof(GetCookiesParameter))]
    [JsonSerializable(typeof(GetCookiesResult))]
    // == message ==
    // send_private_message
    [JsonSerializable(typeof(SendPrivateMessageParameter))]
    [JsonSerializable(typeof(SendPrivateMessageResult))]
    // send_group_message
    [JsonSerializable(typeof(SendGroupMessageParameter))]
    [JsonSerializable(typeof(SendGroupMessageResult))]
    // get_message
    [JsonSerializable(typeof(GetMessageParameter))]
    [JsonSerializable(typeof(GetMessageResult))]
    // get_history_messages
    [JsonSerializable(typeof(GetHistoryMessagesParameter))]
    [JsonSerializable(typeof(GetHistoryMessagesResult))]
    // == friend ==
    // send_friend_nudge
    [JsonSerializable(typeof(SendFriendNudgeParameter))]
    // == group ==
    // send_group_nudge
    [JsonSerializable(typeof(SendGroupNudgeParameter))]
    // set_group_name
    [JsonSerializable(typeof(SetGroupNameParameter))]
    // set_group_member_card
    [JsonSerializable(typeof(SetGroupMemberCardParameter))]
    // set_group_member_special_title
    [JsonSerializable(typeof(SetGroupMemberSpecialTitleParameter))]
    // quit_group
    [JsonSerializable(typeof(QuitGroupParameter))]
    // send_group_message_reaction
    [JsonSerializable(typeof(SendGroupMessageReactionParameter))]
    // get_group_notifications
    [JsonSerializable(typeof(GetGroupNotificationsParameter))]
    [JsonSerializable(typeof(GetGroupNotificationsResult))]
    // == file ==
    // upload_group_file
    [JsonSerializable(typeof(UploadGroupFileParameter))]
    [JsonSerializable(typeof(UploadGroupFileResult))]
    // upload_private_file
    [JsonSerializable(typeof(UploadPrivateFileParameter))]
    [JsonSerializable(typeof(UploadPrivateFileResult))]
    // get_group_file_download_url
    [JsonSerializable(typeof(GetGroupFileDownloadUrlParameter))]
    [JsonSerializable(typeof(GetGroupFileDownloadUrlResult))]
    // delete_group_file
    [JsonSerializable(typeof(DeleteGroupFileParameter))]
    // === debug ===
    [JsonSerializable(typeof(SendPacketParameter))]
    [JsonSerializable(typeof(SendPacketResult))]

    // === event ===
    // bot_offline
    [JsonSerializable(typeof(BotOfflineEvent))]
    // message_receive
    [JsonSerializable(typeof(MessageReceiveEvent))]
    // group_nudge
    [JsonSerializable(typeof(GroupNudgeEvent))]
    // group_member_increase
    [JsonSerializable(typeof(GroupMemberIncreaseEvent))]
    // group_member_decrease
    [JsonSerializable(typeof(GroupMemberDecreaseEvent))]
    // friend_request
    [JsonSerializable(typeof(FriendRequestEvent))]
    // group_invitation
    [JsonSerializable(typeof(GroupInvitationEvent))]
    private partial class JsonContext : JsonSerializerContext;

    public static string Serialize<T>(T value) where T : class
    {
        return JsonSerializer.Serialize(value, typeof(T), JsonContext.Default);
    }

    public static byte[] SerializeToUtf8Bytes<T>(T value) where T : class
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, typeof(T), JsonContext.Default);
    }

    public static byte[] SerializeToUtf8Bytes(Type type, object? value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, type, JsonContext.Default);
    }

    public static T? Deserialize<T>(byte[] json) where T : class
    {
        return JsonSerializer.Deserialize(json, typeof(T), JsonContext.Default) as T;
    }
    public static object? Deserialize(Type type, byte[] json)
    {
        return JsonSerializer.Deserialize(json, type, JsonContext.Default);
    }

    public static T? Deserialize<T>(Stream json) where T : class
    {
        return JsonSerializer.Deserialize(json, typeof(T), JsonContext.Default) as T;
    }
}
