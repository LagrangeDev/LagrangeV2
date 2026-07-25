namespace Lagrange.Core.Common.Response;

public class BotFlashTransferUpload(string fileSetId, List<string> fileIds, string shareLink)
{
    public string FileSetId { get; } = fileSetId;

    public List<string> FileIds { get; } = fileIds;

    public string ShareLink { get; } = shareLink;
}
