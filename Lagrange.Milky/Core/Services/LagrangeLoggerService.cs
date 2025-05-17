using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Lagrange.Core;
using Lagrange.Core.Events.EventArgs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;
using LGRLogLevel = Lagrange.Core.Events.EventArgs.LogLevel;

namespace Lagrange.Milky.Core.Services;

public partial class LagrangeLoggerService(ILoggerFactory loggerFactory, BotContext bot) : IHostedService
{
    private readonly ConcurrentDictionary<string, ILogger> _cache = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        bot.EventInvoker.RegisterEvent<BotLogEvent>(HandleLog);

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

        var logger = _cache.GetOrAdd(@event.Tag, _ => loggerFactory.CreateLogger(InferFullName(@event.Tag)));
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
        bot.EventInvoker.RegisterEvent<BotLogEvent>(HandleLog);

        return Task.CompletedTask;
    }

    private static partial class LoggerHelper
    {
        [LoggerMessage(Message = "{message}", EventId = 0)]
        public static partial void LogBotMessage(ILogger logger, MSLogLevel level, string message);
    }
}