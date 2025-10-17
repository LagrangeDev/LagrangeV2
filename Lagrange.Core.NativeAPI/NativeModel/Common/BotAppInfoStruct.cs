using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Common;

namespace Lagrange.Core.NativeAPI.NativeModel.Common
{
    [StructLayout(LayoutKind.Sequential)]
    struct BotAppInfoStruct
    {
        public BotAppInfoStruct() { }

        public ByteArrayNative Os = new();

        public ByteArrayNative VendorOs = new();

        public ByteArrayNative Kernel = new();

        public ByteArrayNative CurrentVersion = new();

        public ByteArrayNative PtVersion = new();

        public int SsoVersion = 0;

        public ByteArrayNative PackageName = new();

        public ByteArrayNative ApkSignatureMd5 = new();

        public WtLoginSdkInfoStruct SdkInfo = new();

        public int AppId = 0;

        public int SubAppId = 0;

        public ushort AppClientVersion = 0;

        public static implicit operator BotAppInfo(BotAppInfoStruct info)
        {
            return new BotAppInfo()
            {
                Os = Encoding.UTF8.GetString(info.Os.ToByteArrayWithoutFree()),
                VendorOs = Encoding.UTF8.GetString(info.VendorOs.ToByteArrayWithoutFree()),
                Kernel = Encoding.UTF8.GetString(info.Kernel.ToByteArrayWithoutFree()),
                CurrentVersion = Encoding.UTF8.GetString(info.CurrentVersion.ToByteArrayWithoutFree()),
                PtVersion = Encoding.UTF8.GetString(info.PtVersion.ToByteArrayWithoutFree()),
                SsoVersion = info.SsoVersion,
                PackageName = Encoding.UTF8.GetString(info.PackageName.ToByteArrayWithoutFree()),
                ApkSignatureMd5 = info.ApkSignatureMd5.ToByteArrayWithoutFree(),
                SdkInfo = info.SdkInfo,
                AppId = info.AppId,
                SubAppId = info.SubAppId,
                AppClientVersion = info.AppClientVersion
            };
        }

        public static implicit operator BotAppInfoStruct(BotAppInfo info)
        {
            return new BotAppInfoStruct()
            {
                Os = Encoding.UTF8.GetBytes(info.Os),
                VendorOs = Encoding.UTF8.GetBytes(info.VendorOs),
                Kernel = Encoding.UTF8.GetBytes(info.Kernel),
                CurrentVersion = Encoding.UTF8.GetBytes(info.CurrentVersion),
                PtVersion = Encoding.UTF8.GetBytes(info.PtVersion),
                SsoVersion = info.SsoVersion,
                PackageName = Encoding.UTF8.GetBytes(info.PackageName),
                ApkSignatureMd5 = info.ApkSignatureMd5,
                SdkInfo = info.SdkInfo,
                AppId = info.AppId,
                SubAppId = info.SubAppId,
                AppClientVersion = info.AppClientVersion
            };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WtLoginSdkInfoStruct
    {
        public WtLoginSdkInfoStruct() { }

        public uint SdkBuildTime = 0;

        public ByteArrayNative SdkVersion = new();

        public uint MiscBitMap = 0;

        public uint SubSigMap = 0;

        public int MainSigMap = 0;

        public static implicit operator WtLoginSdkInfo(WtLoginSdkInfoStruct info)
        {
            return new WtLoginSdkInfo()
            {
                SdkBuildTime = info.SdkBuildTime,
                SdkVersion = Encoding.UTF8.GetString(info.SdkVersion.ToByteArrayWithoutFree()),
                MiscBitMap = info.MiscBitMap,
                SubSigMap = info.SubSigMap,
                MainSigMap = (Sig)info.MainSigMap,
            };
        }

        public static implicit operator WtLoginSdkInfoStruct(WtLoginSdkInfo info)
        {
            return new WtLoginSdkInfoStruct()
            {
                SdkBuildTime = info.SdkBuildTime,
                SdkVersion = Encoding.UTF8.GetBytes(info.SdkVersion),
                MiscBitMap = info.MiscBitMap,
                SubSigMap = info.SubSigMap,
                MainSigMap = (int)info.MainSigMap,
            };
        }
    }
}