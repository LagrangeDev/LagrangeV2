using System.Security.Cryptography;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Cryptography;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Internal.Context;

public class FlashTransferContext
{
    private const string Tag = nameof(FlashTransferContext);
    private readonly BotContext _botContext;
    private readonly HttpClient _client;
    private readonly string? _url;
    private const uint ChunkSize = 1024 * 1024;

    internal FlashTransferContext(BotContext botContext)
    {
        _botContext = botContext;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
        _url = "https://multimedia.qfile.qq.com/sliceupload";
    }

    public async Task<bool> UploadFile(string uKey, uint appId, Stream bodyStream)
    {
        return await UploadFile(uKey, appId, 2, bodyStream, null, _url);
    }

    public async Task<bool> UploadFile(string uploadToken, string uploadHost, uint appId, uint uploadIndex, string fileSetId, string fileId, uint bindingStage, uint fileType, uint bindingField5, uint bindingField6, Stream bodyStream)
    {
        var binding = new FlashTransferUploadFileBinding
        {
            FileSetId = fileSetId,
            FileSetIdDup = fileSetId,
            FileId = fileId,
            Stage = bindingStage,
            Field5 = bindingField5,
            Field6 = bindingField6,
            FileType = fileType,
            FileIdDup = fileId,
        };

        return await UploadFile(uploadToken, appId, uploadIndex, bodyStream, binding, $"https://{uploadHost}/sliceupload");
    }

    private async Task<bool> UploadFile(string uKey, uint appId, uint uploadIndex, Stream bodyStream, FlashTransferUploadFileBinding? binding, string? url)
    {
        var sha1StateVs = new FlashTransferSha1StateV { State = [] };
        var chunkCount = (uint)((bodyStream.Length + ChunkSize - 1) / ChunkSize);

        var sha1Stream = new Sha1Stream();
        for (uint i = 0; i < chunkCount; i++)
        {
            if (i != chunkCount - 1)
            {
                var accLength = (int)((i + 1) * ChunkSize);
                var accBuffer = new byte[accLength];
            
                bodyStream.Position = 0;
                await bodyStream.ReadExactlyAsync(accBuffer, 0, accLength);
            
                var accSpan = accBuffer.AsSpan();
                var digest = new byte[20];
                sha1Stream.Update(accSpan);
                sha1Stream.Hash(digest, false);
                sha1Stream.Reset();
                sha1StateVs.State.Add(digest.ToArray());
            }
            else
            {
                bodyStream.Position = 0;
                sha1StateVs.State.Add(bodyStream.Sha1());
            }
        }

        for (uint i = 0; i < chunkCount; i++)
        {
            var chunkStart = (long)(i * ChunkSize);
            var chunkLength = (int)Math.Min(ChunkSize, bodyStream.Length - chunkStart);

            bodyStream.Position = chunkStart;
            var uploadBuffer = new byte[chunkLength];
            await bodyStream.ReadExactlyAsync(uploadBuffer, 0, chunkLength);

            var success = await UploadChunk(uKey, appId, uploadIndex, (uint)chunkStart, sha1StateVs, uploadBuffer, binding, url);
            if (!success) return false;
        }

        return true;
    }

    private async Task<bool> UploadChunk(string uKey, uint appId, uint uploadIndex, uint start, FlashTransferSha1StateV chunkSha1S, byte[] body, FlashTransferUploadFileBinding? binding, string? url)
    {
        byte[] chunkSha1 = SHA1.HashData(body);
        var req = new FlashTransferUploadReq
        {
            FileId = 0,
            AppId = appId,
            UploadIndex = uploadIndex,
            Body = new FlashTransferUploadBody
            {
                FileId = [],
                UKey = uKey,
                Start = start,
                End = (uint)(start + body.Length - 1),
                Sha1 = chunkSha1,
                Sha1StateV = binding == null ? chunkSha1S : new FlashTransferSha1StateV { State = [chunkSha1] },
                Body = body,
                FileBinding = binding
            }
        };
        var payload = ProtoHelper.Serialize(req).ToArray();
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Headers =
            {
                { "Accept", "*/*" },
                { "Expect", "100-continue" },
                { "Connection", "Keep-Alive" }
            },
            Content = new ByteArrayContent(payload)
        };
        var response = await _client.SendAsync(request);
        var responseBytes = await response.Content.ReadAsByteArrayAsync();
        var resp = ProtoHelper.Deserialize<FlashTransferUploadResp>(responseBytes);

        if (resp.Status != "success")
        {
            _botContext.LogError(Tag,
                $"FlashTransfer Upload chunk {start} failed: {resp.Status} appId: {appId}, uploadIndex: {uploadIndex}, keyLength: {uKey?.Length ?? 0}");
            return false;
        }

        return true;
    }
}
