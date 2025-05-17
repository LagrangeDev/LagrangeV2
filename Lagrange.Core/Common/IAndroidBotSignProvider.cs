using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Common;

public interface IAndroidBotSignProvider : IBotSignProvider
{
    public Task<byte[]> GetEnergy(long uin, string data);

    public Task<byte[]> GetDebugXwid(long uin, string data);
}

internal class DefaultAndroidBotSignProvider(BotContext context) : IAndroidBotSignProvider, IDisposable
{
    private const string Tag = nameof(DefaultAndroidBotSignProvider);
    
    private readonly HttpClient _client = new();

    private readonly string _url = "http://127.0.0.1:8081";
    
    private static readonly HashSet<string> WhiteListCommand =
    [
        "OidbSvcTrpcTcp.0xf88_1", "OidbSvcTrpcTcp.0x1105_1", "oidb_0xf7e_1",
        "trpc.group.long_msg_interface.MsgService.SsoRecvLongMsg", "OidbSvcTrpcTcp.0x92eb_0", "qzoneh5.h5.wnshtml",
        "OidbSvc.0x580_1", "trpc.commercial.dataworks.UserActionReport_sso.SsoReport", "OidbSvcTrpcTcp.0xe37_1200",
        "OidbSvcTrpcTcp.0xf67_5", "OidbSvcTrpcTcp.0x55f_0",
        "MQUpdateSvc_com_qq_qzone_act.web.OidbSvcTrpcJsapiTcp.0x9377_0",
        "trpc.down.intercept.Intercept.SsoGetInterceptFile", "wtlogin.device_lock", "qidianservice.207",
        "SQQzoneSvc.addReply", "ResourceConfig.GetResourceReq", "wtlogin.name2uin",
        "trpc.passwd.manager.PasswdManager.SetPasswd", "trpc.qqva.vipdata.Vipdata.SsoGetUserData",
        "OidbSvc.0x8a1_0", "OidbSvcTrpcTcp.0x101e_2", "OidbSvcTrpcTcp.0xf67_1", "trpc.qpay.sso.web.Do",
        "OidbSvcTrpcTcp.0xf6e_1", "OidbSvcTcp.0x102a", "trpc.o3.ecdh_access.EcdhAccess.SsoSecureA2Access",
        "OidbSvcTrpcTcp.0x88d_0", "OidbSvcTrpcTcp.0x1258_1", "OidbSvcTrpcTcp.0x11ec_1", "OidbSvcTrpcTcp.0x899_9",
        "FeedCloudSvr.trpc.feedcloud.commwriter.ComWriter.DoComment", "SQQzoneSvc.like",
        "trpc.o3.ecdh_access.EcdhAccess.SsoSecureAccess", "OidbSvc.0x787_11", "OidbSvcTrpcTcp.0x1100_1",
        "SQQzoneSvc.Custom.getFacade", "trpc.o3.ecdh_access.EcdhAccess.SsoEstablishShareKey",
        "ProfileService.Pb.ReqSystemMsgAction.Group", "OidbSvcTrpcTcp.0xf57_1", "OidbSvcTrpcTcp.0x12a9_200",
        "wtlogin.exchange_emp", "OidbSvc.0x56c_6", "QChannelSvr.trpc.qchannel.commwriter.ComWriter.PublishFeed",
        "OidbSvcTrpcTcp.0x6d9_4", "trpc.lplan.feed_svr.StatusWrite.SsoPostStatus",
        "QChannelSvr.trpc.qchannel.commwriter.ComWriter.DoComment", "OidbSvc.0x592_15", "OidbSvc.0x592_8",
        "OidbSvc.0x5eb_42261", "SummaryCard.ReqSummaryCard", "QQStranger.InteractiveMsgSvr.SsoSendInterMsg",
        "ProfileService.GroupMngReq", "OidbSvcTrpcTcp.0xf89_1", "OidbSvc.0xe27", "ConnAuthSvr.get_app_info",
        "qidianservice.135", "OidbSvc.0x5eb_96", "OidbSvc.0x592_5", "trpc.group_pro.msgproxy.sendmsg",
        "oidb_0x43c_4", "OidbSvc.0x787_0", "ConnAuthSvr.get_app_info_emp", "OidbSvc.0xb3c_1",
        "QQStranger.login_svr.SsoLoginInfoReport", "FeedCloudSvr.trpc.feedcloud.commwriter.ComWriter.DoBarrage",
        "FeedCloudSvr.trpc.feedcloud.commreader.ComReader.GetMainPageCommData", "QQConnectLogin.get_promote_page",
        "trpc.springfestival.redpacket.LuckyBag.SsoSubmitGrade", "OidbSvc.0xb3c_6", "OidbSvc.0x89b_1",
        "wtlogin.login", "OidbSvcTrpcTcp.0x7c2_5", "oidb_0xcf3_0", "gcbindgroupsso.get_appid",
        "SQQzoneSvc.getVisitorNotify", "qidianservice.290", "trpc.qlive.word_svr.WordSvr.NewPublicChat",
        "wtlogin.register", "OidbSvcTrpcTcp.0xf57_9", "friendlist.GetMultiTroopInfoReq", "StatSvc.GetOnlineStatus",
        "OidbSvcTrpcTcp.0x10db_1", "OidbSvc.0x9fa", "OidbSvcTrpcTcp.0xe37_800", "qidianservice.269",
        "FeedCloudSvr.trpc.feedcloud.commreader.ComReader.GetCommentList", "OidbSvcTrpcTcp.0xf65_1",
        "OidbSvcTrpcTcp.0x9127_1", "QQStranger.FeedPlazaSvr.SsoFeedPublish", "OidbSvc.0xb3c_get_by_id",
        "OidbSvcTrpcTcp.0x11c5_100", "SQQzoneSvc.mobileqboss.get", "GDCTrpcProxy.down",
        "trpc.login.ecdh.EcdhService.SsoQRLoginGenQr", "OidbSvcTrpcTcp.0xf59_2", "OidbSvc.0x592_10",
        "OidbSvcTrpcTcp.0xfe7_3", "OidbSvc.0x592_18", "OidbSvc.0x88d_0", "trpc.qpay.red_pack_skin.Skin.SsoAddSkin",
        "MsgProxy.SendMsg", "OidbSvcTrpcTcp.0x899_1", "trpc.lplan.user_manager_svr.User.SsoSetProfile",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginUnusualDevice",
        "QChannelSvr.trpc.qchannel.commwriter.ComWriter.DoReply", "OidbSvc.0x5eb_43",
        "trpc.passwd.manager.PasswdManager.VerifyPasswd", "OidbSvc.0xb3c_update_5", "OidbSvc.0x592_17",
        "trpc.login.ecdh.EcdhService.SsoQRLoginScanQr", "FeedCloudSvr.trpc.feedcloud.commwriter.ComWriter.DoLike",
        "SQQzoneSvc.getProfileFeeds", "OidbSvc.0x6d6_0", "WalletGestureSvc.GetSignV2", "OidbSvcTrpcTcp.0x88d_14",
        "friendlist.ModifyGroupInfoReq", "OidbSvc.oidb_0x758", "OidbSvcTrpcTcp.0xf57_106",
        "QQConnectLogin.submit_promote_page_emp", "OidbSvcTrpcTcp.0x1107_1", "ProfileService.getGroupInfoReq",
        "OidbSvcTrpcTcp.0x8a1_7", "OidbSvc.0x42d_4", "QQConnectLogin.auth", "OidbSvcTrpcTcp.0x8fc_3",
        "ConnAuthSvr.get_auth_api_list_emp", "QQConnectLogin.auth_emp", "trpc.o3.report.Report.SsoReport",
        "ConnAuthSvr.get_auth_api_list", "PushService.settoken", "trpc.lplan.feed_svr.StatusWrite.SsoComment",
        "trpc.lplan.feed_svr.StatusWrite.SsoReply", "trpc.lplan.like_svr.Like.SsoDoLike", "OidbSvcTrpcTcp.0xf5d_11",
        "miniapp.trpc.minigame.sdk_qgroup_svr.sdk_qgroup_svr.JoinGroup", "QQStranger.FeedSvr.SsoFeedPublish",
        "OidbSvcTrpcTcp.0x9176_1", "OidbSvc.0x592_6", "LightAppSvc.mini_app_userapp.GetDropdownAppList",
        "OidbSvc.0xb3c_add", "OidbSvc.0xb3c_get_undone", "OidbSvc.0x758_0", "OidbSvcTrpcTcp.0x10ed_1",
        "OidbSvc.0xb3c_get_done", "OidbSvc.0xb3c_update", "OidbSvc.0xc26_0", "SQQzoneSvc.GetCate",
        "OidbSvc.0xb3c_delete", "OidbSvc.0xb3c_share",
        "FeedCloudSvr.trpc.videocircle.circleprofile.CircleProfile.SetProfile", "OidbSvc.0xb3c_get_done_count",
        "OidbSvc.0x592_19", "OidbSvc.0xb3c", "OidbSvcTcp.0x88d_0", "OidbSvc.0xb3c_2",
        "trpc.qlive.relationchain_svr.RelationchainSvr.Follow", "OidbSvc.0xe72", "OidbSvc.0xb3c_5",
        "FeedCloudSvr.trpc.feedcloud.commwriter.ComWriter.DoReply", "OidbSvc.0xb3c_4",
        "trpc.qqstranger.common_proxy.CommonProxy.SsoHandle", "QQStranger.UserInfo.SsoSetMiniUserInfo",
        "OidbSvcTrpcTcp.0xfe7_4", "oidb_0xd9c_11", "wtlogin_device.login", "OidbSvc.0x4ff_9", "OidbSvc.0xef0_1",
        "OidbSvc.0xb3c_7", "OidbSvc.0x4ff_9_IMCore", "OidbSvcTrpcTcp.0x1250_0", "OidbSvcTrpcTcp.0xa80_1",
        "OidbSvc.0x89a_0", "OidbSvc.0xb3c_8", "OidbSvcTrpcTcp.0x930e_0", "SQQzoneSvc.get", "OidbSvc.0x592",
        "ConnAuthSvr.sdk_auth_api", "OidbSvc.0xcd5", "oidb_0xcf4_0", "OidbSvcTrpcTcp.0x5cf_11", "OidbSvc.0x899_0",
        "OidbSvc.0x592_2", "OidbSvcTrpcTcp.0xf65_10", "OidbSvcTrpcTcp.0x92e4_0", "OidbSvc.0x592_7",
        "ConnAuthSvr.fast_qq_login", "OidbSvc.0x758", "trpc.QQService.CommonLogic.StatusService.SsoGetLikeList",
        "OidbSvc.0x592_11", "OidbSvc.0x592_12", "wtlogin_device.tran_sim_emp", "MessageSvc.PbGetGroupMsg",
        "OidbSvc.0x8ba", "account.RequestQueryQQMobileContactsV3", "OidbSvcTrpcTcp.0x758_1",
        "FeedCloudSvr.trpc.circlesearch.exhibition.ExhibitionSvr.ExhibitionSuggestion",
        "trpc.msg.msg_svc.MsgService.SsoGetPeerSeq", "SQQzoneSvc.addComment", "WalletGestureSvc.GetPassword",
        "OidbSvc.0x592_3", "OidbSvcTrpcTcp.0xf55_1", "WalletGestureSvc.SetPassword",
        "friendlist.GetTroopMemberList", "OidbSvcTrpcTcp.0xfd4_1", "OidbSvcTrpcTcp.0xfa5_1",
        "OidbSvcTrpcTcp.0x787_1", "trpc.qpay.encryptedtransfer.Encryption.SsoUINEncrypt", "oidb_0xeb1_1",
        "gcbindgroupsso.unbind_group", "oidb_0xdc2_34", "trpc.qlive.qlive_proxy_svr.TrpcProxy.*", "OidbSvc.0x592_4",
        "OidbSvcTrpcTcp.0xfe1_2", "SQQzoneSvc.publishmood", "trpc.down.intercept.Intercept.SsoGetDownloadTips",
        "LightAppSvc.mini_app_cloudstorage.GetUserCloudStorage",
        "trpc.down.joint_operation_game_intercept.JointOperationGameIntercept.SsoQueryConfig",
        "GDCTrpcProxy.service", "SQQzoneSvc.forward", "OidbSvc.0x5eb_cn_switch", "OidbSvcTrpcTcp.0x930d_0",
        "OidbSvc.0x6d7_0", "OidbSvcTrpcTcp.0xcd5", "trpc.o3.ecdh_access.EcdhAccess.SsoSecureA2Establish",
        "OidbSvcTrpcTcp.0x102a_1", "OidbSvcTrpcTcp.0x89a_0", "trpc.msg.register_proxy.RegisterProxy.PushParams",
        "QQConnectLogin.get_promote_page_emp", "QQStranger.UserInfo.SsoGetMiniUserInfo", "oidb_0xfd4_1",
        "QQConnectLogin.submit_promote_page", "oidb_0xe37_800", "OidbSvcTrpcTcp.0x1102_1", "oidb_0x9045_1",
        "oidb_0x7c4_0", "OidbSvcTrpcTcp.0xfe5_2", "oidb_0x7a2_0", "oidb_0x42e_3",
        "friendlist.GetTroopAppointRemarkReq", "trpc.lplan.map_svr.Map.SsoReportLocation",
        "OidbSvcTrpcTcp.0x125b_500", "friendlist.GetLastLoginInfoReq", "friendlist.GetFriendListReq",
        "oidb_0xe37_1200", "OidbSvcTrpcTcp.0x9124_0", "VisitorSvc.ReqFavorite", "SQQzoneSvc.get_all_feedsphoto_ex",
        "StatSvc.register", "SQQzoneSvc.getPhotoComment", "oidb_0x7df_3", "SQQzoneSvc.getActiveFeeds",
        "SQQzoneSvc.preGetPassiveFeeds", "SQQzoneSvc.getAIONewestFeeds", "ResourceConfig.ClientReq",
        "friendlist.getFriendGroupList", "SQQzoneSvc.photo", "OidbSvc.0xc33_42220",
        "trpc.qmeta.mob_proxy_svr.MobProxy.SsoHandle", "OidbSvcTrpcTcp.0x102a_0",
        "QQStranger.UserInfo.SsoBatchGetMiniUserInfo", "ProfileService.SimpleInfo", "PbMessageSvc.PbUnReadMsgSeq",
        "OidbSvc_device.0x9f5", "OidbSvcTrpcTcp.0xf5d_1", "OidbSvcTcp.0xef0_1", "OidbSvcTrpcTcp.0xf5b_1",
        "OidbSvcTrpcTcp.0xe37_1700", "OidbSvcTrpcTcp.0x972_6", "OidbSvc.0x758_1",
        "trpc.g_qqrtc.qq_mav_room_state_read.GetRoomState.SsoGetInfoByUin", "OidbSvcTrpcTcp.0x901f_1",
        "OidbSvc.0x5eb_99", "OidbSvcTrpcTcp.0x899_0", "OidbSvc.0x6d9_4", "OidbSvc.0x787_1",
        "OidbSvcTrpcTcp.0x88d_111", "friendlist.addFriend", "OidbSvcTrpcTcp.0x6d9_2", "OidbSvcTrpcTcp.0x6d6_3",
        "OidbSvcTrpcTcp.0x127a_0", "oidb_0xe61_1", "OidbSvc.0x88d_1_2", "OidbSvc.0x592_1",
        "OidbSvcTrpcTcp.0x126e_200", "FeedCloudSvr.trpc.feedcloud.commreader.ComReader.GetFeedList",
        "OidbSvc.0xe61", "OidbSvcTrpcTcp.0x6d6_2", "OidbSvc.0x53c_2", "SQQzoneSvc.shuoshuo",
        "OidbSvcTrpcTcp.0x126d_200", "OidbSvcTrpcTcp.0x1225_0", "OidbSvcTrpcTcp.0x11e9_200", "OidbSvc.0x852_48",
        "OidbSvc.0xbcb", "OidbSvc.0x42d", "OidbSvc.0x515_2", "OidbSvcTrpcTcp.0x11c5_200",
        "OidbSvcTrpcTcp.0x11c4_200", "trpc.qqhb.qqhb_proxy.Handler.sso_handle", "OidbSvcTrpcTcp.0x11c4_100",
        "OidbSvc.0x852_35", "OidbSvcTrpcTcp.0x1194_1", "OidbSvcTrpcTcp.0x10c0_1", "OidbSvc.0x88d_7",
        "OidbSvcTrpcTcp.0x110d_1", "OidbSvcTrpcTcp.0x10f4_1", "friendlist.AddFriendReq", "OidbSvcTrpcTcp.0xfe1",
        "OidbSvcTrpcTcp.0x902e_1", "OidbSvcTrpcTcp.0x10c0_2", "QSec.AVEng", "SecuritySvc.GetConfig",
        "FeedCloudSvr.trpc.feedcloud.commwriter.ComWriter.DoFollow", "group_member_card.get_group_member_card_info",
        "OidbSvc.0x570_8", "OidbSvc.0xe63_1", "OidbSvcTrpcTcp.0x8f9_14", "OidbSvc.0x7df_3",
        "FeedCloudSvr.trpc.feedcloud.commreader.ComReader.GetReplyList", "OidbSvc.0x58b", "OidbSvcTrpcTcp.0x9a2_12",
        "OidbSvcTrpcTcp.0xfe7_2", "OidbSvc.0xe3e", "OidbSvc.0x592_16",
        "CertifiedAccountSvc.certified_account_read.GetFollowList",
        "QChannelSvr.trpc.qchannel.commreader.ComReader.BatchGetFeedDetail", "oidb_0x5d0_1", "OidbSvc.0xf7e_1",
        "OidbSvcTrpcTcp.0x644_1", "oidb_0x43c_5", "OidbSvc.0xbe8", "OidbSvcTrpcTcp.0xfe1_8",
        "OidbSvcTrpcTcp.0x101e_1", "OidbSvc.0x5d0_1", "OidbSvc.0x8a1_7", "MessageSvc.PbGetRoamMsg", "oidb_0xe8c_0",
        "OidbSvcTrpcTcp.0x10d8_1", "OidbSvc.0x8a0_0", "OidbSvcTrpcTcp.0xe37_700", "OidbSvc.0x587_normalNightSet",
        "OidbSvc.0x899_9", "OidbSvc.0x587_123", "QQRTCSvc.RoomManager-GetRoomInfo", "friendlist.GetTroopListReqV2",
        "OidbSvc.0x592_13", "MessageSvc.PbSendMsg", "wtlogin.qrlogin", "oidb_0x9072_0", "OidbSvc.0x592_9",
        "OidbSvcTrpcTcp.0x11ea_200", "FeedCloudSvr.trpc.feedcloud.commwriter.ComWriter.PublishFeed",
        "OidbSvc.0x7a2_0", "OidbSvc.0x8a4", "OidbSvc.0x88d_1", "oidb_0x5d6_21", "oidb_0x5d6_19", "OidbSvc.0x8f1",
        "OidbSvcTrpcTcp.0x11e9_100", "OidbSvcTrpcTcp.0x1017_1", "OidbSvc.0xb3c_3",
        "trpc.qpay.midas_order.Order.SsoMakeOrder", "OidbSvcTrpcTcp.0x116c_1", "OidbSvc.0x5eb_ForTheme",
        "SQQzoneSvc.getPhotoWall", "OidbSvc.0x5eb_22", "OidbSvc.0x5eb_42073", "ConnAuthSvr.sdk_auth_api_emp",
        "FeedCloudSvr.trpc.feedcloud.commreader.ComReader.GetRelationGroupList", "NowSummaryCard.NearbyMiniCardReq",
        "OidbSvc.0x5eb_15", "OidbSvcTrpcTcp.0x116d_1", "FeedCloudSvr.trpc.feedcloud.commwriter.ComWriter.DoPush",
        "group_member_statistic.get_group_member_statistic", "SQQzoneSvc.getMainPage", "OidbSvc.0x6d6_3",
        "OidbSvcTrpcTcp.0x1224_0", "QQConnectLogin.pre_auth", "GameCenterMsg.GetUserInfo", "MessageSvc.PbGetMsg",
        "FeedCloudSvr.trpc.feedcloud.commreader.ComReader.GetBusiInfo", "OidbSvc.0xc6b",
        "friendlist.GetTroopMemberListReq", "OidbSvc.0x6d6_2", "friendlist.getTroopMemberList",
        "trpc.down.joint_operation_game_intercept.JointOperationGameIntercept.SsoCheck",
        "SsoSnsSession.Cmd0x3_SubCmd0x1_FuncGetBlockList", "OidbSvcTrpcTcp.0x1262_19", "OidbSvcTrpcTcp.0x105b_1",
        "friendlist.GetSimpleOnlineFriendInfoReq", "OidbSvc.0xbbb", "OidbSvcTrpcTcp.0x1130_1",
        "OidbSvc_device.0x633", "FeedCloudSvr.trpc.feedcloud.commreader.ComReader.GetRerankedFeedList",
        "SQQzoneSvc.getProfile", "QQConnectLogin.pre_auth_emp", "OidbSvc.0xcdd", "MessageSvc.PbBindUinGetMsg",
        "OidbSvc.0x480_9", "AvatarInfoSvr.QQHeadUrlReq", "OidbSvc.0x480_9_IMCore", "OidbSvcTrpcTcp.0x10c8_1",
        "OidbSvcTcp.0x88d_1", "OidbSvcTrpcTcp.0x1289_1", "LightAppSvc.mini_app_privacy.GetPrivacyInfo",
        "FeedCloudSvr.trpc.feedcloud.commreader.ComReader.GetMainPageBasicData", "OidbSvc.0x5eb_common",
        "OidbSvcTrpcTcp.0xaf6_0", "IncreaseURLSvr.QQHeadUrlReq"
    ];
    
