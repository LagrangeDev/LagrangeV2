using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.System;

internal static class FlashTransferServiceCommon
{
    public static FlashTransferUploadHeader CreateHeader(uint scene, uint subCommand, uint capability = 22) => new()
    {
        CommandIdentity = new FlashTransferUploadCommandIdentity { Scene = scene, SubCommand = subCommand },
        ClientCaps = new FlashTransferUploadClientCaps { Field101 = 2, Field102 = 4, Capability = capability, Field200 = 5 },
        Context = new FlashTransferUploadRequestContext { Field1 = 1 }
    };

    public static uint ResolveCapability(uint field5, uint field6) => field5 == 1 ? 23u : field6 == 1 ? 24u : 22u;

    public static string ResolveAsciiName(string name)
    {
        Span<char> chars = stackalloc char[name.Length];
        int length = 0;
        foreach (char c in name)
        {
            chars[length++] = c < 128 && !char.IsWhiteSpace(c) ? c : '_';
        }

        return new string(chars[..length]);
    }

    public static FlashTransferUploadFileBinding CreateBinding(string fileSetId, string fileId, uint stage, uint fileType, uint field5 = 0, uint field6 = 0) => new()
    {
        FileSetId = fileSetId,
        FileSetIdDup = fileSetId,
        FileId = fileId,
        Stage = stage,
        Field5 = field5,
        Field6 = field6,
        FileType = fileType,
        FileIdDup = fileId
    };
}

