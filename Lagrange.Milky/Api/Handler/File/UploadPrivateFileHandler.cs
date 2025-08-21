using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.File;

[Api("upload_private_file")]
public class UploadPrivateFileHandler(BotContext bot, ResourceResolver resolver) : IApiHandler<UploadPrivateFileParameter, UploadPrivateFileResult>
{
    private readonly BotContext _bot = bot;
    private readonly ResourceResolver _resolver = resolver;

    public async Task<UploadPrivateFileResult> HandleAsync(UploadPrivateFileParameter parameter, CancellationToken token)
    {
        using var stream = await _resolver.ToMemoryStreamAsync(parameter.FileUri, token);
        (_, _) = await _bot.SendFriendFile(parameter.UserId, stream, parameter.FileName);

        // TODO: The URL cannot be located at this time
        return new UploadPrivateFileResult(string.Empty);
    }
}

public class UploadPrivateFileParameter(long userId, string fileUri, string fileName)
{
    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonRequired]
    [JsonPropertyName("file_uri")]
    public string FileUri { get; init; } = fileUri;

    [JsonRequired]
    [JsonPropertyName("file_name")]
    public string FileName { get; init; } = fileName;
}

public class UploadPrivateFileResult(string fileId)
{
    [JsonPropertyName("file_id")]
    public string FileId { get; } = fileId;
}