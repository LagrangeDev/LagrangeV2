using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Cryptography;

namespace Lagrange.Core.Internal.Packets.Login;

internal class WtLogin : StructBase
{
    private static readonly byte[] ServerPublicKey = [0x04, 0x92, 0x8D, 0x88, 0x50, 0x67, 0x30, 0x88, 0xB3, 0x43, 0x26, 0x4E, 0x0C, 0x6B, 0xAC, 0xB8, 0x49, 0x6D, 0x69, 0x77, 0x99, 0xF3, 0x72, 0x11, 0xDE, 0xB2, 0x5B, 0xB7, 0x39, 0x06, 0xCB, 0x08, 0x9F, 0xEA, 0x96, 0x39, 0xB4, 0xE0, 0x26, 0x04, 0x98, 0xB5, 0x1A, 0x99, 0x2D, 0x50, 0x81, 0x3D, 0xA8];

    private readonly byte[] _shareKey;
    
    private readonly BotContext _context;
    
    public WtLogin(BotContext context) : base(context)
    {
        _context = context;
        _shareKey = Keystore.Secp192K1.KeyExchange(ServerPublicKey, true);
    }
    
    public ReadOnlyMemory<byte> BuildTransEmp31(byte[]? unusualSig)
    {
        using var writer = new BinaryPacket(stackalloc byte[300]);
        writer.Write<ushort>(0);
        writer.Write(AppInfo.AppId);
        writer.Write<ulong>(0); // uin
        writer.Write(ReadOnlySpan<byte>.Empty); // TGT
        writer.Write<byte>(0);
        writer.Write(ReadOnlySpan<byte>.Empty, Prefix.Int16 | Prefix.LengthOnly);
        
        using var tlvs = new TlvQrCode(_context);
        if (unusualSig != null) tlvs.Tlv11(unusualSig);
        tlvs.Tlv16();
        tlvs.Tlv1B();
        tlvs.Tlv1D();
        tlvs.Tlv33();
        tlvs.Tlv35();
        tlvs.Tlv66();
        tlvs.TlvD1();
        
        writer.Write(tlvs.CreateReadOnlySpan());

        return BuildCode2dPacket(0x31, writer.CreateReadOnlySpan(), EncryptMethod.EM_ECDH_ST);
    }

    public ReadOnlyMemory<byte> BuildTransEmp12()
    {
        using var writer = new BinaryPacket(stackalloc byte[100]);
        writer.Write<ushort>(0);
        writer.Write(AppInfo.AppId);
        writer.Write(Keystore.State.QrSig, Prefix.Int16 | Prefix.LengthOnly);
        writer.Write<ulong>(0); // uin
        writer.Write(ReadOnlySpan<byte>.Empty); // TGT
        writer.Write<byte>(0);
        writer.Write(ReadOnlySpan<byte>.Empty, Prefix.Int16 | Prefix.LengthOnly);
        writer.Write<ushort>(0); // tlv count = 0

        return BuildCode2dPacket(0x12, writer.CreateReadOnlySpan(), EncryptMethod.EM_ECDH_ST);
    }

    public ReadOnlyMemory<byte> BuildQrlogin19(byte[] k) // VerifyCode
    {
        using var writer = new BinaryPacket(stackalloc byte[300]);
        writer.Write<ushort>(0);
        writer.Write(AppInfo.AppId);
        writer.Write(Keystore.Uin);
        writer.Write(k, Prefix.Int16 | Prefix.LengthOnly); // code in java, k in qrcode url
        writer.Write(Keystore.WLoginSigs.A2, Prefix.Int16 | Prefix.LengthOnly);
        writer.Write(Keystore.Guid);
        
        writer.Write<byte>(1);
        writer.Write<short>(1);
        writer.Write<byte>(8);

        var t = (Span<short>) [0x03, 0x05, 0x20, 0x35, 0x36];
        writer.Write((short)t.Length);
        foreach (short tlv in t) // Tencent named it tlv....
        {
            writer.Write(tlv);
        }
        
        using var tlvs = new TlvQrCode(_context);
        tlvs.Tlv09();
        tlvs.Tlv12C();
        tlvs.Tlv39();
        
        writer.Write(tlvs.CreateReadOnlySpan());
        
        return BuildCode2dPacket(0x13, writer.CreateReadOnlySpan(), EncryptMethod.EM_ST, true, true);
    }
    
