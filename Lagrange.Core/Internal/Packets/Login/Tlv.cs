using System.Security.Cryptography;
using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Cryptography;
using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Login;

internal ref struct Tlv : IDisposable
{
    private BinaryPacket _writer;

    private short _count;
    private readonly bool _prefixed;
    
    private readonly BotKeystore _keystore;
    private readonly BotAppInfo _appInfo;
    private readonly BotContext _context;

    public Tlv(short command, BotContext context)
    {
        _context = context;
        _keystore = context.Keystore;
        _appInfo = context.AppInfo;
        
        _writer = new BinaryPacket(1000);
        if (command > 0)
        {
            _writer.Write(command);
            _prefixed = true;
        }
        _writer.Skip(2);
    }

    public void Tlv001()
    {
        WriteTlv(0x1);
        
        _writer.Write<short>(0x0001);
        _writer.Write(Random.Shared.Next());
        _writer.Write((uint)_keystore.Uin);
        _writer.Write((uint)DateTimeOffset.Now.ToUnixTimeSeconds());
        _writer.Write(0); // dummy IP Address
        _writer.Write<short>(0x0000);

        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv008()
    {
        WriteTlv(0x08);

        _writer.Write<ushort>(0);
        _writer.Write(2052); // locale_id
        _writer.Write<ushort>(0);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv018()
    {
        WriteTlv(0x18);
        
        _writer.Write((short)0);
        _writer.Write(5u);
        _writer.Write(0u);
        _writer.Write(8001u); // app client ver
        _writer.Write((uint)_keystore.Uin);
        _writer.Write((short)0);
        _writer.Write((short)0);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv018Android()
    {
        WriteTlv(0x18);
        
        _writer.Write<short>(0x0001);
        _writer.Write(0x00000600u);
        _writer.Write(_appInfo.AppId);
        _writer.Write<int>(_appInfo.AppClientVersion);
        _writer.Write((uint)_keystore.Uin);
        _writer.Write<short>(0x0000);
        _writer.Write<short>(0x0000);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv100()
    {
        WriteTlv(0x100);
        
        _writer.Write((ushort)0); // db buf ver
        _writer.Write(5); // sso ver, dont over 7
        _writer.Write(_appInfo.AppId);
        _writer.Write(_appInfo.SubAppId);
        _writer.Write<int>(_appInfo.AppClientVersion); // app client ver
        _writer.Write((uint)_appInfo.SdkInfo.MainSigMap);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv100Android(uint mainSigMap)
    {
        WriteTlv(0x100);

        _writer.Write<ushort>(1); // db buf ver
        _writer.Write(_appInfo.SsoVersion); // sso ver, dont over 7
        _writer.Write(_appInfo.AppId);
        _writer.Write(_appInfo.SubAppId);
        _writer.Write<int>(_appInfo.AppClientVersion); // app client ver
        _writer.Write(mainSigMap);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv104(byte[] verificationToken)
    {
        WriteTlv(0x104);
        
        _writer.Write(verificationToken);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv106Pwd(string password)
    {
        WriteTlv(0x106);

        var md5 = MD5.HashData(Encoding.UTF8.GetBytes(password));
        
        var keyWriter = new BinaryPacket(stackalloc byte[16 + 4 + 4]);
        keyWriter.Write(md5);
        keyWriter.Write(0); // empty 4 bytes
        keyWriter.Write((uint)_keystore.Uin);
        var key = MD5.HashData(keyWriter.CreateReadOnlySpan());
        
        var plainWriter = new BinaryPacket(stackalloc byte[100]);
        plainWriter.Write<short>(4); // TGTGT Version
        plainWriter.Write(Random.Shared.Next());
        plainWriter.Write(_appInfo.SsoVersion);
        plainWriter.Write(_appInfo.AppId);
        plainWriter.Write<int>(_appInfo.AppClientVersion);
        plainWriter.Write(_keystore.Uin);
        plainWriter.Write((int)DateTimeOffset.Now.ToUnixTimeSeconds());
        plainWriter.Write(0); // dummy IP Address
        plainWriter.Write<byte>(1);
        plainWriter.Write(md5);
        plainWriter.Write(_keystore.WLoginSigs.TgtgtKey);
        plainWriter.Write(0);  // unknown
        plainWriter.Write<byte>(1); // guidAvailable
        plainWriter.Write(_keystore.Guid);
        plainWriter.Write(_appInfo.SubAppId);
        plainWriter.Write(1); // flag
        plainWriter.Write(_keystore.Uin.ToString(), Prefix.Int16 | Prefix.LengthOnly);
        plainWriter.Write<short>(0);
        _writer.Write(TeaProvider.Encrypt(plainWriter.CreateReadOnlySpan(), key));
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv106EncryptedA1()
    {
        WriteTlv(0x106);
        
        _writer.Write(_keystore.WLoginSigs.A1);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv107()
    {
        WriteTlv(0x107);

        _writer.Write((ushort)1); // pic type
        _writer.Write((byte)0x0D); // captcha type
        _writer.Write((ushort)0); // pic size
        _writer.Write((byte)1); // ret type
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv107Android()
    {
        WriteTlv(0x107);

        _writer.Write((ushort)0); // pic type
        _writer.Write((byte)0); // captcha type
        _writer.Write((ushort)0); // pic size
        _writer.Write((byte)1); // ret type
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv109()
    {
        WriteTlv(0x109);
        
        _writer.Write(MD5.HashData(Encoding.UTF8.GetBytes(_keystore.AndroidId)));
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv112(string qid)
    {
        WriteTlv(0x112);
        
        _writer.Write(qid);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv116()
    {
        WriteTlv(0x116);
        
        _writer.Write((byte)0); // version
        _writer.Write(_appInfo.SdkInfo.MiscBitMap); // miscBitMap
        _writer.Write(_appInfo.SdkInfo.SubSigMap);
        _writer.Write((byte)0); // length of subAppId
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv11B()
    {
        WriteTlv(0x11B);

        _writer.Write((byte)2);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv124()
    {
        WriteTlv(0x124);
        
        _writer.Skip(12);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv124Android()
    {
        WriteTlv(0x124);
        
        _writer.Write("android", Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write("13", Prefix.Int16 | Prefix.LengthOnly); // os version
        _writer.Write<short>(0x02); // network type
        _writer.Write("", Prefix.Int16 | Prefix.LengthOnly); // sim info
        _writer.Write("wifi", Prefix.Int32 | Prefix.LengthOnly); // apn
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv128()
    {
        WriteTlv(0x128);
        
        _writer.Write((ushort)0);
        _writer.Write((byte)0); // guid new
        _writer.Write((byte)0); // guid available
        _writer.Write((byte)0); // guid changed
        _writer.Write(0u); // guid flag
        _writer.Write(_appInfo.Os, Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write(_keystore.Guid, Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write(ReadOnlySpan<char>.Empty, Prefix.Int16 | Prefix.LengthOnly); // brand
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv141()
    {
        WriteTlv(0x141);
        
        _writer.Write((ushort)0);
        _writer.Write("Unknown", Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write(0u);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv141Android()
    {
        WriteTlv(0x141);
        
        _writer.Write((ushort)1);
        _writer.Write("", Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write("", Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write("wifi", Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv142()
    {
        WriteTlv(0x142);
        
        _writer.Write((ushort)0);
        _writer.Write(_appInfo.PackageName, Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv144()
    {
        var tlv = new Tlv(-1, _context);
        
        tlv.Tlv16E();
        tlv.Tlv147();
        tlv.Tlv128();
        tlv.Tlv124();

        var span = tlv.CreateReadOnlySpan();
        Span<byte> encrypted = stackalloc byte[TeaProvider.GetCipherLength(span.Length)];
        TeaProvider.Encrypt(span, encrypted, _keystore.WLoginSigs.TgtgtKey);
        tlv.Dispose();
        
        WriteTlv(0x144);
        
        _writer.Write(encrypted);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv144Report(bool useA1Key)
    {
        var tlv = new Tlv(-1, _context);
        
        tlv.Tlv109();
        tlv.Tlv52D();
        tlv.Tlv124Android();
        tlv.Tlv128();
        tlv.Tlv16E();

        var span = tlv.CreateReadOnlySpan();
        Span<byte> encrypted = stackalloc byte[TeaProvider.GetCipherLength(span.Length)];
        TeaProvider.Encrypt(span, encrypted, useA1Key ? _keystore.WLoginSigs.A1Key : _keystore.WLoginSigs.TgtgtKey);
        tlv.Dispose();
        
        WriteTlv(0x144);
        
        _writer.Write(encrypted);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv145()
    {
        WriteTlv(0x145);

        _writer.Write(_keystore.Guid);

        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv147()
    {
        WriteTlv(0x147);
        
        _writer.Write(_appInfo.AppId);
        _writer.Write(_appInfo.PtVersion, Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write(_appInfo.ApkSignatureMd5, Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv154()
    {
        WriteTlv(0x154);
        
        _writer.Write(0);  // seq
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv166()
    {
        WriteTlv(0x166);
        
        _writer.Write((byte)5);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv16A()
    {
        WriteTlv(0x16A);

        _writer.Write(_keystore.WLoginSigs.NoPicSig);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv16E()
    {
        WriteTlv(0x16E);
        
        _writer.Write(_keystore.DeviceName);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv174(byte[] session)
    {
        WriteTlv(0x174);
        
        _writer.Write(session);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv177()
    {
        WriteTlv(0x177);
        
        _writer.Write((byte)1);
        _writer.Write(0); // sdk build time
        _writer.Write(_appInfo.SdkInfo.SdkVersion, Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv17A()
    {
        WriteTlv(0x17A);
        
        _writer.Write(9);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv17C(string code)
    {
        WriteTlv(0x17C);
        
        _writer.Write(code, Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv187()
    {
        WriteTlv(0x187);
        
        _writer.Write(MD5.HashData([0x02, 0x00, 0x00, 0x00, 0x00, 0x00])); // Dummy Mac Address
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv188()
    {
        WriteTlv(0x188);
        
        _writer.Write(MD5.HashData(Encoding.UTF8.GetBytes(_keystore.AndroidId)));
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv191(byte k)
    {
        WriteTlv(0x191);
        
        _writer.Write(k);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv193(byte[] ticket)
    {
        WriteTlv(0x193);
        
        _writer.Write(ticket);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv197()
    {
        WriteTlv(0x197);
        
        _writer.Write<byte>(0);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv198()
    {
        WriteTlv(0x198);
        
        _writer.Write<byte>(0);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv318()
    {
        WriteTlv(0x318);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv400()
    {
        WriteTlv(0x400);
        
        var randomKey = new byte[16];
        RandomNumberGenerator.Fill(randomKey);
        var randSeed = new byte[8];
        RandomNumberGenerator.Fill(randSeed);
        
        var writer = new BinaryPacket(stackalloc byte[100]);
        writer.Write<short>(1);
        writer.Write(_keystore.Uin);
        writer.Write(_keystore.Guid);
        writer.Write(randomKey);
        writer.Write(16);
        writer.Write(1);
        writer.Write((uint)DateTimeOffset.Now.ToUnixTimeSeconds());
        writer.Write(randSeed);
        
        var span = writer.CreateReadOnlySpan();
        Span<byte> encrypted = stackalloc byte[TeaProvider.GetCipherLength(span.Length)];
        TeaProvider.Encrypt(span, encrypted, _keystore.Guid);

        _writer.Write(encrypted);
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv401()
    {
        WriteTlv(0x401);

        Span<byte> random = stackalloc byte[16];
        Random.Shared.NextBytes(random);
        _writer.Write(random);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv511()
    {
        WriteTlv(0x511);

        string[] domains =
        [
            "office.qq.com",
            "qun.qq.com",
            "gamecenter.qq.com",
            "docs.qq.com",
            "mail.qq.com",
            "tim.qq.com",
            "ti.qq.com",
            "vip.qq.com",
            "tenpay.com",
            "qqweb.qq.com",
            "qzone.qq.com",
            "mma.qq.com",
            "game.qq.com",
            "openmobile.qq.com",
            "connect.qq.com"
        ];
        
        _writer.Write<short>((short)domains.Length);
        foreach (string domain in domains)
        {
            _writer.Write<byte>(1);
            _writer.Write(domain, Prefix.Int16 | Prefix.LengthOnly);
        }
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv516()
    {
        WriteTlv(0x516);
        
        _writer.Write(0);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv521()
    {
        WriteTlv(0x521);
        
        _writer.Write(0x13);
        _writer.Write("basicim", Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv521Android()
    {
        WriteTlv(0x521);
        
        _writer.Write(0);
        _writer.Write("", Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv525()
    {
        WriteTlv(0x525);
        
        _writer.Write<short>(1); // tlvCount
        _writer.Write<short>(0x536); // tlv536
        _writer.Write([0x02, 0x01, 0x00], Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv52D()
    {
        WriteTlv(0x52D);
        
        var report = new DeviceReport
        {
            BootId = "unknown",
            ProcVersion = "Linux version 4.19.157-perf-g92c089fc2d37 (builder@pangu-build-component-vendor-272092-qncbv-vttl3-61r9m) (clang version 10.0.7 for Android NDK, GNU ld (binutils-2.27-bd24d23f) 2.27.0.20170315) #1 SMP PREEMPT Wed Jun 5 13:27:08 UTC 2024",
            CodeName = "REL",
            Bootloader = "V816.0.6.0.TKHCNXM",
            Fingerprint = "Redmi/alioth/alioth:13/TKQ1.221114.001/V816.0.6.0.TKHCNXM:user/release-keys",
            AndroidId = _keystore.AndroidId,
            BaseBand = "",
            InnerVersion = "V816.0.6.0.TKHCNXM"
        };
        ProtoHelper.Serialize(ref _writer, report);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv544(byte[] energy)
    {
        WriteTlv(0x544);
        
        _writer.Write(energy);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv545()
    {
        WriteTlv(0x545);
        
        _writer.Write(_keystore.Qimei);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv547(byte[] clientPow)
    {
        WriteTlv(0x547);
        
        _writer.Write(clientPow);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv548(byte[] nativeGetTestData)
    {
        WriteTlv(0x548);
        
        _writer.Write(nativeGetTestData);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv553(byte[] fekitAttach)
    {
        WriteTlv(0x553);
        
        _writer.Write(fekitAttach);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public ReadOnlySpan<byte> CreateReadOnlySpan()
    {
        _writer.Write(_count, _prefixed ? 2 : 0);
        return _writer.CreateReadOnlySpan();
    }

    private void WriteTlv(short tag)
    {
        _writer.Write(tag);
        _writer.EnterLengthBarrier<short>();
        _count++;
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
}

[ProtoPackable]
internal partial class DeviceReport
{
    [ProtoMember(1)] public string? Bootloader { get; set; }
    
    [ProtoMember(2)] public string? ProcVersion { get; set; }
    
    [ProtoMember(3)] public string? CodeName { get; set; }
    
    [ProtoMember(4)] public string? Incremental { get; set; }
    
    [ProtoMember(5)] public string? Fingerprint { get; set; }
    
    [ProtoMember(6)] public string? BootId { get; set; }
    
    [ProtoMember(7)] public string? AndroidId { get; set; }
    
    [ProtoMember(8)] public string? BaseBand { get; set; }
    
    [ProtoMember(9)] public string? InnerVersion { get; set; }
}