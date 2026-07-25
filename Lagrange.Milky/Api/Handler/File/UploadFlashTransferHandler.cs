using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.File;

[Api("upload_flash_transfer")]
public class UploadFlashTransferHandler(BotContext bot, ResourceResolver resolver) : IApiHandler<UploadFlashTransferParameter, UploadFlashTransferResult>
{
    private readonly BotContext _bot = bot;
    private readonly ResourceResolver _resolver = resolver;

    public async Task<UploadFlashTransferResult> HandleAsync(UploadFlashTransferParameter parameter, CancellationToken token)
    {
        var streams = new List<MemoryStream>();
        try
        {
            foreach (var file in parameter.Files)
            {
                streams.Add(await _resolver.ToMemoryStreamAsync(file.FileUri, token));
            }

            var files = streams.Select((stream, index) => ((Stream)stream, parameter.Files[index].FileName)).ToList();
            var result = await _bot.UploadFlashTransfer(files, parameter.Title);
            return new UploadFlashTransferResult(result.FileSetId, result.FileIds, result.ShareLink);
        }
        finally
        {
            foreach (var stream in streams) stream.Dispose();
        }
    }
}

public class UploadFlashTransferParameter(List<UploadFlashTransferFileParameter> files, string? title = null)
{
    [JsonRequired]
    [JsonPropertyName("files")]
    public List<UploadFlashTransferFileParameter> Files { get; init; } = files;

    [JsonPropertyName("fileset_name")]
    public string? Title { get; init; } = title;
}

public class UploadFlashTransferFileParameter(string fileUri, string? fileName = null)
{
    [JsonRequired]
    [JsonPropertyName("file_uri")]
    public string FileUri { get; init; } = fileUri;

    [JsonPropertyName("file_name")]
    public string? FileName { get; init; } = fileName;
}

public class UploadFlashTransferResult(string fileSetId, List<string> fileIds, string shareUrl)
{
    [JsonPropertyName("fileset_id")]
    public string FileSetId { get; } = fileSetId;

    [JsonPropertyName("file_ids")]
    public List<string> FileIds { get; } = fileIds;

    [JsonPropertyName("share_url")]
    public string ShareUrl { get; } = shareUrl;
}