/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api.Console.Helpers;
using Corsinvest.ProxmoxVE.TelegramBot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var app = ConsoleHelper.CreateApp("Telegram bot for Proxmox VE");

var optChatToken = app.AddOption<string>("--token", "Telegram API token bot");
optChatToken.Required = true;

var optChatsId = app.AddOption<string>("--chatsId", "Telegram Chats Id valid for communication (comma separated)");

// Deprecated: accepted and ignored, so service units written for older versions still start.
var optServiceMode = app.AddOption<bool>("--service-mode", "Deprecated: the bot runs as a service natively");
optServiceMode.Hidden = true;

app.SetAction(async (action, cancellationToken) =>
{
    var chatIds = new List<long>();
    foreach (var chatId in (action.GetValue(optChatsId) + "").Split(",", StringSplitOptions.RemoveEmptyEntries))
    {
        if (long.TryParse(chatId, out var id))
        {
            chatIds.Add(id);
        }
    }

    var options = new BotServiceOptions
    {
        ChatToken = action.GetValue(optChatToken)!,
        ChatIds = chatIds,
        Host = action.GetValue(app.GetHostOption())!,
        Username = action.GetValue(app.GetUsernameOption())!,
        Password = app.GetPasswordFromOption(),
        ApiToken = action.GetValue(app.GetApiTokenOption()),
        ValidateCertificate = action.GetValue(app.GetValidateCertificateOption()),
        DeprecatedServiceMode = action.GetValue(optServiceMode),
    };

    // A plain HostBuilder, not Host.CreateDefaultBuilder: no appsettings.json and no file watcher on the
    // content root, which under systemd is "/" and would be watched recursively.
    var host = new HostBuilder()
                   .UseSystemd()
                   .UseWindowsService()
                   .ConfigureLogging(logging =>
                   {
                       // UseWindowsService adds the Event Log when running as a Windows service.
                       logging.AddConsole();

                       var logLevel = app.GetLogLevelFromDebug();
                       logging.AddFilter("Microsoft", LogLevel.Warning);
                       logging.AddFilter("System", LogLevel.Warning);
                       logging.AddFilter("Corsinvest.ProxmoxVE.Api.PveClientBase", logLevel);
                       logging.SetMinimumLevel(logLevel);
                   })
                   .ConfigureServices((_, services) =>
                   {
                       services.AddSingleton(options);
                       services.AddHostedService<BotBackgroundService>();
                   })
                   .Build();

    // Ctrl+C and SIGTERM cancel the token: the host stops cleanly and the process exits with 0.
    await host.RunAsync(cancellationToken);
    return options.ExitCode;
});

var loggerFactory = ConsoleHelper.CreateLoggerFactory<Program>(app.GetLogLevelFromDebug());

return await app.ExecuteAppAsync(args, loggerFactory.CreateLogger<Program>());