    public bool IsWhiteListCommand(string cmd) => WhiteListCommand.Contains(cmd);
    
    public async Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
    {
        try
        {
            var payload = new JsonObject
            {
                ["uin"] = uin,
                ["cmd"] = cmd,
                ["seq"] = seq,
                ["buffer"] = Convert.ToHexString(body.Span),
                ["guid"] = Convert.ToHexString(context.Keystore.Guid),
                ["version"] = context.AppInfo.PtVersion
            };
            
            var response = await _client.PostAsync($"{_url}/sign", new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode) return null;
            
            var content = JsonHelper.Deserialize<ResponseRoot<SignResponse>>(await response.Content.ReadAsStringAsync());
            if (content == null) return null;
            
            return new SsoSecureInfo
            {
                SecSign = Convert.FromHexString(content.Value.Sign),
                SecToken = Convert.FromHexString(content.Value.Token), 
                SecExtra = Convert.FromHexString(content.Value.Extra)
            };
        }
        catch (Exception e)
        {
            context.LogWarning(Tag, "Failed to get sign: {0}", e.Message);
            return null;
        }
    }

    public async Task<byte[]> GetEnergy(long uin, string data)
    {
        try
        {
            var payload = new JsonObject
            {
                ["uin"] = uin,
                ["data"] = data,
                ["guid"] = Convert.ToHexString(context.Keystore.Guid),
                ["ver"] = context.AppInfo.SdkInfo.SdkVersion,
                ["version"] = context.AppInfo.PtVersion
            };
            
            var response = await _client.PostAsync($"{_url}/energy", new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode) return [];
            
            var content = JsonHelper.Deserialize<ResponseRoot<string>>(await response.Content.ReadAsStringAsync());
            return content == null ? [] : Convert.FromHexString(content.Value);
        }
        catch (Exception e)
        {
            context.LogWarning(Tag, "Failed to get energy: {0}", e.Message);
            return [];
        }
    }

    public async Task<byte[]> GetDebugXwid(long uin, string data)
    {
        try
        {
            var payload = new JsonObject
            {
                ["uin"] = uin,
                ["data"] = data,
                ["guid"] = Convert.ToHexString(context.Keystore.Guid),
                ["version"] = context.AppInfo.PtVersion
            };
            
            var response = await _client.PostAsync($"{_url}/get_tlv553", new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode) return [];
            
            var content = JsonHelper.Deserialize<ResponseRoot<string>>(await response.Content.ReadAsStringAsync());
            return content == null ? [] : Convert.FromHexString(content.Value);
        }
        catch (Exception e)
        {
            context.LogWarning(Tag, "Failed to get debug_xwid: {0}", e.Message);
            return [];
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [Serializable]
    internal class ResponseRoot<T>
    {
        [JsonPropertyName("data")] public T Value { get; set; } = default!;
    }
    
    [Serializable]
    internal class SignResponse
    {
        [JsonPropertyName("sign")] public string Sign { get; set; } = string.Empty;
        
        [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
        
        [JsonPropertyName("extra")] public string Extra { get; set; } = string.Empty;
    }
}