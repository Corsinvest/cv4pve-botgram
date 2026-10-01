/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.TelegramBot.Api;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;

namespace Corsinvest.ProxmoxVE.TelegramBot;

/// <summary>
/// Starts the bot in <see cref="StartAsync"/>, before the host reports it has started: with systemd
/// <c>Type=notify</c>, <c>systemctl start</c> returns only when the bot is logged in to Proxmox VE and to
/// Telegram.
/// </summary>
internal sealed class BotBackgroundService(ILogger<BotBackgroundService> logger,
                                           ILoggerFactory loggerFactory,
                                           BotServiceOptions options,
                                           IHostApplicationLifetime appLifetime) : IHostedService
{
    private BotManager? _botManager;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (options.DeprecatedServiceMode)
        {
            logger.LogWarning("--service-mode is deprecated and ignored: the bot runs as a service natively. Remove it, it will be dropped in the next major version.");
        }

        try
        {
            // A Windows service has no console: what the bot writes goes to the log, so to the Event Log.
            TextWriter output = WindowsServiceHelpers.IsWindowsService()
                                    ? new LoggerTextWriter(logger)
                                    : Console.Out;

            _botManager = new BotManager(() => ClientHelper.GetClientAndTryLoginAsync(options.Host,
                                                                                      options.Username,
                                                                                      options.Password,
                                                                                      options.ApiToken,
                                                                                      options.ValidateCertificate,
                                                                                      loggerFactory),
                                         options.ChatToken,
                                         [.. options.ChatIds],
                                         output);

            _botManager.FatalError += OnFatalError;

            logger.LogInformation("Authorized chats: {ChatIds}", string.Join(", ", options.ChatIds));

            await _botManager.StartReceiving();

            output.WriteLine("Press Ctrl+C to stop.");
            logger.LogInformation("Telegram bot started");
        }
        catch (Exception ex)
        {
            // E.g. the cluster cannot be reached or Telegram refuses the token: exit with an error, so a
            // service manager set to restart on failure tries again.
            logger.LogCritical(ex, "Cannot start the Telegram bot: {Message}", ex.Message);
            options.ExitCode = 1;
            appLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Telegram bot stopping");
        _botManager?.StopReceiving();
        return Task.CompletedTask;
    }

    private void OnFatalError(object? sender, Exception exception)
    {
        logger.LogCritical(exception, "Fatal error in the Telegram bot, the application stops");
        options.ExitCode = 1;
        appLifetime.StopApplication();
    }
}
