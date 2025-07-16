using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.NativeAPI.Test.NativeModel;

namespace Lagrange.Core.NativeAPI.Test;

class Program
{
    static int _index = 0;

    static TaskCompletionSource<bool> _tcs = new();

    static async Task Main(string[] args)
    {
        var botConfig = new BotConfigStruct { };
        IntPtr keystorePtr = IntPtr.Zero;
        IntPtr botConfigPtr = Marshal.AllocHGlobal(Marshal.SizeOf(botConfig));
        Marshal.StructureToPtr(botConfig, botConfigPtr, false);
        _index = Wrapper.Initialize(botConfigPtr, keystorePtr);
        Console.WriteLine($"Bot initialized with index: {_index}");
        int status = Wrapper.Start(_index);
        Console.WriteLine($"Bot started with status: {status}");
        Marshal.FreeHGlobal(botConfigPtr);
        
        var timer = new System.Timers.Timer(100);
        timer.Elapsed += PollingProcesser;
        timer.Start();
        await _tcs.Task;
    }

    static async void PollingProcesser(Object? source, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            //await GetEventCount();
            await GetQrCodeEvent();
            await GetLogEvent();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    static async Task GetEventCount()
    {
        await Task.Run(() =>
        {
            IntPtr ptr = Wrapper.GetEventCount(_index);
            if (ptr == IntPtr.Zero)
            {
                return;
            }
            
            var eventCount = Marshal.PtrToStructure<ReverseEventCountStruct>(ptr);
            Console.WriteLine($"BotCaptchaEventCount: {eventCount.BotCaptchaEventCount}");
            Console.WriteLine($"BotLoginEventCount: {eventCount.BotLoginEventCount}");
            Console.WriteLine($"BotLogEventCount: {eventCount.BotLogEventCount}");
            Console.WriteLine($"BotMessageEventCount: {eventCount.BotMessageEventCount}");
            Console.WriteLine($"BotNewDeviceVerifyEventCount: {eventCount.BotNewDeviceVerifyEventCount}");
            Console.WriteLine($"BotOnlineEventCount: {eventCount.BotOnlineEventCount}");
            Console.WriteLine($"BotQrCodeEventCount: {eventCount.BotQrCodeEventCount}");
            Console.WriteLine($"BotQrCodeQueryEventCount: {eventCount.BotQrCodeQueryEventCount}");
            Console.WriteLine($"BotRefreshKeystoreEventCount: {eventCount.BotRefreshKeystoreEventCount}");
            Console.WriteLine($"BotSMSEventCount: {eventCount.BotSMSEventCount}");
            Wrapper.FreeMemory(ptr);
        });
    }

    static async Task GetLogEvent()
    {
        await Task.Run(() =>
        {
            IntPtr ptr = Wrapper.GetBotLogEvent(_index);
            if (ptr == IntPtr.Zero)
            {
                return;
            }

            var logEventArray = Marshal.PtrToStructure<EventArrayStruct>(ptr);

            for (int i = 0; i < logEventArray.Count; i++)
            {
                // 计算当前结构体的指针位置
                IntPtr currentStructPtr = logEventArray.Events + i * Marshal.SizeOf<BotLogEventStruct>();

                // 将指针转换为结构体
                var logEvent = Marshal.PtrToStructure<BotLogEventStruct>(currentStructPtr);

                // 处理日志事件
                Console.WriteLine($"Log: {Encoding.UTF8.GetString(logEvent.Message.ToByteArrayWithoutFree())}");
            }

            Wrapper.FreeMemory(ptr);
        });
    }
    
    static async Task GetQrCodeEvent()
    {
        await Task.Run(() =>
        {
            IntPtr ptr = Wrapper.GetQrCodeEvent(_index);
            if (ptr == IntPtr.Zero)
            {
                return;
            }

            var qrCodeEventArray = Marshal.PtrToStructure<EventArrayStruct>(ptr);
            
            for (int i = 0; i < qrCodeEventArray.Count; i++)
            {
                IntPtr currentStructPtr = qrCodeEventArray.Events + i * Marshal.SizeOf<BotQrCodeEventStruct>();
                
                var qrCodeEvent = Marshal.PtrToStructure<BotQrCodeEventStruct>(currentStructPtr);
                
                Console.WriteLine($"QrCodeUrl: {Encoding.UTF8.GetString(qrCodeEvent.Url.ToByteArrayWithoutFree())}");
            }
        
            Wrapper.FreeMemory(ptr);
        });
    }
}
