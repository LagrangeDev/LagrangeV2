using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Lagrange.Core;
using Lagrange.Core.Events.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;
using LGRLogLevel = Lagrange.Core.Events.EventArgs.LogLevel;

namespace Lagrange.Milky.Core.Services;

public partial class BotLoggerService : IHostedService
{
    private readonly ConcurrentDictionary<string, ILogger> _cache = new();
    private readonly ILoggerFactory _loggerFactory;
    private readonly BotContext _bot;

    public BotLoggerService(ILoggerFactory loggerFactory, ILogger<BotLoggerService> logger, BotContext bot, IConfiguration config)
    {
        _loggerFactory = loggerFactory;
        _bot = bot;
        
        ChangeToken.OnChange(config.GetReloadToken, () =>
        {
            var currentMinLevel = GetCurrentMinimumLevel(logger);
            _bot.Config.LogLevel = (LGRLogLevel)currentMinLevel;
            logger.LogInformation("BotLoggerService minimum log level reloaded = {MinLevel}", currentMinLevel);
        });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _bot.EventInvoker.RegisterEvent<BotLogEvent>(HandleLog);

        return Task.CompletedTask;
    }

    private void HandleLog(BotContext _, BotLogEvent @event)
    {
        var level = @event.Level switch
        {
            LGRLogLevel.Critical => MSLogLevel.Critical,
            LGRLogLevel.Error => MSLogLevel.Error,
            LGRLogLevel.Warning => MSLogLevel.Warning,
            LGRLogLevel.Information => MSLogLevel.Information,
            LGRLogLevel.Debug => MSLogLevel.Debug,
            LGRLogLevel.Trace => MSLogLevel.Trace,
            _ => throw new NotSupportedException()
        };

        var logger = _cache.GetOrAdd(@event.Tag, _ => _loggerFactory.CreateLogger(InferFullName(@event.Tag)));
        LoggerHelper.LogBotMessage(logger, level, @event.Message);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "It does not matter")]
    private static string InferFullName(string tag)
    {
        foreach (var type in typeof(BotContext).Assembly.GetTypes())
        {
            if (type.Name == tag) return type.FullName ?? type.Name;
        }

        return tag;
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _bot.EventInvoker.RegisterEvent<BotLogEvent>(HandleLog);

        return Task.CompletedTask;
    }

    private static partial class LoggerHelper
    {
        [LoggerMessage(Message = "{message}", EventId = 0)]
        public static partial void LogBotMessage(ILogger logger, MSLogLevel level, string message);
    }
    
    private static MSLogLevel GetCurrentMinimumLevel(ILogger logger)
    {
        if (logger.IsEnabled(MSLogLevel.Trace)) return MSLogLevel.Trace;
        if (logger.IsEnabled(MSLogLevel.Debug)) return MSLogLevel.Debug;
        if (logger.IsEnabled(MSLogLevel.Information)) return MSLogLevel.Information;
        if (logger.IsEnabled(MSLogLevel.Warning)) return MSLogLevel.Warning;
        if (logger.IsEnabled(MSLogLevel.Error)) return MSLogLevel.Error;
        if (logger.IsEnabled(MSLogLevel.Critical)) return MSLogLevel.Critical;
        return MSLogLevel.None;
    }
}