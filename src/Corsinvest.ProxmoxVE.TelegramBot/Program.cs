/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using System.CommandLine;
using Corsinvest.ProxmoxVE.Api.Shell.Helpers;
using Corsinvest.ProxmoxVE.TelegramBot.Api;
using Microsoft.Extensions.Logging;

var app = ConsoleHelper.CreateApp("cv4pve-botgram", "Telegram bot for Proxmox VE");
var loggerFactory = ConsoleHelper.CreateLoggerFactory<Program>(app.GetLogLevelFromDebug());

var optChatToken = app.AddOption<string>("--token", "Telegram API token bot");
optChatToken.IsRequired = true;

var optChatsId = app.AddOption<string>("--chatsId", "Telegram Chats Id valid for communication (comma separated)");

app.SetHandler((host, apiToken, username, validateCertificate, chatToken, chatsId) =>
{
    var chatIds = new List<long>();
    foreach (var chatId in (chatsId + "").Split(","))
    {
        if (long.TryParse(chatId, out var id)) { chatIds.Add(id); }
    }

    var botManager = new BotManager(host,
                                    apiToken,
                                    username,
                                    app.GetPasswordFromOption(),
                                    validateCertificate,
                                    loggerFactory,
                                    chatToken,
                                    [.. chatIds],
                                    Console.Out);
    botManager.StartReceiving();

    Console.ReadLine();

    try { botManager.StopReceiving(); }
    catch { }

    Console.Out.WriteLine("End application");
},
 app.GetHostOption(),
 app.GetApiTokenOption(),
 app.GetUsernameOption(),
 app.GetValidateCertificateOption(),
 optChatToken,
 optChatsId);

return await app.ExecuteAppAsync(args, loggerFactory.CreateLogger(typeof(Program)));
