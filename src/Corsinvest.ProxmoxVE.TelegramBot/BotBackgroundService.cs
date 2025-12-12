/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.TelegramBot.Api;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal sealed class BotBackgroundService(
    ILogger<BotBackgroundService> logger,
    ILoggerFactory loggerFactory,
    BotServiceOptions options,
    IHostApplicationLifetime appLifetime) : BackgroundService
{
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private BotManager? _botManager;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Create bot manager with Proxmox VE client
            _botManager = new BotManager(
                async () => await ClientHelper.GetClientAndTryLoginAsync(
                    options.Host,
                    options.Username,
                    options.Password,
                    options.ApiToken,
                    options.ValidateCertificate,
                    _loggerFactory),
                options.ChatToken,
                [.. options.ChatIds],
                Console.Out);

            // Subscribe to fatal error events
            _botManager.FatalError += OnFatalError;

            logger.LogInformation("Starting Telegram bot in {Mode} mode...",
                options.ServiceMode ? "service" : "console");
            logger.LogInformation("Authorized chats: {ChatIds}", string.Join(", ", options.ChatIds));

            await _botManager.StartReceiving();
            logger.LogInformation("Telegram bot started successfully");

            // Register graceful shutdown
            stoppingToken.Register(() =>
            {
                logger.LogInformation("Shutdown requested, stopping Telegram bot...");
                try
                {
                    _botManager?.StopReceiving();
                    logger.LogInformation("Telegram bot stopped successfully");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while stopping bot");
                }
            });

            // Keep service running until cancellation
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
                logger.LogDebug("Bot service task cancelled");
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Fatal error in bot service");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("BotBackgroundService stopping...");
        await base.StopAsync(cancellationToken);
    }

    private void OnFatalError(object? sender, Exception exception)
    {
        logger.LogCritical(exception, "Fatal error occurred in bot. Application will terminate.");

        // Stop the application gracefully
        appLifetime.StopApplication();
    }
}