    public ReadOnlyMemory<byte> BuildQrlogin20(byte[] k) // CloseCode
    {
        using var writer = new BinaryPacket(stackalloc byte[300]);
        writer.Write<ushort>(0);
        writer.Write(AppInfo.AppId);
        writer.Write(Keystore.Uin);
        writer.Write(k, Prefix.Int16 | Prefix.LengthOnly); // code in java, k in qrcode url
        writer.Write(Keystore.WLoginSigs.A2, Prefix.Int16 | Prefix.LengthOnly);
        
        writer.Write<byte>(8);
        using var tlvs = new TlvQrCode(_context);
        tlvs.Tlv02();
        tlvs.Tlv04();
        tlvs.Tlv15();
        tlvs.Tlv68();
        tlvs.Tlv16();
        tlvs.Tlv18();
        tlvs.Tlv19();
        tlvs.Tlv1D();
        tlvs.Tlv12C();
        
        writer.Write(tlvs.CreateReadOnlySpan());
        
        return BuildCode2dPacket(0x14, writer.CreateReadOnlySpan(), EncryptMethod.EM_ST, true, true);
    }
    
    public ReadOnlyMemory<byte> BuildQrlogin22(byte[] k)
    {
        using var writer = new BinaryPacket(stackalloc byte[300]);
        writer.Write<ushort>(0);
        writer.Write(AppInfo.AppId);
        writer.Write(k, Prefix.Int16 | Prefix.LengthOnly); // code in java, k in qrcode url
        writer.Write(_context.Keystore.Uin); // uin
        writer.Write<byte>(8);
        writer.Write(Keystore.WLoginSigs.A2, Prefix.Int16 | Prefix.LengthOnly);
        
        writer.Write<short>(0);
        using var tlvs = new TlvQrCode(_context);
        tlvs.Tlv12C();
        
        writer.Write(tlvs.CreateReadOnlySpan());
        
        return BuildCode2dPacket(0x16, writer.CreateReadOnlySpan(), EncryptMethod.EM_ST, true, true);
    }

    public ReadOnlyMemory<byte> BuildOicq09()
    {
        using var tlvs = new Tlv(0x09, _context);
        
        tlvs.Tlv106EncryptedA1();
        tlvs.Tlv144();
        tlvs.Tlv116();
        tlvs.Tlv142();
        tlvs.Tlv145();
        tlvs.Tlv018();
        tlvs.Tlv141();
        tlvs.Tlv177();
        tlvs.Tlv191(0);
        tlvs.Tlv100();
        tlvs.Tlv107();
        tlvs.Tlv318();
        tlvs.Tlv16A();
        tlvs.Tlv166();
        tlvs.Tlv521();
        
        return BuildPacket(0x810, tlvs.CreateReadOnlySpan());
    }
    
    public async Task<ReadOnlyMemory<byte>> BuildOicq09Android(string password)
    {
        var sign = (AndroidBotSignProvider)_context.PacketContext.SignProvider;
        var energy = await sign.GetEnergy(_context.BotUin, "810_9");
        var attach = await sign.GetDebugXwid(_context.BotUin, "810_9");
        
        using var tlvs = new Tlv(0x09, _context); // if login by email, add tlv104(sessionToken) and tlv112(username / email)
        
        tlvs.Tlv018Android();
        tlvs.Tlv001();
        tlvs.Tlv106Pwd(password);
        tlvs.Tlv116();
        tlvs.Tlv100Android((uint)AppInfo.SdkInfo.MainSigMap);
        tlvs.Tlv107Android();
        tlvs.Tlv142();
        tlvs.Tlv144Report(false);
        tlvs.Tlv145();
        tlvs.Tlv147();
        tlvs.Tlv154();
        tlvs.Tlv141Android();
        tlvs.Tlv008();
        tlvs.Tlv511();
        tlvs.Tlv187();
        tlvs.Tlv188();
        tlvs.Tlv191(0x82);
        tlvs.Tlv177();
        tlvs.Tlv516();
        tlvs.Tlv521Android();
        tlvs.Tlv525();
        tlvs.Tlv544(energy);
        tlvs.Tlv545();
        tlvs.Tlv548(PowProvider.GenerateTlv548());
        tlvs.Tlv553(attach);
        // 542 smsExtraData
        
        return BuildPacket(0x810, tlvs.CreateReadOnlySpan());
    }

