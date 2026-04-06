using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Internal.Packets.Service;

internal static class NTV2RichMedia
{
    public static NTV2RichMediaReq BuildDownloadReq(BotMessage @struct, RichMediaEntityBase entity, DownloadExt ext)
    {
        if (entity.MsgInfo == null) throw new InvalidOperationException("MsgInfo is null");

        return new NTV2RichMediaReq
        {
            ReqHead = BuildHead(@struct, entity, 200),
            Download = new DownloadReq { Node = entity.MsgInfo.MsgInfoBody[0].Index, Download = ext }
        };
    }
    
    public static NTV2RichMediaReq BuildUploadReq(
        BotMessage @struct,
        RichMediaEntityBase entity, ExtBizInfo ext,
        params (uint, RichMediaEntityBase)[] subFileInfos)
    {
        if (entity.Stream == null) throw new InvalidOperationException("Stream is null");
        
        return new NTV2RichMediaReq
        {
            ReqHead = BuildHead(@struct, entity, 100),
            Upload = new UploadReq
            {
                UploadInfo = 
                [
                    new UploadInfo
                    {
                        FileInfo = BuildFileInfo(entity),
                        SubFileType = 0
                    }, 
                    ..subFileInfos.Select(x => new UploadInfo
                    { 
                        FileInfo = BuildFileInfo(x.Item2), 
                        SubFileType = x.Item1
                    })
                ],
                TryFastUploadCompleted = true,
                SrvSendMsg = false,
                ClientRandomId = (ulong)Random.Shared.Next(),
                CompatQMsgSceneType = @struct.IsGroup()? 2u : 1u ,
                ClientSeq = 10,
                ExtBizInfo = ext,
                NoNeedCompatMsg = false
            }
        };
    }

    private static MultiMediaReqHead BuildHead(BotMessage @struct, RichMediaEntityBase entity, uint cmd)
    {
        var (request, business) = entity switch
        {
            ImageEntity => (2u, 1u),
            RecordEntity => (2u, 3u),
            VideoEntity => (2u, 2u),
            _ => throw new ArgumentOutOfRangeException(nameof(entity))
        };

        return new MultiMediaReqHead
        {
            Common = new CommonHead { RequestId = 1, Command = cmd },
            Scene = BuildSceneInfo(@struct.Contact, request, business),
            Client = new ClientMeta { AgentType = 2 }
        };
    }
    
    private static SceneInfo BuildSceneInfo(BotContact contact, uint requestType, uint businessType)
    {
        var sceneInfo = new SceneInfo
        {
            RequestType = requestType,
            BusinessType = businessType,
        };
        
        if (contact is BotGroupMember member)
        {
            sceneInfo.SceneType = 2;
            sceneInfo.Group = new GroupInfo { GroupUin = member.Group.GroupUin, };
        }
        else
        {
            sceneInfo.SceneType = 1;
            sceneInfo.C2C = new C2CUserInfo { TargetUid = contact.Uid, AccountType = 2 };
        }
        return sceneInfo;
    }

    private static FileInfo BuildFileInfo(RichMediaEntityBase entity)
    {
        if (entity.Stream == null) throw new InvalidOperationException("MsgInfo is null");
        
        var stream = entity.Stream.Value;
        string md5 = Convert.ToHexString(entity.Stream.Value.Md5());
        string sha1 = Convert.ToHexString(entity.Stream.Value.Sha1());
        var info = new FileInfo
        {
            FileSize = (uint)stream.Length,
            FileHash = md5,
            FileSha1 = sha1
        };

        switch (entity)
        {
            case ImageEntity:
            {
                Span<byte> fileHead = stackalloc byte[10240];
                stream.Seek(0, SeekOrigin.Begin);
                _ = stream.Read(fileHead);
                stream.Seek(0, SeekOrigin.Begin);

                var type = ImageHelper.Resolve(fileHead, out var size);
                info.Type = new FileType { Type = 1, PicFormat = (uint)type };
                info.Width = (uint)size.X;
                info.Height = (uint)size.Y;
                info.FileName = $"{md5}.{type switch
                {
                    ImageFormat.Jpeg => "jpg",
                    ImageFormat.Png => "png",
                    ImageFormat.Gif => "gif",
                    ImageFormat.Bmp => "bmp",
                    _ => "unknown"
                }}";
                info.Original = 1;
                break;
            }
            case RecordEntity:
            {
                var payload = GC.AllocateUninitializedArray<byte>((int)entity.Stream.Value.Length);
                stream.Seek(0, SeekOrigin.Begin);
                _ = stream.Read(payload);
                stream.Seek(0, SeekOrigin.Begin);
                
                var type = AudioHelper.DetectAudio(payload);
                info.Type = new FileType { Type = 3, VoiceFormat = 1 };
                info.Time = type switch
                {
                    AudioFormat.TenSilkV3 => (uint)AudioHelper.GetTenSilkTime(payload),
                    AudioFormat.SilkV3 => (uint)AudioHelper.GetSilkTime(payload),
                    _ => 0
                };
                info.FileName = $"{md5}.{type switch
                {
                    AudioFormat.TenSilkV3 => "silk",
                    AudioFormat.SilkV3 => "silk",
                    AudioFormat.Mp3 => "mp3",
                    _ => "unknown"
                }}";
                break;
            }
            case VideoEntity:
            {
                info.Type.Type = 2; // unable to determine video type, skip
                info.FileName = $"{md5}.mp4"; // default to mp4
                break;
            }
        }
        
        return info;
    }
}