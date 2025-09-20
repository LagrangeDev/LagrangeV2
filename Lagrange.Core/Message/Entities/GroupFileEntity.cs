using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Message.Entities;

public class GroupFileEntity : IMessageEntity
{
    public string FileId { get; internal init; } = string.Empty;

    public string FileName { get; internal init; } = string.Empty;

    public long FileSize { get; internal init; }

    public string FileMd5 { get; internal init; } = string.Empty;

    public string FileUrl { get; set; }  = string.Empty;

    public Task Postprocess(BotContext context, BotMessage message)
    {
        return Task.CompletedTask; // TODO: implement group file download event
    }

    Elem[] IMessageEntity.Build() => throw new NotSupportedException();

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.TransElemInfo is { ElemType: 24 } trans)
        {
            var payload = new BinaryPacket(trans.ElemValue.AsSpan());
            payload.Skip(1);
            var data = payload.ReadBytes(Prefix.Int16 | Prefix.LengthOnly);
            var extra = ProtoHelper.Deserialize<GroupFileExtra>(data).Inner.Info;

            return new GroupFileEntity
            {
                FileId = extra.FileId,
                FileName = extra.FileName,
                FileSize = extra.FileSize,
                FileMd5 = extra.FileMd5,
            };
        }

        return null;
    }

    public string ToPreviewString() => $"[群文件 {FileName}]";
}