    public async Task<ReadOnlyMemory<byte>> BuildOicq02Android(string ticket)
    {
        var sign = (AndroidBotSignProvider)_context.PacketContext.SignProvider;
        var energy = await sign.GetEnergy(_context.BotUin, "810_2");
        var attach = await sign.GetDebugXwid(_context.BotUin, "810_2");
        
        using var tlvs = new Tlv(0x02, _context);
        
        tlvs.Tlv193(Encoding.UTF8.GetBytes(ticket));
        tlvs.Tlv008();
        if (Keystore.State.Tlv104 is { } tlv104) tlvs.Tlv104(tlv104);
        tlvs.Tlv116();
        if (Keystore.State.Tlv547 is { } tlv547) tlvs.Tlv547(tlv547);
        tlvs.Tlv544(energy);
        tlvs.Tlv553(attach);
        // 542 smsExtraData
        
        return BuildPacket(0x810, tlvs.CreateReadOnlySpan());
    }

    public async Task<ReadOnlyMemory<byte>> BuildOicq04Android(string qid)
    {
        var sign = (AndroidBotSignProvider)_context.PacketContext.SignProvider;
        var attach = await sign.GetDebugXwid(_context.BotUin, "810_4");
        
        using var tlvs = new Tlv(0x04, _context);
        
        tlvs.Tlv100();
        tlvs.Tlv112(qid);
        tlvs.Tlv107Android();
        tlvs.Tlv154();
        tlvs.Tlv008();
        tlvs.Tlv553(attach);
        tlvs.Tlv521Android();
        tlvs.Tlv124Android();
        tlvs.Tlv128();
        tlvs.Tlv116();
        tlvs.Tlv191(0x82);
        tlvs.Tlv11B();
        tlvs.Tlv52D();
        tlvs.Tlv548(PowProvider.GenerateTlv548());
        // 542 smsExtraData
        
        return BuildPacket(0x810, tlvs.CreateReadOnlySpan(), EncryptMethod.EM_ECDH);
    }

    public async Task<ReadOnlyMemory<byte>> BuildOicq07Android(string code)
    {
        var sign = (AndroidBotSignProvider)_context.PacketContext.SignProvider;
        var energy = await sign.GetEnergy(_context.BotUin, "810_7");
        var attach = await sign.GetDebugXwid(_context.BotUin, "810_7");
        
        using var tlvs = new Tlv(0x07, _context);
        tlvs.Tlv008();
        if (Keystore.State.Tlv104 is { } tlv104) tlvs.Tlv104(tlv104);
        tlvs.Tlv116();
        if (Keystore.State.Tlv174 is { } tlv174) tlvs.Tlv174(tlv174);
        tlvs.Tlv17C(code);
        tlvs.Tlv401();
        tlvs.Tlv198();
        // 542 smsExtraData
        tlvs.Tlv544(energy);
        tlvs.Tlv553(attach);
        
        return BuildPacket(0x810, tlvs.CreateReadOnlySpan());
    }
    
    public async Task<ReadOnlyMemory<byte>> BuildOicq08Android()
    {
        var sign = (AndroidBotSignProvider)_context.PacketContext.SignProvider;
        var attach = await sign.GetDebugXwid(_context.BotUin, "810_8");
        
        using var tlvs = new Tlv(0x08, _context);
        tlvs.Tlv008();
        if (Keystore.State.Tlv104 is { } tlv104) tlvs.Tlv104(tlv104);
        tlvs.Tlv116();
        if (Keystore.State.Tlv174 is { } tlv174) tlvs.Tlv174(tlv174);
        tlvs.Tlv17A();
        tlvs.Tlv197();
        // 542 smsExtraData
        tlvs.Tlv553(attach);

        
        return BuildPacket(0x810, tlvs.CreateReadOnlySpan());
    }
    
