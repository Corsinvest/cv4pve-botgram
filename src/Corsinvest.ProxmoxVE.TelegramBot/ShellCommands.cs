/*
 * This file is part of the cv4pve-botgram https://github.com/Corsinvest/cv4pve-botgram,
 * Copyright (C) 2016 Corsinvest Srl
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
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

                Console.WriteLine($@"Start listening 
Telegram
  User: @{botManager.Username}
Proxmox VE
  Host: {PveHelper.Host}
  Username: {PveHelper.Username}");

                Console.ReadLine();
                botManager.StopReceiving();
                Console.WriteLine("End application");
            });
        }
    }
}