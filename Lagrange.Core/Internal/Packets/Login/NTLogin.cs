using Lagrange.Proto;

#pragma warning disable CS8618

namespace Lagrange.Core.Internal.Packets.Login;

#region Enum

internal enum NTLoginPlatform {
    PLATFORM_UNKNOWN = 0,
    PLATFORM_IOS = 1,
    PLATFORM_ANDROID = 2,
    PLATFROM_SYMBIAN = 3,
    PLATFORM_WINDOWS = 4,
    PLATFORM_MAC = 5,
    PLATFORM_IPAD = 6,
    PLATFORM_LINUX = 7,
    PLATFORM_HARMONY = 8
}

internal enum NTLoginRetCode {
    SUCCESS_UNSPECIFIED = 0,
    ERR_DEFAULT = 140022000,
    ERR_INVALID_PARAMETER = 140022001,
    ERR_SYSTEM_FAILED = 140022002,
    ERR_TIMEOUT_RETRY = 140022003,
    ERR_NEED_UPGRADE = 140022004,
    ERR_BEEN_FORBIDEN = 140022005,
    ERR_STOLEN_PROTECT = 140022006,
    ERR_DIFF_STRICK = 140022007,
    ERR_NEED_VERIFY_WATERPROOF_WALL = 140022008,
    ERR_REFUSE_PASSOWRD_LOGIN = 140022009,
    ERR_NEED_VERIFY_NEW_DEVICE = 140022010,
    ERR_NEED_VERIFY_UNUSUAL_DEVICE = 140022011,
    ERR_INVALID_COOKIE = 140022012,
    ERR_ACCOUNT_OR_PASSWORD_ERROR = 140022013,
    ERR_EXPIRE_TICKET = 140022014,
    ERR_KICKED_TICKET = 140022015,
    ERR_ILLAGE_TICKET = 140022016,
    ERR_SEC_BEAT = 140022017,
    ERR_ACCOUNT_NOT_UIN = 140022018,
    ERR_NEED_VERIFY_REAL_NAME = 140022019,
    ERR_NICE_ACCOUNT_EXPIRED = 150022020,
    ERR_BLACK_ACCOUNT = 150022021,
    ERR_SMS_TOO_OFTEN = 150022022,
    ERR_SMS_TOO_MANY_TIMES_TODAY = 150022023,
    ERR_PHONE_UNREGISTERED = 150022024,
    ERR_NICE_ACCOUNT_PARENT_CHILD_EXPIRED = 150022025,
    ERR_SMS_INVALID = 150022026,
    ERR_TGTGT_EXCHG_A1_FORBID = 150022027,
    ERR_REMIND_CANCELLATED_STATUS = 150022028,
    ERR_MUTIPLE_PASSWORD_INCORRECT = 150022029,
    ERR_ILLEGAL_ACCOUNT = 150022030,
    ERR_INTERFACE_UNAVAILABLE = 150022031,
    ERR_PASSWORD_MODIFIED_DURING_LOGIN = 150022032,
    ERR_LH_REFUND_FREEZE = 150022033,
    ERR_TESTUIN_LOGIN_NEED_CHG_PWD_LIMIT = 150022034
}

internal enum NTLoginPasswordVerifyResult {
    PASSWORD_VERIFY_RESULT_UNKOWN = 0,
    PASSWORD_VERIFY_RESULT_COREECT = 1,
    PASSWORD_VERIFY_RESULT_WRONG = 2
}

internal enum NTLoginAccountType {
    ACCOUNT_TYPE_UNKOWN = 0,
    ACCOUNT_TYPE_UIN = 1,
    ACCOUNT_TYPE_PHONE = 2,
    ACCOUNT_TYPE_MAIL = 3,
    ACCOUNT_TYPE_QID = 4,
    ACCOUNT_TYPE_MASK_UIN = 5
}