    public async Task<ReadOnlyMemory<byte>> BuildOicq15Android()
    {
        var sign = (AndroidBotSignProvider)_context.PacketContext.SignProvider;
        var energy = await sign.GetEnergy(_context.BotUin, "810_f");
        var attach = await sign.GetDebugXwid(_context.BotUin, "810_f");
        
        using var tlvs = new Tlv(0x0f, _context);
        tlvs.Tlv018Android();
        tlvs.Tlv001();
        tlvs.Tlv106EncryptedA1();
        tlvs.Tlv116();
        tlvs.Tlv100Android(34607328);
        tlvs.Tlv107Android();
        tlvs.Tlv144Report(true);
        tlvs.Tlv142();
        tlvs.Tlv145();
        tlvs.Tlv16A();
        tlvs.Tlv154();
        tlvs.Tlv141Android();
        tlvs.Tlv008();
        tlvs.Tlv511();
        tlvs.Tlv147();
        tlvs.Tlv177();
        tlvs.Tlv400();
        tlvs.Tlv187();
        tlvs.Tlv188();
        tlvs.Tlv516();
        tlvs.Tlv521Android();
        tlvs.Tlv525();
        tlvs.Tlv544(energy);
        tlvs.Tlv553(attach);
        tlvs.Tlv545();
        
        return BuildPacket(0x810, tlvs.CreateReadOnlySpan());
    }
    
    private ReadOnlyMemory<byte> BuildPacket(
        short command, 
        scoped ReadOnlySpan<byte> payload,
        EncryptMethod method = EncryptMethod.EM_ECDH_ST,
        bool useWtSession = false) // corrected
    {
        ReadOnlySpan<byte> key;

        switch (method)
        {
            case EncryptMethod.EM_ECDH or EncryptMethod.EM_ECDH_ST:
                key = _shareKey;
                break;
            case EncryptMethod.EM_ST:
                key = useWtSession ? Keystore.WLoginSigs.WtSessionTicketKey : Keystore.WLoginSigs.RandomKey;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }
        
        int cipherLength = TeaProvider.GetCipherLength(payload.Length);
        var writer = new BinaryPacket(cipherLength + 80);
        Span<byte> cipher = stackalloc byte[cipherLength];
        TeaProvider.Encrypt(payload, cipher, key);
        
        writer.Write((byte)2); // getRequestEncrptedPackage
        writer.EnterLengthBarrier<short>();
        
        writer.Write((short)8001); // version
        writer.Write(command);
        writer.Write((short)0); // sequence
        writer.Write((uint)Keystore.Uin);
        writer.Write((byte)3);
        writer.Write((byte)method);
        writer.Write(0);
        writer.Write((byte)2);
        writer.Write((short)0); // insId
        writer.Write(AppInfo.AppClientVersion); // insId
        writer.Write(0); // retryTime
        BuildEncryptHead(ref writer, useWtSession);
        writer.Write(cipher);
        writer.Write((byte)3);
        
        writer.ExitLengthBarrier<short>(true, 1);

        return writer.ToArray();
    }

    private ReadOnlyMemory<byte> BuildCode2dPacket(
        short command,
        scoped ReadOnlySpan<byte> tlv,
        EncryptMethod method,
        bool encrypt = false,
        bool useWtSession = false)
    {
        using var reqBody = new BinaryPacket(stackalloc byte[48 + tlv.Length]);
        reqBody.Write((uint)DateTimeOffset.Now.ToUnixTimeSeconds());
        
        reqBody.Write((byte)2); // encryptMethod == EncryptMethod.EM_ST || encryptMethod == EncryptMethod.EM_ECDH_ST | Section of length 43 + tlv.Length + 1
        reqBody.EnterLengthBarrier<short>();
        reqBody.Write(command);
        reqBody.Skip(21);
        reqBody.Write((byte)3); // flag, 4 for oidb_func, 1 for register, 3 for code_2d, 2 for name_func, 5 for devlock
        reqBody.Write((short)0x00); // close
        reqBody.Write((short)0x32); // Version Code: 50
        reqBody.Write(0); // trans_emp sequence
        reqBody.Write(Keystore.Uin); // dummy uin
        reqBody.Write(tlv);
        reqBody.Write((byte)3); // oicq.wlogin_sdk.code2d.c.get_request
        reqBody.ExitLengthBarrier<short>(true, 1);
        
        var reqSpan = encrypt
            ? TeaProvider.Encrypt(reqBody.CreateReadOnlySpan(), Keystore.WLoginSigs.StKey)
            : reqBody.CreateReadOnlySpan();
        using var writer = new BinaryPacket(stackalloc byte[14 + reqSpan.Length]);
        writer.Write(Convert.ToByte(encrypt)); // flag for encrypt, if 1, encrypt by StKey
        writer.Write((ushort)reqSpan.Length);
        writer.Write(AppInfo.AppId);
        writer.Write((uint)0x72); // Role
        writer.Write(encrypt ? Keystore.WLoginSigs.St : ReadOnlySpan<byte>.Empty, Prefix.Int16 | Prefix.LengthOnly); // uSt
        writer.Write(ReadOnlySpan<byte>.Empty, Prefix.Int8 | Prefix.LengthOnly); // rollback
        writer.Write(reqSpan); // oicq.wlogin_sdk.request.d0

        return BuildPacket(0x812, writer.CreateReadOnlySpan(), method, useWtSession);
    }

