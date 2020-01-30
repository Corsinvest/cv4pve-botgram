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
using System.Linq;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api
{
    internal class Help : Command
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