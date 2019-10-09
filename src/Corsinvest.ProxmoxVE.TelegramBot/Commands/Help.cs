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
using System.Linq;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands
{
    public class Help : Command
    {
        public override string Name => "help";
        public override string Description => "Show help";
        protected virtual string GetText(Message message)
            => "Usage:" +
               Environment.NewLine +
               string.Join(Environment.NewLine,
                           Command.GetCommands()
                                  .Where(a => a.ShowInHelp)
                                  .Select(a => $"/{a.Name} - {a.Description}"));

        public override async Task<bool> Execute(Message message, TelegramBotClient botClient)
        {
            await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, GetText(message));

            return await Task.FromResult(true);
        }
    }
}