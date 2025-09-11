using System.Security.Cryptography;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Cryptography;

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

    public async Task<bool> UploadFile(string uKey, Stream bodyStream)
    {
        byte[] body;
        if (bodyStream is MemoryStream ms)
        {
            body = ms.ToArray();
        }
        else
        {
            bodyStream.Position = 0;
            var buffer = new byte[bodyStream.Length];
            await bodyStream.ReadExactlyAsync(buffer, 0, buffer.Length);
            body = buffer;
        }

        return await UploadFile(uKey, body);
    }

    public async Task<bool> UploadFile(string uKey, byte[] body)
    {
        var chunkSha1S = new FlashTransferSha1StateV { State = [] };
        var chunkBuffers = new List<byte[]>();
        var chunkCount = (uint)((body.Length + ChunkSize - 1) / ChunkSize);

        using var accStream = new MemoryStream();
        for (uint i = 0; i < chunkCount; i++)
        {
            var chunkBuffer = body.AsSpan((int)(i * ChunkSize), (int)Math.Min(ChunkSize, body.Length - i * ChunkSize))
                .ToArray();
            chunkBuffers.Add(chunkBuffer);

            if (i != chunkCount - 1)
            {
                accStream.Write(chunkBuffer, 0, chunkBuffer.Length);
                var accBytes = accStream.ToArray();
                var sha1Stream = new Sha1Stream();
                var digest = new byte[20];
                sha1Stream.Update(accBytes);
                sha1Stream.Hash(digest, false);
                chunkSha1S.State.Add(digest);
            }
            else
            {
                chunkSha1S.State.Add(SHA1.HashData(body));
            }
        }

        // cnm闹禅tx为什么不能并发,回答我
        foreach (var chunkBuffer in chunkBuffers)
        {
            var success = await UploadChunk(uKey, (uint)(chunkBuffers.IndexOf(chunkBuffer) * ChunkSize), chunkSha1S, chunkBuffer);
            if (!success)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> UploadChunk(string uKey, uint start, FlashTransferSha1StateV chunkSha1S, byte[] body)
    {
        var req = new FlashTransferUploadReq
        {
            FieId1 = 0,
            AppId = 1407,
            FileId3 = 2,
            Body = new FlashTransferUploadBody
            {
                FieId1 = [],
                UKey = uKey,
                Start = start,
                End = (uint)(start + body.Length - 1),
                Sha1 = SHA1.HashData(body),
                Sha1StateV = chunkSha1S,
                Body = body
            }
        };
        var payload = ProtoHelper.Serialize(req).ToArray();
        var request = new HttpRequestMessage(HttpMethod.Post, _url)
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
                $"FlashTransfer Upload chunk {start} failed: {resp.Status}");
            return false;
        }

        return true;
    }
}