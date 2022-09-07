/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api.Shell.Helpers;
using Corsinvest.ProxmoxVE.TelegramBot.Api;

namespace Corsinvest.ProxmoxVE.TelegramBot;

class Program
{
    static async Task Main(string[] args)
    {
        var app = ConsoleHelper.CreateApp("cv4pve-botgram", "Telegram bot for Proxmox VE");

        var optToken = app.AddOption("--token", "Telegram API token bot");
        optToken.IsRequired = true;

        var optChatsId = app.AddOption("--chatsId", "Telegram Chats Id valid for communication (comma separated)");

        app.SetHandler(() =>
        {
            var chatsId = new List<long>();
            foreach (var chatId in (optChatsId.GetValue() + "").Split(","))
            {
                if (long.TryParse(chatId, out var id)) { chatsId.Add(id); }
            }

            var botManager = new BotManager(app.GetHost().GetValue(),
                                            app.GetApiToken().GetValue(),
                                            app.GetUsername().GetValue(),
                                            app.GetPasswordFromOption(),
                                            optToken.GetValue(),
                                            chatsId.ToArray(),
                                            Console.Out);
            botManager.StartReceiving();

            Console.ReadLine();

            try { botManager.StopReceiving(); }
            catch { }

            Console.Out.WriteLine("End application");
        });

        await app.ExecuteApp(args);
    }
}
