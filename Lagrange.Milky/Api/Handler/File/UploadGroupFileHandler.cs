using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.File;

[Api("upload_group_file")]
public class UploadGroupFileHandler(BotContext bot, ResourceResolver resolver) : IApiHandler<UploadGroupFileParameter, UploadGroupFileResult>
{
    private readonly BotContext _bot = bot;
    private readonly ResourceResolver _resolver = resolver;

    public async Task<UploadGroupFileResult> HandleAsync(UploadGroupFileParameter parameter, CancellationToken token)
    {
        using var stream = await _resolver.ToMemoryStreamAsync(parameter.FileUri, token);
        string id = await _bot.SendGroupFile(parameter.GroupId, stream, parameter.FileName, parameter.ParentFolderId);

        return new UploadGroupFileResult(id);
    }
}

public class UploadGroupFileParameter(long groupId, string fileUri, string fileName, string parentFolderId = "/")
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("file_uri")]
    public string FileUri { get; init; } = fileUri;

    [JsonRequired]
    [JsonPropertyName("file_name")]
    public string FileName { get; init; } = fileName;

    [JsonPropertyName("parent_folder_id")]
    public string ParentFolderId { get; } = parentFolderId;
}

public class UploadGroupFileResult(string fileId)
{
    [JsonPropertyName("file_id")]
    public string FileId { get; } = fileId;
}