internal enum NTLoginCodeType {
    TYPE_AUTHCODE = 0,
    TYPE_TGT = 1,
    TYPE_A2 = 2,
    TYPE_TGTGT = 3
}

#endregion

#region Head

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginHead
{
    [ProtoMember(1)] public NTLoginUserInfo UserInfo { get; set; }

    [ProtoMember(2)] public NTLoginClientInfo ClientInfo { get; set; }
    
    [ProtoMember(3)] public NTLoginAppInfo AppInfo { get; set; }
    
    [ProtoMember(4)] public NTLoginErrorInfo ErrorInfo { get; set; }
    
    [ProtoMember(5)] public NTLoginCookie? Cookie { get; set; }
    
    [ProtoMember(6)] public NTLoginSecurityInfo SecurityInfo { get; set; }
    
    [ProtoMember(7)] public NTLoginSdkInfo SdkInfo { get; set; }
    
    [ProtoMember(8)] public NTLoginLongCookie LongCookie { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginUserInfo
{
    [ProtoMember(1)] public string Account { get; set; }
    
    [ProtoMember(2)] public uint CountryCode { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginClientInfo
{
    [ProtoMember(1)] public string DeviceType { get; set; }
    
    [ProtoMember(2)] public string DeviceName { get; set; }
    
    [ProtoMember(3)] public NTLoginPlatform Platform { get; set; }
    
    [ProtoMember(4)] public byte[] Guid { get; set; }
    
    [ProtoMember(5)] public uint Pubno { get; set; }
    
    [ProtoMember(6)] public uint ClientVer { get; set; }
    
    [ProtoMember(7)] public uint ClientType { get; set; }
    
    [ProtoMember(8)] public uint SsoVer { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginAppInfo
{
    [ProtoMember(1)] public string Version { get; set; }
    
    [ProtoMember(2)] public int AppId { get; set; }
    
    [ProtoMember(3)] public string AppName { get; set; }
    
    [ProtoMember(4)] public uint ClientA1Version { get; set; }
    
    [ProtoMember(5)] public string Qua { get; set; }
    
    [ProtoMember(6)] public NTLoginOpenInfo OpenInfo { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginOpenInfo
{
    [ProtoMember(1)] public uint AppId { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSdkInfo
{
    [ProtoMember(1)] public uint Version { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSecurityInfo;

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCookie
{
    [ProtoMember(1)] public string CookieContent { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginLongCookie
{
    [ProtoMember(1)] public byte[] Content { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginErrorInfo
{
    [ProtoMember(1)] public ulong ErrCode { get; set; }
    
    [ProtoMember(2)] public string StrTipsTitle { get; set; }
    
    [ProtoMember(3)] public string StrTipsContent { get; set; }
    
    [ProtoMember(4)] public string StrJumpWording { get; set; }
    
    [ProtoMember(5)] public string StrJumpUrl { get; set; }
    
    [ProtoMember(6)] public NTLoginErrorDetail MsgDetail { get; set; }
    
    [ProtoMember(7)] public List<NTLoginButton> RptMsgButton { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginErrorDetail
{
    [ProtoMember(1)] public NTLoginErrorNeedVerifyNewDevice MsgNeedVerifyNewDevice { get; set; }
    
    [ProtoMember(2)] public NTLoginErrorUnregistered MsgUnregistered { get; set; }
    
    [ProtoMember(3)] public NTLoginErrorBeenForbiden MsgBeenForbiden { get; set; }
    
    [ProtoMember(4)] public NTLoginErrorNiceAccountExpire MsgNiceAccountExpire { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginErrorNeedVerifyNewDevice
{
    [ProtoMember(1)] public bool AllowGateWayVerify { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginErrorUnregistered
{
    [ProtoMember(1)] public string UnregisteredSig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginErrorBeenForbiden
{
    [ProtoMember(1)] public uint Area { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginErrorNiceAccountExpire
{
    [ProtoMember(1)] public string ExpireSig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginButton
{
    [ProtoMember(1)] public string Wording { get; set; }
    
    [ProtoMember(2)] public string Url { get; set; }
}
#endregion

#region Body

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCommonInfo
{
    [ProtoMember(1)] public uint Face { get; set; }

    [ProtoMember(2)] public string Nick { get; set; }

    [ProtoMember(3)] public uint Gender { get; set; }

    [ProtoMember(4)] public uint Flag { get; set; }

    [ProtoMember(5)] public uint Age { get; set; }

    [ProtoMember(6)] public int SvrFlag { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginTgtInfo
{
    [ProtoMember(1)] public byte[] Tgt { get; set; }

    [ProtoMember(2)] public byte[] GtkeyTgt { get; set; }

    [ProtoMember(3)] public uint TgtVer { get; set; }

    [ProtoMember(4)] public ulong Priority { get; set; }

    [ProtoMember(5)] public ulong RefreshInterval { get; set; }

    [ProtoMember(6)] public ulong ValidateInterval { get; set; }

    [ProtoMember(7)] public ulong TryRefreshInterval { get; set; }

    [ProtoMember(8)] public ulong TryRefreshCount { get; set; }

    [ProtoMember(9)] public ulong DstAppid { get; set; }

    [ProtoMember(10)] public byte[] GtkeyTgtpwd { get; set; }

    [ProtoMember(11)] public NTLoginCommonInfo CommInfo { get; set; }

    [ProtoMember(12)] public byte[] SigSession { get; set; }

    [ProtoMember(13)] public byte[] SigSessionKey { get; set; }

    [ProtoMember(14)] public ulong NextRefreshGap { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSTInfo
{
    [ProtoMember(1)] public byte[] St { get; set; }

    [ProtoMember(2)] public byte[] GtkeySt { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSTHttpInfo
{
    [ProtoMember(1)] public uint AllowPtlogin { get; set; }

    [ProtoMember(2)] public byte[] StHttp { get; set; }

    [ProtoMember(3)] public byte[] GtkeyStHttp { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginControlRefreshTime
{
    [ProtoMember(1)] public ulong NextStartRefreshTime { get; set; }

    [ProtoMember(2)] public ulong ExpireTime { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginTickets
{
    [ProtoMember(3)] public byte[] A1 { get; set; }

    [ProtoMember(4)] public byte[] A2 { get; set; }

    [ProtoMember(5)] public byte[] D2 { get; set; }

    [ProtoMember(6)] public byte[] D2Key { get; set; }

    [ProtoMember(7)] public byte[] AuthCode { get; set; }

    [ProtoMember(8)] public NTLoginTgtInfo TgtInfo { get; set; }

    [ProtoMember(9)] public NTLoginSTInfo StInfo { get; set; }

    [ProtoMember(10)] public byte[] SecExtra { get; set; }

    [ProtoMember(11)] public NTLoginSTHttpInfo StHttpInfo { get; set; }

    [ProtoMember(12)] public NTLoginControlRefreshTime A1RefreshTime { get; set; }

    [ProtoMember(13)] public byte[] Nopicsig { get; set; }

    [ProtoMember(14)] public byte[] A1Key { get; set; }

    [ProtoMember(15)] public ulong A1Seq { get; set; }

    [ProtoMember(16)] public NTLoginControlRefreshTime A2RefreshTime { get; set; }

    [ProtoMember(17)] public byte[] A2Key { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginUserProfile
{
    [ProtoMember(1)] public byte[] NickName { get; set; }

    [ProtoMember(2)] public bool RegisterWithoutPassword { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginAccoutInfo
{
    [ProtoMember(1)] public ulong Uin { get; set; }

    [ProtoMember(2)] public string Uid { get; set; }

    [ProtoMember(3)] public NTLoginUserProfile UserProfile { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginIframe
{
    [ProtoMember(1)] public string IframeSig { get; set; }

    [ProtoMember(2)] public string IframeRandstr { get; set; }

    [ProtoMember(3)] public string IframeSid { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSecProtect
{
    [ProtoMember(1)] public byte[] NewDeviceCheckSig { get; set; }

    [ProtoMember(2)] public byte[] UnusualDeviceCheckSig { get; set; }

    [ProtoMember(3)] public string UnusualDeviceQrSig { get; set; }

    [ProtoMember(4)] public string UinToken { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSecCheck
{
    [ProtoMember(3)] public string IframeUrl { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginLoginProcessReqBody
{
    [ProtoMember(1)] public bool NeedRemindCancellatedStatus { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginLoginProcessRspBody
{
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginPasswordLoginReqBody
{
    [ProtoMember(1)] public byte[] A1 { get; set; }

    [ProtoMember(2)] public NTLoginIframe? Iframe { get; set; }

    [ProtoMember(3)] public byte[] NewDeviceCheckSucceedSig { get; set; }

    [ProtoMember(4)] public string RegisterSucceedSig { get; set; }

    [ProtoMember(5)] public NTLoginLoginProcessReqBody LoginProcessReq { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginPasswordLoginRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }

    [ProtoMember(2)] public NTLoginSecCheck SecCheck { get; set; }

    [ProtoMember(3)] public NTLoginSecProtect SecProtect { get; set; }

    [ProtoMember(4)] public NTLoginAccoutInfo Account { get; set; }

    [ProtoMember(5)] public NTLoginLoginProcessRspBody LoginProcessRsp { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginPasswordLoginNewDeviceReqBody
{
    [ProtoMember(1)] public byte[] NewDeviceCheckSucceedSig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginPasswordLoginNewDeviceRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }

    [ProtoMember(3)] public NTLoginAccoutInfo Account { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginPasswordLoginUnusualDeviceReqBody
{
    [ProtoMember(1)] public byte[] A1 { get; set; }

    [ProtoMember(2)] public byte[] UnusualDeviceCheckSucceedSig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginPasswordLoginUnusualDeviceRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }

    [ProtoMember(2)] public NTLoginAccoutInfo Account { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginEasyLoginReqBody
{
    [ProtoMember(1)] public byte[] A1 { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginEasyLoginRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }

    [ProtoMember(2)] public NTLoginSecCheck SecCheck { get; set; }

    [ProtoMember(3)] public NTLoginSecProtect SecProtect { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginEasyLoginUnusualDeviceReqBody
{
    [ProtoMember(1)] public byte[] A1 { get; set; }

    [ProtoMember(2)] public byte[] UnusualDeviceCheckSucceedSig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginEasyLoginUnusualDeviceRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginTGTExchangeFastLoginReqBody
{
    [ProtoMember(1)] public byte[] Tgt { get; set; }

    [ProtoMember(2)] public byte[] SecExtra { get; set; }

    [ProtoMember(3)] public NTLoginCodeType CodeType { get; set; }
}


[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginTGTExchangeFastLoginRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRefreshTicketReqBody
{
    [ProtoMember(1)] public byte[] A1 { get; set; }

    [ProtoMember(2)] public byte[] Nopicsig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRefreshTicketRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }

    [ProtoMember(2)] public NTLoginAccoutInfo Account { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRefreshA2ReqBody
{
    [ProtoMember(1)] public byte[] A2 { get; set; }

    [ProtoMember(2)] public byte[] D2 { get; set; }

    [ProtoMember(3)] public byte[] D2Key { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRefreshA2RspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }

    [ProtoMember(2)] public NTLoginAccoutInfo Account { get; set; }
}

#endregion

[ProtoPackable]
internal partial class NTLoginCommon
{
    [ProtoMember(1)] public NTLoginHead Head { get; set; }
    
    [ProtoMember(2)] public ReadOnlyMemory<byte> Body { get; set; }
}