    private void BuildEncryptHead(ref BinaryPacket writer, bool useWtSession)
    {
        if (useWtSession)
        {
            writer.Write(Keystore.WLoginSigs.WtSessionTicket, Prefix.Int16 | Prefix.LengthOnly);
        }
        else
        {
            writer.Write((byte)1);
            writer.Write((byte)1);
            writer.Write(Keystore.WLoginSigs.RandomKey);
            writer.Write((short)0x102); // encrypt type
            writer.Write(Keystore.Secp192K1.PackPublic(true), Prefix.Int16 | Prefix.LengthOnly);
        }
    }

    public ReadOnlySpan<byte> Parse(ReadOnlySpan<byte> input, out ushort command)
    {
        var reader = new BinaryPacket(input);
        
        byte header = reader.Read<byte>();
        ushort length = reader.Read<ushort>();
        ushort version = reader.Read<ushort>();
        command = reader.Read<ushort>();
        ushort sequence = reader.Read<ushort>();
        uint uin = reader.Read<uint>();
        byte flag = reader.Read<byte>();
        byte encryptType = reader.Read<byte>();
        byte state = reader.Read<byte>();
        var encrypted = reader.ReadBytes()[..^1]; // remove last byte

        byte[] key;
        switch (encryptType)
        {
            case 0:
            {
                key = state == 180 ? Keystore.WLoginSigs.RandomKey : _shareKey;
                break;
            }
            case 3:
            {
                key = Keystore.WLoginSigs.WtSessionTicketKey;
                break;
            }
            case 4:
            {
                var raw = TeaProvider.Decrypt(encrypted, _shareKey);
                var rawReader = new BinaryPacket(raw.AsSpan());
                var publicKey = rawReader.ReadBytes(Prefix.Int16 | Prefix.LengthOnly);
                key = Keystore.Secp192K1.KeyExchange(publicKey.ToArray(), true);
                encrypted = rawReader.ReadBytes();
                break;
            }
            default:
            {
                throw new Exception($"Unknown encrypt type: {encryptType}");
            }
        }
        
        var span = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(encrypted), encrypted.Length);
        TeaProvider.Decrypt(span, span, key);
        return TeaProvider.CreateDecryptSpan(span);
    }

    public ReadOnlySpan<byte> ParseCode2dPacket(ReadOnlySpan<byte> input, out ushort command)
    {
        byte encrypt = input[1];
        short layer = BinaryPrimitives.ReadInt16BigEndian(input[2..]);

        var span = encrypt == 0 ? input.Slice(5, layer) : TeaProvider.Decrypt(input.Slice(5, layer), _context.Keystore.WLoginSigs.StKey);
        var reader = new BinaryPacket(span);
        
        byte header = reader.Read<byte>();
        ushort length = reader.Read<ushort>();
        command = reader.Read<ushort>();
        reader.Skip(21);
        byte flag = reader.Read<byte>();
        ushort retryTime = reader.Read<ushort>();
        ushort version = reader.Read<ushort>();
        uint sequence = reader.Read<uint>();
        long uin = reader.Read<long>();

        return reader.ReadBytes();
    }
    
    public enum EncryptMethod : byte
    {
        EM_ST = 0x45,
        EM_ECDH = 0x07,
        EM_ECDH_ST = 0x87 // same with EM_ECDH, but controlled with a flag, if flag is set to 1, the ST would be used
    }
}