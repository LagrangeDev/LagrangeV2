using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class FileIncomingSegment(FileIncomingSegmentData data) : IncomingSegmentBase<FileIncomingSegmentData>(data)
{
    public FileIncomingSegment(string fileId, string fileName, long fileSize) : this(new FileIncomingSegmentData(fileId, fileName, fileSize, null)) { }

    public FileIncomingSegment(string fileId, string fileName, long fileSize, string fileHash) : this(new FileIncomingSegmentData(fileId, fileName, fileSize, fileHash)) { }
}

public class FileIncomingSegmentData(string fileId, string fileName, long fileSize, string? fileHash)
{
    [JsonPropertyName("file_id")]
    public string FileId { get; } = fileId;

    [JsonPropertyName("file_name")]
    public string FileName { get; } = fileName;

    [JsonPropertyName("file_size")]
    public long FileSize { get; } = fileSize;

    [JsonPropertyName("file_hash")]
    public string? FileHash { get; } = fileHash;
}