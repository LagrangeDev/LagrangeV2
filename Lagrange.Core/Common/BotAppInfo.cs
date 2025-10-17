#pragma warning disable CS8618

namespace Lagrange.Core.Common;

public class BotAppInfo
{
    public string Os { get; init; }
    
    public string VendorOs { get; init; }
    
    public string Kernel { get; init; }

    public string CurrentVersion { get; init; }
    
    public string PtVersion { get; init; }
    
    public int SsoVersion { get; init; }
    
    public string PackageName { get; init; }
    
    public byte[] ApkSignatureMd5 { get; init; }
    
    public WtLoginSdkInfo SdkInfo { get; init; }
    
    public int AppId { get; init; }
    
    public int SubAppId { get; init; }

    public ushort AppClientVersion { get; init; }

    private static readonly BotAppInfo Linux = new()
    {
        Os = "Linux",
        Kernel = "Linux",
        VendorOs = "linux",
        CurrentVersion = "3.2.19-39038",
        PtVersion = "2.0.0",
        SsoVersion = 19,
        PackageName = "com.tencent.qq",
        ApkSignatureMd5 = "com.tencent.qq"u8.ToArray(),
        SdkInfo = new WtLoginSdkInfo
        {
            SdkBuildTime = 0,
            SdkVersion = "nt.wtlogin.0.0.1",
            MiscBitMap = 12058620,
            SubSigMap = 0,
            MainSigMap = (Sig)169742560,// Sig.WLOGIN_STWEB | Sig.WLOGIN_A2 | Sig.WLOGIN_ST | Sig.WLOGIN_SKEY | Sig.WLOGIN_VKEY | Sig.WLOGIN_D2 | Sig.WLOGIN_SID | Sig.WLOGIN_PSKEY | Sig.WLOGIN_DA2 | Sig.WLOGIN_PT4Token
        },
        AppId = 1600001615,
        SubAppId = 537313942,
        AppClientVersion = 39038
    };
    
    private static readonly BotAppInfo MacOs = new()
    {
        Os = "Mac",
        Kernel = "Darwin",
        VendorOs = "mac",
        CurrentVersion = "6.9.23-20139",
        PtVersion = "2.0.0",
        SsoVersion = 23,
        PackageName = "com.tencent.qq",
        ApkSignatureMd5 = "com.tencent.qq"u8.ToArray(),
        SdkInfo = new WtLoginSdkInfo
        {
            SdkBuildTime = 0,
            SdkVersion = "nt.wtlogin.0.0.1",
            MiscBitMap = 12058620,
            SubSigMap = 0,
            MainSigMap = Sig.WLOGIN_STWEB | Sig.WLOGIN_A2 | Sig.WLOGIN_ST | Sig.WLOGIN_SKEY | Sig.WLOGIN_VKEY | Sig.WLOGIN_D2 | Sig.WLOGIN_SID | Sig.WLOGIN_PSKEY | Sig.WLOGIN_DA2 | Sig.WLOGIN_PT4Token
        },
        AppId = 1600001602,
        SubAppId = 537200848,
        AppClientVersion = 13172
    };
    
    private static readonly BotAppInfo Windows = new()
    {
        Os = "Windows",
        Kernel = "Windows_NT",
        VendorOs = "win32",
        CurrentVersion = "9.9.19-35184",
        PtVersion = "2.0.0",
        SsoVersion = 23,
        PackageName = "com.tencent.qq",
        ApkSignatureMd5 = "com.tencent.qq"u8.ToArray(),
        SdkInfo = new WtLoginSdkInfo
        {
            SdkBuildTime = 0,
            SdkVersion = "nt.wtlogin.0.0.1",
            MiscBitMap = 12058620,
            SubSigMap = 0,
            MainSigMap = Sig.WLOGIN_STWEB | Sig.WLOGIN_A2 | Sig.WLOGIN_ST | Sig.WLOGIN_SKEY | Sig.WLOGIN_VKEY | Sig.WLOGIN_D2 | Sig.WLOGIN_SID | Sig.WLOGIN_PSKEY | Sig.WLOGIN_DA2 | Sig.WLOGIN_PT4Token
        },
        AppId = 1600001604,
        SubAppId = 537291048,
        AppClientVersion = 35184
    };

    private static readonly BotAppInfo AndroidPhone = new()
    {
        Os = "Android",
        CurrentVersion = "9.1.60.045f5d19",
        PtVersion = "9.1.60",
        SsoVersion = 22,
        AppId = 16,
        SubAppId = 537275636,
        PackageName = "com.tencent.mobileqq",
        ApkSignatureMd5 = [0xA6, 0xB7, 0x45, 0xBF, 0x24, 0xA2, 0xC2, 0x77, 0x52, 0x77, 0x16, 0xF6, 0xF3, 0x6E, 0xB6, 0x8D],
        SdkInfo = new WtLoginSdkInfo
        {
            SdkBuildTime = 1740483688,
            SdkVersion = "6.0.0.2568",
            MiscBitMap = 150470524,
            SubSigMap = 66560,
            MainSigMap = Sig.WLOGIN_A5 | Sig.WLOGIN_RESERVED | Sig.WLOGIN_STWEB | Sig.WLOGIN_A2 | Sig.WLOGIN_ST | Sig.WLOGIN_LSKEY | Sig.WLOGIN_SKEY | Sig.WLOGIN_SIG64 | Sig.WLOGIN_VKEY | Sig.WLOGIN_D2 | Sig.WLOGIN_SID | Sig.WLOGIN_PSKEY | Sig.WLOGIN_AQSIG | Sig.WLOGIN_LHSIG | Sig.WLOGIN_PAYTOKEN | (Sig)65536
        },
        AppClientVersion = 0
    };
    
