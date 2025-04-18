﻿using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;
using Lagrange.OneBot.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lagrange.OneBot.Core;

public partial class BotService(ILogger<BotService> logger, ILogger<BotContext> botLogger, BotContext context, IConfiguration config) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        context.EventInvoker.RegisterEvent<BotLogEvent>((_, @event) =>
        {
            var level = @event.Level switch
            {
                Lagrange.Core.Events.EventArgs.LogLevel.Critical => LogLevel.Critical,
                Lagrange.Core.Events.EventArgs.LogLevel.Error => LogLevel.Error,
                Lagrange.Core.Events.EventArgs.LogLevel.Warning => LogLevel.Warning,
                Lagrange.Core.Events.EventArgs.LogLevel.Information => LogLevel.Information,
                Lagrange.Core.Events.EventArgs.LogLevel.Debug => LogLevel.Debug,
                Lagrange.Core.Events.EventArgs.LogLevel.Trace => LogLevel.Trace,
                _ => throw new ArgumentOutOfRangeException()
            };
            botLogger.Log(level, @event.Message);
        });
        
        context.EventInvoker.RegisterEvent<BotQrCodeEvent>(async (_, @event) =>
        {
            await File.WriteAllBytesAsync("qrcode.png", @event.Image, cancellationToken);
            bool compatibilityMode = config.GetValue<bool>("QrCode:ConsoleCompatibilityMode");
            QrCodeHelper.Output(@event.Url, compatibilityMode);
            Log.QrCodeSuccess(logger, 120, @event.Url);
        });
        
        context.EventInvoker.RegisterEvent<BotQrCodeQueryEvent>((_, @event) =>
        {
            var level = @event.State switch
            {
                BotQrCodeQueryEvent.TransEmpState.Confirmed or BotQrCodeQueryEvent.TransEmpState.WaitingForConfirm => LogLevel.Information,
                BotQrCodeQueryEvent.TransEmpState.Canceled or BotQrCodeQueryEvent.TransEmpState.CodeExpired => LogLevel.Error,
                _ => LogLevel.Debug
            };
            Log.QrCodeState(logger, level, @event.State);
        });
        
        bool result = await context.Login(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, EventId = 0, Message = "Fetch QrCode Success, Expiration: {expiration} seconds, Url: {url}")]
        public static partial void QrCodeSuccess(ILogger logger, int expiration, string url);
        
        [LoggerMessage(EventId = 1, Message = "QrCode State: {state}")]
        public static partial void QrCodeState(ILogger logger, LogLevel level, BotQrCodeQueryEvent.TransEmpState state);
    }
}