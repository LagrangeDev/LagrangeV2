using System.Text.Json;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Core.Configuration;
using Lagrange.Milky.Core.Services;
using Lagrange.Milky.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Core.Extension;

public static class HostApplicationBuilderExtension
{
    public static HostApplicationBuilder AddCore(this HostApplicationBuilder builder)
    {
        builder.Services.Configure<LagrangeConfiguration>(builder.Configuration.GetSection("Lagrange"));
        builder.Services.AddSingleton(services =>
        {
            var config = services.GetRequiredService<IOptions<LagrangeConfiguration>>().Value;
            var logger = services.GetRequiredService<ILogger<UrlSignProvider>>();

            return new UrlSignProvider(
                logger,
                services,
                config.Protocol.Protocol ?? throw new Exception("Protocol cannot be null"),
                config.Protocol.Signer.Url ?? throw new Exception("Signer url cannot be null"),
                config.Protocol.Signer.ProxyUrl
            );
        });
        builder.Services.AddSingleton(services =>
        {
            var config = services.GetRequiredService<IOptions<LagrangeConfiguration>>().Value;
            var signer = services.GetRequiredService<UrlSignProvider>();

            return new BotConfig
            {
                Protocol = config.Protocol.Protocol ?? throw new Exception("Protocol cannot be null"),
                AutoReconnect = config.Login.AutoReconnect,
                UseIPv6Network = config.Protocol.UseIPv6Network,
                GetOptimumServer = config.Protocol.GetOptimumServer,
                AutoReLogin = config.Login.AutoReLogin,
                SignProvider = signer,
            };
        });
        builder.Services.AddSingleton(services =>
        {
            var config = services.GetRequiredService<IOptions<LagrangeConfiguration>>().Value;

            string path = $"{config.Login.Uin}.keystore";

            if (File.Exists(path))
            {
                if (JsonSerializer.Deserialize(
                        File.ReadAllText(path),
                        typeof(BotKeystore),
                        CoreJsonContext.Default) is not BotKeystore keystore)
                {
                    throw new Exception($"Keystore is null, please delete the '{config.Login.Uin}.keystore' file and try again");
                }
                return keystore;
            }
            else
            {
                var keystore = BotKeystore.CreateEmpty();
                keystore.DeviceName = config.Protocol.DeviceIdentifier;
                return keystore;
            }
        });
        builder.Services.AddSingleton(services =>
        {
            BotConfig config = services.GetRequiredService<BotConfig>();
            BotKeystore keystore = services.GetRequiredService<BotKeystore>();

            return BotFactory.Create(config, keystore);
        });

        builder.Services.AddSingleton<ICaptchaResolver, OnlineCaptchaResolver>();

        builder.Services.AddHostedService<BotLoggerService>();

        return builder;
    }

    public static HostApplicationBuilder AddCoreLoginService(this HostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<BotLoginService>();

        return builder;
    }
}