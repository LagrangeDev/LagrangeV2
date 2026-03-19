using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api;
using Lagrange.Milky.Cache;
using Lagrange.Milky.Configuration;
using Lagrange.Milky.Core;
using Lagrange.Milky.Event;
using Lagrange.Milky.Utility;
using Lagrange.Milky.Utility.CaptchaResolver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoreLogLevel = Lagrange.Core.Events.EventArgs.LogLevel;

namespace Lagrange.Milky.Extension;

public static class HostApplicationBuilderExtension
{
    public static HostApplicationBuilder ConfigureConfiguration(this HostApplicationBuilder builder, Action<ConfigurationManager> configurer)
    {
        configurer(builder.Configuration);
        return builder;
    }

    public static HostApplicationBuilder ConfigureServices(this HostApplicationBuilder builder, Action<IServiceCollection> configurer)
    {
        configurer(builder.Services);
        return builder;
    }

    public static HostApplicationBuilder ConfigureCore(this HostApplicationBuilder builder) => builder
#pragma warning disable IL2026, IL3050
        // https://github.com/dotnet/runtime/issues/94544
        .ConfigureServices(services => services
            .Configure<CoreConfiguration>(builder.Configuration.GetSection("Core"))
#pragma warning restore IL2026, IL3050
            // Signer
            .AddSingleton<Signer>()
            // BotConfig
            .AddSingleton(services =>
            {
                var loggerConfiguration = services.GetRequiredService<IOptions<LoggerFilterOptions>>().Value;
                var coreConfiguration = services.GetRequiredService<IOptions<CoreConfiguration>>().Value;
                var signer = services.GetRequiredService<Signer>();

                return new BotConfig
                {
                    Protocol = Protocols.Linux,
                    LogLevel = (CoreLogLevel)loggerConfiguration.GetDefaultLogLevel(),
                    AutoReconnect = coreConfiguration.Server.AutoReconnect,
                    UseIPv6Network = coreConfiguration.Server.UseIPv6Network,
                    GetOptimumServer = coreConfiguration.Server.GetOptimumServer,
                    AutoReLogin = coreConfiguration.Login.AutoReLogin,
                    SignProvider = signer,
                };
            })
            // BotKeystore
            .AddSingleton(services =>
            {
                var configuration = services.GetRequiredService<IOptions<CoreConfiguration>>().Value;

                if (!configuration.Login.Uin.HasValue) throw new Exception("Core.Login.Uin cannot be null");
                string path = $"{configuration.Login.Uin.Value}.keystore";

                BotKeystore keystore;
                if (File.Exists(path))
                {
                    var keystoreNullable = JsonUtility.Deserialize<BotKeystore>(File.ReadAllBytes(path));
                    keystore = keystoreNullable ?? throw new Exception(
                        $"Invalid keystore detected. Please remove the '{path}' file and re-authenticate."
                    );
                }
                else
                {
                    keystore = BotKeystore.CreateEmpty();
                }

                keystore.DeviceName = configuration.Login.DeviceName;
                return keystore;
            })
            // BotContext
            .AddSingleton(services =>
            {
                var config = services.GetRequiredService<BotConfig>();
                var keystore = services.GetRequiredService<BotKeystore>();

                return BotFactory.Create(config, keystore);
            })

            // CaptchaResolver
            .AddSingleton<ICaptchaResolver>(services =>
            {
                var configuration = services.GetRequiredService<IOptions<CoreConfiguration>>().Value;

                return configuration.Login.UseOnlineCaptchaResolver
                    ? ActivatorUtilities.CreateInstance<OnlineCaptchaResolver>(services)
                    : ActivatorUtilities.CreateInstance<ManualCaptchaResolver>(services);
            })

            // Logger
            .AddHostedService<CoreLoggerService>()

            // Login
            .AddHostedService<CoreLoginService>()
        );

    public static HostApplicationBuilder ConfigureMilky(this HostApplicationBuilder builder) => builder
#pragma warning disable IL2026, IL3050
    // https://github.com/dotnet/runtime/issues/94544
    .ConfigureServices(services => services
        .Configure<MilkyConfiguration>(builder.Configuration.GetSection("Milky"))
#pragma warning restore IL2026, IL3050
        // MessageCache
        .AddSingleton<MessageCache>()
        .AddHostedService(ServiceProviderServiceExtensions.GetRequiredService<MessageCache>)
        // ResourceResolver
        .AddSingleton<ResourceResolver>()
        // Converter
        .AddSingleton<EntityConvert>()
    )
    .ConfigureServices(services =>
    {
#pragma warning disable IL2026, IL3050
        // https://github.com/dotnet/runtime/issues/94544
        var configuration = builder.Configuration.GetSection("Milky").Get<MilkyConfiguration>()
            ?? throw new Exception("Milky cannot be null");
#pragma warning restore IL2026, IL3050

        // Api Handlers
        services.AddApiHandlers(configuration.Debug);
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<MilkyHttpApiService>();

#pragma warning disable IL2026, IL3050
        // https://github.com/dotnet/runtime/issues/94544
        var configuration = builder.Configuration.GetSection("Milky").Get<MilkyConfiguration>()
            ?? throw new Exception("Milky cannot be null");
#pragma warning restore IL2026, IL3050

        if (configuration.EnabledWebSocket || configuration.WebHook != null)
        {
            services.AddSingleton<EventService>();
            services.AddHostedService(ServiceProviderServiceExtensions.GetRequiredService<EventService>);
        }

        if (configuration.EnabledWebSocket) services.AddHostedService<MilkyWebSocketEventService>();
        if (configuration.WebHook != null) services.AddHostedService<MilkyWebHookEventService>();
    });
}