using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.NativeAPI.NativeModel.Common;

namespace Lagrange.Core.NativeAPI;

public static class Program
{
    public static List<Context> Contexts { get; set; } = [];

    [UnmanagedCallersOnly(EntryPoint = "Initialize")]
    public static int Initialize(IntPtr botConfigPtr, IntPtr keystorePtr, IntPtr appInfoPtr)
    {
        var botConfigStruct = Marshal.PtrToStructure<BotConfigStruct>(botConfigPtr);
        var botConfig = botConfigStruct;

        BotAppInfo? appInfo = null;
        if (appInfoPtr != IntPtr.Zero)
        {
            var appInfoStruct = Marshal.PtrToStructure<BotAppInfoStruct>(appInfoPtr);
            appInfo = appInfoStruct;
        }

        int index = Contexts.Count;
        if (keystorePtr != IntPtr.Zero)
        {
            var keystoreStruct = Marshal.PtrToStructure<BotKeystoreStruct>(keystorePtr);
            var keystore = keystoreStruct.ToKeystoreWithoutFree();
            Contexts.Add(new Context(BotFactory.Create(botConfig, keystore, appInfo)));
        }
        else
        {
            Contexts.Add(new Context(BotFactory.Create(botConfig, appInfo)));
        }

        return index;
    }

    [UnmanagedCallersOnly(EntryPoint = "Start")]
    public static StatusCode Start(int index)
    {
        if (Contexts.Count <= index)
        {
            return StatusCode.InvalidIndex;
        }

        if (Contexts[index].BotContext.IsOnline)
        {
            return StatusCode.AlreadyStarted;
        }

        Task.Run(async () =>
        {
            await Contexts[index].BotContext.Login();
            await Task.Delay(Timeout.Infinite);
        });

        return StatusCode.Success;
    }

    [UnmanagedCallersOnly(EntryPoint = "StartWithAndroidSigner")]
    public static StatusCode StartWithAndroidSigner(int index, long uin, ByteArrayNative password)
    {
        if (Contexts.Count <= index)
        {
            return StatusCode.InvalidIndex;
        }

        if (Contexts[index].BotContext.IsOnline)
        {
            return StatusCode.AlreadyStarted;
        }

        Task.Run(async () =>
        {
            await Contexts[index].BotContext.Login(uin, Encoding.UTF8.GetString(password.ToByteArrayWithoutFree()));
            await Task.Delay(Timeout.Infinite);
        });

        return StatusCode.Success;
    }

    [UnmanagedCallersOnly(EntryPoint = "Stop")]
    public static StatusCode Stop(int index)
    {
        if (Contexts.Count <= index)
        {
            return StatusCode.InvalidIndex;
        }

        Contexts[index].BotContext.Dispose();
        Contexts.RemoveAt(index);
        return StatusCode.Success;
    }

    [UnmanagedCallersOnly(EntryPoint = "FreeMemory")]
    public static void FreeMemory(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
