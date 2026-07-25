using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class FlashTransferCreateFileSetRequest
{
    [ProtoMember(1)] public uint Scene { get; set; }

    [ProtoMember(2)] public FlashTransferFileSetCreateInfo FileSet { get; set; }

    [ProtoMember(3)] public uint ClientType { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferFileSetCreateInfo
{
    [ProtoMember(2)] public string Title { get; set; }

    [ProtoMember(3)] public string AsciiTitle { get; set; }

    [ProtoMember(4)] public uint FileCount { get; set; }

    [ProtoMember(5)] public ulong TotalSize { get; set; }

    [ProtoMember(10)] public FlashTransferPeer Peer { get; set; }

    [ProtoMember(16)] public uint FileCountDup { get; set; }

    [ProtoMember(20)] public uint Field20 { get; set; }

    [ProtoMember(21)] public uint Field21 { get; set; }

    [ProtoMember(23)] public uint Field23 { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferPeer
{
    [ProtoMember(1)] public string UidOrOpenid { get; set; }

    [ProtoMember(2)] public string DisplayName { get; set; }

    [ProtoMember(3)] public string Remark { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferCreateFileSetResponse
{
    [ProtoMember(1)] public string FileSetId { get; set; }

    [ProtoMember(2)] public string FileSetIdDup { get; set; }

    [ProtoMember(3)] public string ShareLink { get; set; }

    [ProtoMember(4)] public ulong ExpireTime { get; set; }

    [ProtoMember(5)] public ulong ExpireLeftTime { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferRegisterFilesRequest
{
    [ProtoMember(1)] public uint Scene { get; set; }

    [ProtoMember(2)] public string FileSetId { get; set; }

    [ProtoMember(3)] public string FileSetIdDup { get; set; }

    [ProtoMember(4)] public List<FlashTransferFileSetFile> Files { get; set; }

    [ProtoMember(5)] public uint Field5 { get; set; }

    [ProtoMember(6)] public uint Field6 { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferFileSetFile
{
    [ProtoMember(1)] public string FileSetId { get; set; }

    [ProtoMember(2)] public string FileId { get; set; }

    [ProtoMember(3)] public uint Field3 { get; set; }

    [ProtoMember(4)] public byte[] Field4 { get; set; }

    [ProtoMember(5)] public uint Field5 { get; set; }

    [ProtoMember(6)] public uint Index { get; set; }

    [ProtoMember(7)] public uint FileType { get; set; }

    [ProtoMember(8)] public string FileName { get; set; }

    [ProtoMember(9)] public string DisplayName { get; set; }

    [ProtoMember(10)] public uint Field10 { get; set; }

    [ProtoMember(11)] public ulong FileSize { get; set; }

    [ProtoMember(12)] public uint Field12 { get; set; }

    [ProtoMember(24)] public byte[] Field24 { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferRegisterFilesResponse;

[ProtoPackable]
internal partial class FlashTransferQueryFileSetStatusRequest
{
    [ProtoMember(1)] public string FileSetId { get; set; }

    [ProtoMember(2)] public byte[] Field2 { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferQueryFileSetStatusResponse
{
    [ProtoMember(1)] public uint Field1 { get; set; }

    [ProtoMember(3)] public string Field3 { get; set; }

    [ProtoMember(6)] public byte[] Field6 { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUpdateFileSetStatusRequest
{
    [ProtoMember(1)] public string FileSetId { get; set; }

    [ProtoMember(2)] public uint Status { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUpdateFileSetStatusResponse
{
    [ProtoMember(1)] public string FileSetId { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadRequest
{
    [ProtoMember(1)] public FlashTransferUploadHeader Header { get; set; }

    [ProtoMember(2)] public FlashTransferUploadAuthorizePayload AuthorizePayload { get; set; }

    [ProtoMember(12)] public FlashTransferUploadCompletePayload CompletePayload { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadHeader
{
    [ProtoMember(1)] public FlashTransferUploadCommandIdentity CommandIdentity { get; set; }

    [ProtoMember(2)] public FlashTransferUploadClientCaps ClientCaps { get; set; }

    [ProtoMember(3)] public FlashTransferUploadRequestContext Context { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadCommandIdentity
{
    [ProtoMember(1)] public uint Scene { get; set; }

    [ProtoMember(2)] public uint SubCommand { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadClientCaps
{
    [ProtoMember(101)] public uint Field101 { get; set; }

    [ProtoMember(102)] public uint Field102 { get; set; }

    [ProtoMember(103)] public uint Capability { get; set; }

    [ProtoMember(200)] public uint Field200 { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadRequestContext
{
    [ProtoMember(1)] public uint Field1 { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadAuthorizePayload
{
    [ProtoMember(1)] public FlashTransferUploadFileContainer FileContainer { get; set; }

    [ProtoMember(2)] public uint Field2 { get; set; }

    [ProtoMember(3)] public uint Field3 { get; set; }

    [ProtoMember(4)] public uint Field4 { get; set; }

    [ProtoMember(5)] public uint Field5 { get; set; }

    [ProtoMember(6)] public byte[] Field6 { get; set; }

    [ProtoMember(7)] public uint Field7 { get; set; }

    [ProtoMember(8)] public uint Field8 { get; set; }

    [ProtoMember(9)] public FlashTransferUploadFileBinding FileBinding { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadFileContainer
{
    [ProtoMember(1)] public FlashTransferUploadFileInfo File { get; set; }

    [ProtoMember(2)] public uint Field2 { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadFileInfo
{
    [ProtoMember(1)] public ulong FileSize { get; set; }

    [ProtoMember(2)] public string Md5 { get; set; }

    [ProtoMember(3)] public string Sha1 { get; set; }

    [ProtoMember(4)] public string FileName { get; set; }

    [ProtoMember(5)] public byte[] Field5 { get; set; }

    [ProtoMember(6)] public uint Field6 { get; set; }

    [ProtoMember(7)] public uint Field7 { get; set; }

    [ProtoMember(8)] public uint Field8 { get; set; }

    [ProtoMember(9)] public uint Field9 { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadFileBinding
{
    [ProtoMember(1)] public string FileSetId { get; set; }

    [ProtoMember(2)] public string FileSetIdDup { get; set; }

    [ProtoMember(3)] public string FileId { get; set; }

    [ProtoMember(4)] public uint Stage { get; set; }

    [ProtoMember(5)] public uint Field5 { get; set; }

    [ProtoMember(6)] public uint Field6 { get; set; }

    [ProtoMember(7)] public uint FileType { get; set; }

    [ProtoMember(8)] public string FileIdDup { get; set; }

    [ProtoMember(9)] public uint Field9 { get; set; }

    [ProtoMember(10)] public uint Field10 { get; set; }

    [ProtoMember(11)] public uint Field11 { get; set; }

    [ProtoMember(12)] public uint Field12 { get; set; }

    [ProtoMember(13)] public uint Field13 { get; set; }

    [ProtoMember(14)] public uint Field14 { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadAuthorizeResponse
{
    [ProtoMember(1)] public FlashTransferUploadStatus Status { get; set; }

    [ProtoMember(2)] public FlashTransferUploadAuthorizeResult Result { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadStatus
{
    [ProtoMember(1)] public FlashTransferUploadCommandIdentity CommandIdentity { get; set; }

    [ProtoMember(3)] public string Message { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadAuthorizeResult
{
    [ProtoMember(1)] public string UploadToken { get; set; }

    [ProtoMember(2)] public ulong Field2 { get; set; }

    [ProtoMember(6)] public FlashTransferUploadResourceInfo ResourceInfo { get; set; }

    [ProtoMember(7)] public FlashTransferUploadEchoInfo Echo { get; set; }

    [ProtoMember(11)] public string UploadHost { get; set; }

    [ProtoMember(16)] public FlashTransferUploadChunkConfig ChunkConfig { get; set; }

    [ProtoMember(17)] public string BackupUploadHost { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadResourceInfo
{
    [ProtoMember(1)] public FlashTransferUploadResourceOuter Outer { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadResourceOuter
{
    [ProtoMember(1)] public FlashTransferUploadResource Resource { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadResource
{
    [ProtoMember(1)] public byte[] Field1 { get; set; }

    [ProtoMember(2)] public string ResourceKey { get; set; }

    [ProtoMember(3)] public uint Field3 { get; set; }

    [ProtoMember(4)] public ulong CreateTime { get; set; }

    [ProtoMember(5)] public ulong TtlSeconds { get; set; }

    [ProtoMember(7)] public uint AppId { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadEchoInfo
{
    [ProtoMember(2)] public uint Field2 { get; set; }

    [ProtoMember(3)] public FlashTransferUploadFileBinding FileBinding { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadChunkConfig
{
    [ProtoMember(1)] public uint ChunkSize { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadCompletePayload
{
    [ProtoMember(1)] public FlashTransferUploadCompleteFileResult RawFileResult { get; set; }

    [ProtoMember(2)] public byte[] Field2 { get; set; }

    [ProtoMember(3)] public byte[] Field3 { get; set; }

    [ProtoMember(4)] public byte[] Field4 { get; set; }

    [ProtoMember(10)] public FlashTransferUploadFileBinding FileBinding { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadCompleteFileResult
{
    [ProtoMember(1)] public FlashTransferUploadFileInfo File { get; set; }

    [ProtoMember(2)] public string ResourceKey { get; set; }

    [ProtoMember(3)] public uint Field3 { get; set; }

    [ProtoMember(4)] public ulong CreateTime { get; set; }

    [ProtoMember(5)] public ulong TtlSeconds { get; set; }

    [ProtoMember(6)] public uint Field6 { get; set; }

    [ProtoMember(7)] public uint Field7 { get; set; }

    [ProtoMember(8)] public uint Field8 { get; set; }
}

[ProtoPackable]
internal partial class FlashTransferUploadCompleteResponse
{
    [ProtoMember(1)] public FlashTransferUploadStatus Status { get; set; }
}
