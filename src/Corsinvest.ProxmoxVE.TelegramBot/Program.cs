/*
 * This file is part of the cv4pve-botgram https://github.com/Corsinvest/cv4pve-botgram,
 *
 * This source file is available under two different licenses:
 * - GNU General Public License version 3 (GPLv3)
 * - Corsinvest Enterprise License (CEL)
 * Full copyright and license information is available in
 * LICENSE.md which is distributed with this source code.
 *
 * Copyright (C) 2016 Corsinvest Srl	GPLv3 and CEL
 */

using System;
using Corsinvest.ProxmoxVE.Api.Shell.Helpers;
using Corsinvest.ProxmoxVE.TelegramBot.Api;
using McMaster.Extensions.CommandLineUtils;

namespace Corsinvest.ProxmoxVE.TelegramBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = ShellHelper.CreateConsoleApp("cv4pve-botgram", "Telegram bot for Proxmox VE");

            var optToken = app.Option("--token", "Telegram API token bot", CommandOptionType.SingleValue)
                              .DependOn(app, CommandOptionExtension.HOST_OPTION_NAME);

            app.OnExecute(() =>
            {
                var botManager = new BotManager(app.GetHost().Value(),
                                                app.GetUsername().Value(),
                                                app.GetPasswordFromOption(),
                                                optToken.Value(),
                                                app.Out);
                botManager.StartReceiving();

                Console.ReadLine();

                try { botManager.StopReceiving(); }
                catch { }

                app.Out.WriteLine("End application");
            });

            app.ExecuteConsoleApp(args);
        }
    }
}