[EventSubscribe<FlashTransferCreateFileSetEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x93cf_1")]
internal class FlashTransferCreateFileSetService : OidbService<FlashTransferCreateFileSetEventReq, FlashTransferCreateFileSetEventResp, FlashTransferCreateFileSetRequest, FlashTransferCreateFileSetResponse>
{
    private protected override uint Command => 0x93cf;

    private protected override uint Service => 1;

    private protected override uint Reserved => 1;

    private protected override Task<FlashTransferCreateFileSetRequest> ProcessRequest(FlashTransferCreateFileSetEventReq request, BotContext context)
    {
        return Task.FromResult(new FlashTransferCreateFileSetRequest
        {
            Scene = 1,
            FileSet = new FlashTransferFileSetCreateInfo
            {
                Title = request.Title,
                AsciiTitle = request.AsciiTitle,
                FileCount = (uint)request.Files.Count,
                TotalSize = (ulong)request.Files.Sum(file => file.Stream.Length),
                Peer = new FlashTransferPeer
                {
                    UidOrOpenid = context.Keystore.Uid,
                    DisplayName = context.Keystore.BotInfo?.Name ?? context.Keystore.Uin.ToString(),
                    Remark = string.Empty
                },
                FileCountDup = (uint)request.Files.Count,
                Field20 = 0,
                Field21 = 0,
                Field23 = 0
            },
            ClientType = 14
        });
    }

    private protected override Task<FlashTransferCreateFileSetEventResp> ProcessResponse(FlashTransferCreateFileSetResponse response, BotContext context)
        => Task.FromResult(new FlashTransferCreateFileSetEventResp(response.FileSetId, response.ShareLink));
}

[EventSubscribe<FlashTransferRegisterFilesEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x93d0_1")]
internal class FlashTransferRegisterFilesService : OidbService<FlashTransferRegisterFilesEventReq, FlashTransferRegisterFilesEventResp, FlashTransferRegisterFilesRequest, FlashTransferRegisterFilesResponse>
{
    private protected override uint Command => 0x93d0;

    private protected override uint Service => 1;

    private protected override uint Reserved => 1;

    private protected override Task<FlashTransferRegisterFilesRequest> ProcessRequest(FlashTransferRegisterFilesEventReq request, BotContext context)
    {
        return Task.FromResult(new FlashTransferRegisterFilesRequest
        {
            Scene = 1,
            FileSetId = request.FileSetId,
            FileSetIdDup = request.FileSetId,
            Files = request.Files.Select(file => new FlashTransferFileSetFile
            {
                FileSetId = request.FileSetId,
                FileId = file.FileId,
                Field3 = 0,
                Field4 = [],
                Field5 = 1,
                Index = file.Index,
                FileType = file.FileType,
                FileName = file.FileName,
                DisplayName = FlashTransferServiceCommon.ResolveAsciiName(file.FileName),
                Field10 = 0,
                FileSize = (ulong)file.Stream.Length,
                Field12 = 0,
                Field24 = []
            }).ToList(),
            Field5 = 1,
            Field6 = 1
        });
    }

    private protected override Task<FlashTransferRegisterFilesEventResp> ProcessResponse(FlashTransferRegisterFilesResponse response, BotContext context)
        => Task.FromResult(new FlashTransferRegisterFilesEventResp());
}

[EventSubscribe<FlashTransferQueryFileSetStatusEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x93db_1")]
internal class FlashTransferQueryFileSetStatusService : OidbService<FlashTransferQueryFileSetStatusEventReq, FlashTransferQueryFileSetStatusEventResp, FlashTransferQueryFileSetStatusRequest, FlashTransferQueryFileSetStatusResponse>
{
    private protected override uint Command => 0x93db;

    private protected override uint Service => 1;

    private protected override uint Reserved => 1;

    private protected override Task<FlashTransferQueryFileSetStatusRequest> ProcessRequest(FlashTransferQueryFileSetStatusEventReq request, BotContext context)
    {
        return Task.FromResult(new FlashTransferQueryFileSetStatusRequest
        {
            FileSetId = request.FileSetId,
            Field2 = []
        });
    }

    private protected override Task<FlashTransferQueryFileSetStatusEventResp> ProcessResponse(FlashTransferQueryFileSetStatusResponse response, BotContext context)
        => Task.FromResult(new FlashTransferQueryFileSetStatusEventResp());
}

[EventSubscribe<FlashTransferUpdateFileSetStatusEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x93d1_1")]
internal class FlashTransferUpdateFileSetStatusService : OidbService<FlashTransferUpdateFileSetStatusEventReq, FlashTransferUpdateFileSetStatusEventResp, FlashTransferUpdateFileSetStatusRequest, FlashTransferUpdateFileSetStatusResponse>
{
    private protected override uint Command => 0x93d1;

    private protected override uint Service => 1;

    private protected override uint Reserved => 1;

    private protected override Task<FlashTransferUpdateFileSetStatusRequest> ProcessRequest(FlashTransferUpdateFileSetStatusEventReq request, BotContext context)
    {
        return Task.FromResult(new FlashTransferUpdateFileSetStatusRequest
        {
            FileSetId = request.FileSetId,
            Status = request.Status
        });
    }

    private protected override Task<FlashTransferUpdateFileSetStatusEventResp> ProcessResponse(FlashTransferUpdateFileSetStatusResponse response, BotContext context)
        => Task.FromResult(new FlashTransferUpdateFileSetStatusEventResp());
}

[EventSubscribe<FlashTransferUploadAuthorizeEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x12a9_100")]
internal class FlashTransferUploadAuthorizeService : OidbService<FlashTransferUploadAuthorizeEventReq, FlashTransferUploadAuthorizeEventResp, FlashTransferUploadRequest, FlashTransferUploadAuthorizeResponse>
{
    private protected override uint Command => 0x12a9;

    private protected override uint Service => 100;

    private protected override Task<FlashTransferUploadRequest> ProcessRequest(FlashTransferUploadAuthorizeEventReq request, BotContext context)
    {
        var file = request.File;


        return Task.FromResult(new FlashTransferUploadRequest
        {
            Header = FlashTransferServiceCommon.CreateHeader(request.Scene, 100),
            AuthorizePayload = new FlashTransferUploadAuthorizePayload
            {
                FileContainer = new FlashTransferUploadFileContainer
                {
                    File = new FlashTransferUploadFileInfo
                    {
                        FileSize = (ulong)file.Stream.Length,
                        Md5 = string.Empty,
                        Sha1 = Convert.ToHexString(file.FileSha1).ToLowerInvariant(),
                        FileName = file.FileName,
                        Field5 = [],
                        Field6 = 0,
                        Field7 = 0,
                        Field8 = 0,
                        Field9 = 1
                    },
                    Field2 = 0
                },
                Field2 = 0,
                Field3 = 0,
                Field4 = 0,
                Field5 = 0,
                Field6 = [],
                Field7 = 0,
                Field8 = 0,
                FileBinding = FlashTransferServiceCommon.CreateBinding(request.FileSetId, file.FileId, file.Index, file.FileType)
            }
        });
    }

    private protected override Task<FlashTransferUploadAuthorizeEventResp> ProcessResponse(FlashTransferUploadAuthorizeResponse response, BotContext context)
    {
        string statusMessage = response.Status?.Message ?? string.Empty;
        if (statusMessage != "success") throw new OperationException(-1, string.IsNullOrEmpty(statusMessage) ? "FlashTransfer upload authorize failed without status message" : statusMessage);

        var result = response.Result;
        if (result == null)
        {
            context.LogWarning(nameof(FlashTransferUploadAuthorizeService), "FlashTransfer authorize response has no result payload");
            throw new OperationException(-1, "FlashTransfer upload authorize response has no result payload");
        }

        var resource = result.ResourceInfo.Outer.Resource;
        string resourceKey = resource.ResourceKey ?? string.Empty;
        var echoBinding = result.Echo?.FileBinding;
        string uploadHost = string.IsNullOrEmpty(result.UploadHost) ? result.BackupUploadHost : result.UploadHost;

        return Task.FromResult(new FlashTransferUploadAuthorizeEventResp(
            result.UploadToken ?? string.Empty,
            resourceKey,
            resource.AppId,
            uploadHost ?? string.Empty,
            result.ChunkConfig?.ChunkSize ?? 1024 * 1024,
            echoBinding?.Stage ?? 0,
            echoBinding?.Field5 ?? 0,
            echoBinding?.Field6 ?? 0));
    }
}

[EventSubscribe<FlashTransferUploadCompleteEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x12a9_103")]
internal class FlashTransferUploadCompleteService : OidbService<FlashTransferUploadCompleteEventReq, FlashTransferUploadCompleteEventResp, FlashTransferUploadRequest, FlashTransferUploadCompleteResponse>
{
    private protected override uint Command => 0x12a9;

    private protected override uint Service => 103;

    private protected override Task<FlashTransferUploadRequest> ProcessRequest(FlashTransferUploadCompleteEventReq request, BotContext context)
    {
        var file = request.File;
        ulong now = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return Task.FromResult(new FlashTransferUploadRequest
        {
            Header = FlashTransferServiceCommon.CreateHeader(request.Scene, 103, FlashTransferServiceCommon.ResolveCapability(request.BindingField5, request.BindingField6)),
            CompletePayload = new FlashTransferUploadCompletePayload
            {
                RawFileResult = new FlashTransferUploadCompleteFileResult
                {
                    File = new FlashTransferUploadFileInfo
                    {
                        FileSize = (ulong)file.Stream.Length,
                        Md5 = Convert.ToHexString(file.FileMd5).ToLowerInvariant(),
                        Sha1 = Convert.ToHexString(file.FileSha1).ToLowerInvariant(),
                        FileName = file.FileName,
                        Field5 = [0x08, 0x00, 0x10, 0x00, 0x18, 0x00, 0x20, 0x00],
                        Field6 = 0,
                        Field7 = 0,
                        Field8 = 0,
                        Field9 = 1
                    },
                    ResourceKey = request.ResourceKey,
                    Field3 = 1,
                    CreateTime = now,
                    TtlSeconds = 1209600,
                    Field6 = 0,
                    Field7 = 0,
                    Field8 = 0
                },
                Field2 = [0x08, 0x02],
                Field3 = [0x08, 0x00, 0x10, 0x00, 0x18, 0x00, 0x22, 0x00],
                Field4 = [0x08, 0x00, 0x12, 0x00],
                FileBinding = FlashTransferServiceCommon.CreateBinding(request.FileSetId, file.FileId, request.BindingStage, file.FileType, request.BindingField5, request.BindingField6)
            }
        });
    }

    private protected override Task<FlashTransferUploadCompleteEventResp> ProcessResponse(FlashTransferUploadCompleteResponse response, BotContext context)
    {
        string statusMessage = response.Status?.Message ?? string.Empty;
        if (statusMessage != "success") throw new OperationException(-1, string.IsNullOrEmpty(statusMessage) ? "FlashTransfer upload complete failed without status message" : statusMessage);
        return Task.FromResult(new FlashTransferUploadCompleteEventResp());
    }
}
