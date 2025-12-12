/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api.Console.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Parse command line arguments using ConsoleHelper
var app = ConsoleHelper.CreateApp("cv4pve-botgram", "Telegram bot for Proxmox VE");

var optChatToken = app.AddOption<string>("--token", "Telegram API token bot");
optChatToken.Required = true;

var optChatsId = app.AddOption<string>("--chatsId", "Telegram Chats Id valid for communication (comma separated)");
var optServiceMode = app.AddOption<bool>("--service-mode", "Run as background service (no console interaction)");

app.SetAction(async (action) =>
{
    // Parse chat IDs
    var chatIds = new List<long>();
    foreach (var chatId in (action.GetValue(optChatsId) + "").Split(",", StringSplitOptions.RemoveEmptyEntries))
    {
        if (long.TryParse(chatId, out var id))
        {
            chatIds.Add(id);
        }
    }

    // Create host builder with dependency injection
    var hostBuilder = Host.CreateDefaultBuilder()
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();

            // Apply same filtering as ConsoleHelper.CreateLoggerFactory
            var logLevel = app.GetLogLevelFromDebug();
            logging.AddFilter("Microsoft", LogLevel.Warning);
            logging.AddFilter("System", LogLevel.Warning);
            logging.AddFilter("Corsinvest.ProxmoxVE.Api.PveClientBase", logLevel);
            logging.SetMinimumLevel(logLevel);
        })
        .ConfigureServices((context, services) =>
        {
            // Register bot configuration options
            services.AddSingleton(new BotServiceOptions
            {
                ChatToken = action.GetValue(optChatToken)!,
                ChatIds = chatIds,
                Host = action.GetValue(app.GetHostOption())!,
                Username = action.GetValue(app.GetUsernameOption())!,
                Password = app.GetPasswordFromOption(),
                ApiToken = action.GetValue(app.GetApiTokenOption()),
                ValidateCertificate = action.GetValue(app.GetValidateCertificateOption()),
                ServiceMode = action.GetValue(optServiceMode)
            });

            // Register the bot background service
            services.AddHostedService<BotBackgroundService>();
        });

    var host = hostBuilder.Build();

    // Run in appropriate mode
    if (action.GetValue(optServiceMode) == false)
    {
        // Console mode - allow manual stop with Enter key
        await host.StartAsync();

        Console.WriteLine("Bot is running. Press Enter to stop...");
        Console.ReadLine();

        Console.WriteLine("Stopping bot...");
        await host.StopAsync(TimeSpan.FromSeconds(10));
    }
    else
    {
        // Service mode - run indefinitely until process is killed
        await host.RunAsync();
    }
});


var loggerFactory = ConsoleHelper.CreateLoggerFactory<Program>(app.GetLogLevelFromDebug());

return await app.ExecuteAppAsync(args, loggerFactory.CreateLogger<Program>());

