using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D89AReqBody
{
    [ProtoPackable]
    internal partial class GroupExInfoOnly
    {
        [ProtoMember(1)] public uint? TribeId { get; set; }

        [ProtoMember(2)] public uint? MoneyForAddGroup { get; set; }
    }

    [ProtoPackable]
    internal partial class GroupGeoInfo
    {
        [ProtoMember(1)] public uint? CityId { get; set; }

        [ProtoMember(2)] public ulong? Longitude { get; set; }

        [ProtoMember(3)] public ulong? Latitude { get; set; }

        [ProtoMember(4)] public string? GeoContent { get; set; }

        [ProtoMember(5)] public ulong? PoiId { get; set; }
    }

    [ProtoPackable]
    internal partial class GroupNewGuidelinesInfo
    {
        [ProtoMember(1)] public bool? Enabled { get; set; }

        [ProtoMember(2)] public string? Content { get; set; }
    }

    [ProtoPackable]
    internal partial class GroupInfo
    {
        [ProtoMember(1)] public uint? GroupExtAdmNum { get; set; }

        [ProtoMember(2)] public uint? Flag { get; set; }

        [ProtoMember(3)] public string? GroupName { get; set; }

        [ProtoMember(4)] public string? GroupMemo { get; set; }

        [ProtoMember(5)] public string? GroupFingerMemo { get; set; }

        [ProtoMember(6)] public string? GroupAioSkinUrl { get; set; }

        [ProtoMember(7)] public string? GroupBoardSkinUrl { get; set; }

        [ProtoMember(8)] public string? GroupCoverSkinUrl { get; set; }

        [ProtoMember(9)] public uint? GroupGrade { get; set; }

        [ProtoMember(10)] public uint? ActiveMemberNum { get; set; }

        [ProtoMember(11)] public uint? CertificationType { get; set; }

        [ProtoMember(12)] public string? CertificationText { get; set; }

        [ProtoMember(13)] public string? GroupRichFingerMemo { get; set; }

        [ProtoMember(14)] public GroupNewGuidelinesInfo? GroupNewGuidelines { get; set; }

        [ProtoMember(15)] public uint? GroupFace { get; set; }

        [ProtoMember(16)] public uint? AddOption { get; set; }

        [ProtoMember(17)] public uint? ShutupTime { get; set; }

        [ProtoMember(18)] public uint? GroupTypeFlag { get; set; }

        [ProtoMember(19)] public List<string>? GroupTag { get; set; }

        [ProtoMember(20)] public GroupGeoInfo? GroupGeo { get; set; }

        [ProtoMember(21)] public uint? GroupClassExt { get; set; }

        [ProtoMember(22)] public string? GroupClassText { get; set; }

        [ProtoMember(23)] public uint? AppPrivilegeFlag { get; set; }

        [ProtoMember(24)] public uint? AppPrivilegeMask { get; set; }

        [ProtoMember(25)] public GroupExInfoOnly? GroupExInfo { get; set; }

        [ProtoMember(26)] public uint? GroupSecLevel { get; set; }

        [ProtoMember(27)] public uint? GroupSecLevelInfo { get; set; }

        [ProtoMember(28)] public long? SubscriptionUin { get; set; }

        [ProtoMember(29)] public uint? AllowMemberInvite { get; set; }

        [ProtoMember(30)] public string? GroupQuestion { get; set; }

        [ProtoMember(31)] public string? GroupAnswer { get; set; }

        [ProtoMember(32)] public uint? GroupFlagExt3 { get; set; }

        [ProtoMember(33)] public uint? GroupFlagExt3Mask { get; set; }

        [ProtoMember(34)] public uint? GroupOpenAppid { get; set; }

        [ProtoMember(35)] public uint? NoFingerOpenFlag { get; set; }

        [ProtoMember(36)] public uint? NoCodeFingerOpenFlag { get; set; }

        [ProtoMember(37)] public long? RootId { get; set; }

        [ProtoMember(38)] public uint? MsgLimitFrequency { get; set; }

        [ProtoMember(39)] public uint? HlGuildAppid { get; set; }

        [ProtoMember(40)] public uint? HlGuildSubType { get; set; }

        [ProtoMember(41)] public uint? HlGuildOrgid { get; set; }

        [ProtoMember(42)] public uint? GroupFlagExt4 { get; set; }

        [ProtoMember(43)] public uint? GroupFlagExt4Mask { get; set; }
    }

    [ProtoMember(1)] public long? GroupCode { get; set; }

    [ProtoMember(2)] public GroupInfo Group { get; set; }

    [ProtoMember(3)] public long? OriginalOperatorUin { get; set; }

    [ProtoMember(4)] public uint? ReqGroupOpenAppid { get; set; }
}

[ProtoPackable]
internal partial class D89ARspBody
{
    [ProtoMember(1)] public long GroupCode { get; set; }

    [ProtoMember(2)] public string ErrorInfo { get; set; }
}
