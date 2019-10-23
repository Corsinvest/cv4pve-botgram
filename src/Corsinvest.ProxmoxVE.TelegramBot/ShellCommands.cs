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
using Corsinvest.ProxmoxVE.Api.Extension.Helpers.Shell;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers;
using McMaster.Extensions.CommandLineUtils;

namespace Corsinvest.ProxmoxVE.TelegramBot
{
    public class ShellCommands
    {
        internal static void Commands(CommandLineApplication app)
        {
            var optToken = app.Option("--token", "Telegram API token bot", CommandOptionType.SingleValue)
                              .IsRequired();

            app.OnExecute(() =>
            {
                (PveHelper.Host, PveHelper.Port, PveHelper.Username, PveHelper.Password) = app.GetOptionsConnection();

                var botManager = new BotManager(optToken.Value());
                botManager.StartReceiving();

                Console.Out.WriteLine($@"Start listening 
Telegram
  User: @{botManager.Username}
Proxmox VE
  Host: {PveHelper.Host}
  Username: {PveHelper.Username}");

                Console.ReadLine();
                botManager.StopReceiving();
                Console.Out.WriteLine("End application");
            });
        }
    }
}