    private static readonly BotAppInfo AndroidPad = new()
    {
        Os = "Android",
        CurrentVersion = "9.1.60.045f5d19",
        PtVersion = "9.1.60",
        AppId = 16,
        SubAppId = 537275675,
        SsoVersion = 22,
        PackageName = "com.tencent.mobileqq",
        ApkSignatureMd5 = [0xA6, 0xB7, 0x45, 0xBF, 0x24, 0xA2, 0xC2, 0x77, 0x52, 0x77, 0x16, 0xF6, 0xF3, 0x6E, 0xB6, 0x8D],
        SdkInfo = new WtLoginSdkInfo
        {
            SdkBuildTime = 1740483688,
            SdkVersion = "6.0.0.2568",
            MiscBitMap = 150470524,
            SubSigMap = 66560,
            MainSigMap = Sig.WLOGIN_A5 | Sig.WLOGIN_RESERVED | Sig.WLOGIN_STWEB | Sig.WLOGIN_A2 | Sig.WLOGIN_ST | Sig.WLOGIN_LSKEY | Sig.WLOGIN_SKEY | Sig.WLOGIN_SIG64 | Sig.WLOGIN_VKEY | Sig.WLOGIN_D2 | Sig.WLOGIN_SID | Sig.WLOGIN_PSKEY | Sig.WLOGIN_AQSIG | Sig.WLOGIN_LHSIG | Sig.WLOGIN_PAYTOKEN | (Sig)65536
        },
        AppClientVersion = 0
    };

    private static readonly BotAppInfo AndroidWatch = new()
    {
        Os = "Android",
        CurrentVersion = "testrevision",
        PtVersion = "9.0.5",
        AppId = 16,
        SubAppId = 537258298,
        SsoVersion = 22,
        PackageName = "com.tencent.qqlite",
        ApkSignatureMd5 = [0xA6, 0xB7, 0x45, 0xBF, 0x24, 0xA2, 0xC2, 0x77, 0x52, 0x77, 0x16, 0xF6, 0xF3, 0x6E, 0xB6, 0x8D],
        SdkInfo = new WtLoginSdkInfo
        {
            SdkBuildTime = 1724730201,
            SdkVersion = "6.0.0.2564",
            MiscBitMap = 150470524,
            SubSigMap = 66560,
            MainSigMap = Sig.WLOGIN_A5 | Sig.WLOGIN_RESERVED | Sig.WLOGIN_STWEB | Sig.WLOGIN_A2 | Sig.WLOGIN_ST | Sig.WLOGIN_LSKEY | Sig.WLOGIN_SKEY | Sig.WLOGIN_SIG64 | Sig.WLOGIN_VKEY | Sig.WLOGIN_D2 | Sig.WLOGIN_SID | Sig.WLOGIN_PSKEY | Sig.WLOGIN_AQSIG | Sig.WLOGIN_LHSIG | Sig.WLOGIN_PAYTOKEN | (Sig)65536
        },
        AppClientVersion = 0
    };
    
    public static readonly Dictionary<Protocols, BotAppInfo> ProtocolToAppInfo = new()
    {
        { Protocols.Windows, Windows },
        { Protocols.Linux, Linux },
        { Protocols.MacOs, MacOs },
        { Protocols.AndroidPhone, AndroidPhone },
        { Protocols.AndroidPad, AndroidPad },
        { Protocols.AndroidWatch, AndroidWatch }
    };
}

public class WtLoginSdkInfo
{
    public uint SdkBuildTime { get; init; }
    
    public string SdkVersion { get; init; }
    
    public uint MiscBitMap { get; init; }
    
    public uint SubSigMap { get; init; }
    
    public Sig MainSigMap { get; init; }
}

[Flags]
public enum Sig
{
    WLOGIN_A5 = 1 << 1,
    WLOGIN_RESERVED = 1 << 4,
    WLOGIN_STWEB = 1 << 5,
    WLOGIN_A2 = 1 << 6,
    WLOGIN_ST = 1 << 7,
    WLOGIN_LSKEY = 1 << 9,
    WLOGIN_SKEY = 1 << 12,
    WLOGIN_SIG64 = 1 << 13,
    WLOGIN_OPENKEY = 1 << 14,
    WLOGIN_TOKEN = 1 << 15,
    WLOGIN_VKEY = 1 << 17,
    WLOGIN_D2 = 1 << 18,
    WLOGIN_SID = 1 << 19,
    WLOGIN_PSKEY = 1 << 20,
    WLOGIN_AQSIG = 1 << 21,
    WLOGIN_LHSIG = 1 << 22,
    WLOGIN_PAYTOKEN = 1 << 23,
    WLOGIN_PF = 1 << 24,
    WLOGIN_DA2 = 1 << 25,
    WLOGIN_QRPUSH = 1 << 26,
    WLOGIN_PT4Token = 1 << 27
}