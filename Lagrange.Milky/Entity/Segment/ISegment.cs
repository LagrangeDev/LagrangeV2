using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextIncomingSegment), typeDiscriminator: "text")]
[JsonDerivedType(typeof(MentionIncomingSegment), typeDiscriminator: "mention")]
[JsonDerivedType(typeof(MentionAllIncomingSegment), typeDiscriminator: "mention_all")]
[JsonDerivedType(typeof(FaceIncomingSegment), typeDiscriminator: "face_incoming")]
[JsonDerivedType(typeof(ReplyIncomingSegment), typeDiscriminator: "reply")]
[JsonDerivedType(typeof(ImageIncomingSegment), typeDiscriminator: "image")]
[JsonDerivedType(typeof(RecordIncomingSegment), typeDiscriminator: "record")]
[JsonDerivedType(typeof(VideoIncomingSegment), typeDiscriminator: "video")]
[JsonDerivedType(typeof(ForwardIncomingSegment), typeDiscriminator: "forward")]
[JsonDerivedType(typeof(MarketFaceIncomingSegment), typeDiscriminator: "market_face")]
[JsonDerivedType(typeof(LightAppIncomingSegment), typeDiscriminator: "light_app")]
[JsonDerivedType(typeof(XmlIncomingSegment), typeDiscriminator: "xml")]
public interface IIncomingSegment
{
    object? Data { get; }
}


[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextOutgoingSegment), typeDiscriminator: "text")]
[JsonDerivedType(typeof(MentionOutgoingSegment), typeDiscriminator: "mention")]
[JsonDerivedType(typeof(MentionAllOutgoingSegment), typeDiscriminator: "mention_all")]
[JsonDerivedType(typeof(FaceOutgoingSegment), typeDiscriminator: "face")]
[JsonDerivedType(typeof(ReplyOutgoingSegment), typeDiscriminator: "reply")]
[JsonDerivedType(typeof(ImageOutgoingSegment), typeDiscriminator: "image")]
[JsonDerivedType(typeof(RecordOutgoingSegment), typeDiscriminator: "record")]
[JsonDerivedType(typeof(VideoOutgoingSegment), typeDiscriminator: "video")]
[JsonDerivedType(typeof(ForwardOutgoingSegment), typeDiscriminator: "forward")]
public interface IOutgoingSegment
{
    object? Data { get; }
}
