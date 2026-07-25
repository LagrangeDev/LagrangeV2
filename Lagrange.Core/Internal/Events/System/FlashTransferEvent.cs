using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Internal.Events.System;

internal class FlashTransferFile(string fileId, uint index, string fileName, Stream stream)
{
    public const uint DefaultFileType = 11;

    public string FileId { get; } = fileId;

    public uint Index { get; } = index;

    public string FileName { get; } = fileName;

    public uint FileType { get; } = ResolveFileType(fileName);

    public FlashTransferFileCategory Category => ResolveCategory(FileType);

    public Stream Stream { get; } = stream;

    public byte[] FileSha1 { get; } = stream.Sha1();

    public byte[] FileMd5 { get; } = stream.Md5();


    private static uint ResolveFileType(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".mp3" or ".wav" or ".aac" or ".flac" => 1,
            ".mp4" or ".avi" or ".mkv" or ".mov" or ".3gp" or ".mpeg" or ".rmvb" or ".rm" or ".wmv" or ".flv" or ".asf" or ".webm" or ".mpg" or ".vob" or ".m4v" or ".f4v" => 2,
            ".doc" or ".docx" => 3,
            ".zip" or ".rar" or ".tar" or ".gz" => 4,
            ".apk" => 5,
            ".xls" or ".xlsx" => 6,
            ".ppt" or ".pptx" => 7,
            ".html" or ".htm" => 8,
            ".pdf" => 9,
            ".txt" => 10,
            ".psd" => 12,
            ".pt" or ".pth" or ".onnx" or ".model" or ".mlmodel" => 15,
            ".ttf" or ".otf" => 16,
            ".ipa" => 17,
            ".key" => 18,
            ".note" => 19,
            ".numbers" => 20,
            ".pages" => 21,
            ".sketch" => 22,
            ".dmg" => 23,
            ".pkg" => 24,
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".heic" or ".heif" or ".dib" or ".ico" or ".avif" or ".tif" or ".tiff" => 26,
            _ => DefaultFileType
        };
    }

    private static FlashTransferFileCategory ResolveCategory(uint fileType)
    {
        return fileType switch
        {
            3 or 6 or 7 or 9 or 10 or 13 or 18 or 19 or 20 or 21 => FlashTransferFileCategory.Document,
            26 => FlashTransferFileCategory.Image,
            2 => FlashTransferFileCategory.Video,
            4 => FlashTransferFileCategory.Archive,
            25 => FlashTransferFileCategory.Folder,
            _ => FlashTransferFileCategory.Other
        };
    }

    private static string ResolveCategoryName(FlashTransferFileCategory category)
    {
        return category switch
        {
            FlashTransferFileCategory.Document => "文档",
            FlashTransferFileCategory.Image => "图片",
            FlashTransferFileCategory.Video => "视频",
            FlashTransferFileCategory.Archive => "压缩包",
            FlashTransferFileCategory.Folder => "文件夹",
            _ => "其他"
        };
    }
}

internal enum FlashTransferFileCategory : uint
{
    Document = 1,
    Image = 2,
    Video = 3,
    Archive = 4,
    Folder = 5,
    Other = 6
}


internal class FlashTransferCreateFileSetEventReq(string title, string asciiTitle, List<FlashTransferFile> files) : ProtocolEvent
{
    public string Title { get; } = title;

    public string AsciiTitle { get; } = asciiTitle;

    public List<FlashTransferFile> Files { get; } = files;

}

internal class FlashTransferCreateFileSetEventResp(string fileSetId, string shareLink) : ProtocolEvent
{
    public string FileSetId { get; } = fileSetId;

    public string ShareLink { get; } = shareLink;
}

internal class FlashTransferRegisterFilesEventReq(string fileSetId, List<FlashTransferFile> files) : ProtocolEvent
{
    public string FileSetId { get; } = fileSetId;

    public List<FlashTransferFile> Files { get; } = files;
}

internal class FlashTransferRegisterFilesEventResp : ProtocolEvent;

internal class FlashTransferQueryFileSetStatusEventReq(string fileSetId) : ProtocolEvent
{
    public string FileSetId { get; } = fileSetId;
}

internal class FlashTransferQueryFileSetStatusEventResp : ProtocolEvent;

internal class FlashTransferUploadAuthorizeEventReq(string fileSetId, FlashTransferFile file, uint scene) : ProtocolEvent
{
    public string FileSetId { get; } = fileSetId;

    public FlashTransferFile File { get; } = file;

    public uint Scene { get; } = scene;
}

internal class FlashTransferUploadAuthorizeEventResp(string uploadToken, string resourceKey, uint appId, string uploadHost, uint chunkSize, uint bindingStage, uint bindingField5, uint bindingField6) : ProtocolEvent
{
    public string UploadToken { get; } = uploadToken;

    public string ResourceKey { get; } = resourceKey;

    public uint AppId { get; } = appId;

    public string UploadHost { get; } = uploadHost;

    public uint ChunkSize { get; } = chunkSize;

    public uint BindingStage { get; } = bindingStage;

    public uint BindingField5 { get; } = bindingField5;

    public uint BindingField6 { get; } = bindingField6;
}

internal class FlashTransferUploadCompleteEventReq(string fileSetId, FlashTransferFile file, string resourceKey, uint scene, uint bindingStage, uint bindingField5, uint bindingField6) : ProtocolEvent
{
    public string FileSetId { get; } = fileSetId;

    public FlashTransferFile File { get; } = file;

    public string ResourceKey { get; } = resourceKey;

    public uint Scene { get; } = scene;

    public uint BindingStage { get; } = bindingStage;

    public uint BindingField5 { get; } = bindingField5;

    public uint BindingField6 { get; } = bindingField6;
}

internal class FlashTransferUploadCompleteEventResp : ProtocolEvent;

internal class FlashTransferUpdateFileSetStatusEventReq(string fileSetId, uint status) : ProtocolEvent
{
    public string FileSetId { get; } = fileSetId;

    public uint Status { get; } = status;
}

internal class FlashTransferUpdateFileSetStatusEventResp : ProtocolEvent;
