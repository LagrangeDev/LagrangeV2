using Lagrange.Proto;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable]
internal partial class FlashTransferUploadReq
{
    [ProtoMember(1)] public uint FieId1 { get; set; } // 0
    [ProtoMember(2)] public uint AppId { get; set; } // 1402: 私信语音, 1403: 群语音, 1413: 私信视频, 1414: 私信视频封面, 1415: 群视频, 1416: 群视频封面, 1406: 私信图片, 1407: 群聊图片, 14901: 闪传, 14903: 闪传封面
    [ProtoMember(3)] public uint FileId3 { get; set; } // 0
    [ProtoMember(107)] public FlashTransferUploadBody Body { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadBody
{
    [ProtoMember(1)] public byte[] FieId1 { get; set; } // Empty
    [ProtoMember(2)] public string UKey { get; set; }
    [ProtoMember(3)] public uint Start { get; set; } // Start
    [ProtoMember(4)] public uint End { get; set; } // Start + Size - 1
    [ProtoMember(5)] public byte[] Sha1 { get; set; }
    [ProtoMember(6)] public FlashTransferSha1StateV Sha1StateV { get; set; }
    [ProtoMember(7)] public byte[] Body { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferSha1StateV
{
    [ProtoMember(1)] public List<byte[]> State { get; set; }
}