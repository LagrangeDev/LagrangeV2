using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Runner.Utility;

namespace Lagrange.Core.Runner;

internal static class Program
{
    private static async Task Main()
    {
        var sign = new InteropSignProvider();
        
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        
        BotContext context;

        if (File.Exists("keystore.json"))
        {
            context = BotFactory.Create(new BotConfig
            {
                Protocol = Protocols.Windows,
                SignProvider = sign,
            }, JsonSerializer.Deserialize<BotKeystore>(await File.ReadAllTextAsync("keystore.json")) ?? throw new InvalidOperationException());
        }
        else
        {
            context = BotFactory.Create(new BotConfig
            {
                Protocol = Protocols.Windows,
                SignProvider = sign
            });
        }
        
        AppDomain.CurrentDomain.ProcessExit += async (_, _) =>
        {
            await context.Logout();
        };
        
        context.EventInvoker.RegisterEvent<BotLogEvent>((_, args) =>
        {
            Console.WriteLine(args);
        });
        
        context.EventInvoker.RegisterEvent<BotQrCodeEvent>((_, args) =>
        {
            Console.WriteLine(args);
            QrCodeHelper.Output(args.Url, false);
        });
        
        context.EventInvoker.RegisterEvent<BotRefreshKeystoreEvent>(async (_, args) =>
        {
            await File.WriteAllTextAsync("keystore.json", JsonSerializer.Serialize(args.Keystore));
        });

        await context.Login();
        await Task.Delay(-1);
    }
}