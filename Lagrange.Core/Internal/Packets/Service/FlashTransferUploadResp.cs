using Lagrange.Proto;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable]
internal partial class FlashTransferUploadResp
{
    [ProtoMember(5)] public string Status { get; set; }
}