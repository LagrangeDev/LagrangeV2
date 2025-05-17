using System.Reflection;
using System.Text;
using Lagrange.Milky.Core.Extension;
using Lagrange.Milky.Extension;
using Lagrange.Milky.Implementation.Extension;
using Microsoft.Extensions.Hosting;

namespace Lagrange.Milky;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        ShowBanner();
        Console.WriteLine();
        ShowVersion();

        if (!File.Exists(Constants.ConfigFileName))
        {
            Console.WriteLine("Config file not found, creating a new one...");

            CreateAppSettingsFile();

            Console.WriteLine($"Please edit {Constants.ConfigFileName} and press any key to continue.");
            Console.ReadKey(true);
        }

        var host = Host.CreateApplicationBuilder(args)
            .ConfigureConfiguration(Constants.ConfigFileName)
            .AddCore()
            .AddCoreLoginService() // Finally start the login
            .AddMilky()
            .Build();

        await host.RunAsync();
    }

    private static void ShowBanner()
    {
        Console.WriteLine(Constants.Banner);
    }

    private static void ShowVersion()
    {
        string? version = Assembly
            .GetAssembly(typeof(Program))
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        Console.WriteLine($"Version: {version}");
    }

    public static void CreateAppSettingsFile()
    {
        using var istr = typeof(Program).Assembly.GetManifestResourceStream(Constants.ConfigResourceName)!;
        using var temp = File.Create(Constants.ConfigFileName);
        istr.CopyTo(temp);
    }
}