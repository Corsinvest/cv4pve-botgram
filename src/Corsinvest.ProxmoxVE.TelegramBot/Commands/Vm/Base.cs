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

using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api.Extension.VM;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Vm
{
    public abstract class Base : Command
    {
        protected abstract StatusEnum StatusRequestVM { get; }

        public override async Task<bool> Execute(Message message, TelegramBotClient botClient)
        {
            await botClient.ChooseVmInlineKeyboard(message.Chat.Id,
                                                   PveHelper.GetClient(),
                                                   (StatusRequestVM == StatusEnum.Stop));

            return await Task.FromResult(false);
        }

        public override async Task<bool> Execute(Message message,
                                                 CallbackQuery callbackQuery,
                                                 TelegramBotClient botClient)
        {
            var vm = PveHelper.GetClient().GetVM(callbackQuery.Data);
            vm.SetStatus(StatusRequestVM, false);

            await botClient.SendTextMessageAsyncNoKeyboard(callbackQuery.Message.Chat.Id,
                                                           $"VM/CT {vm.Id} on node {vm.Node} {StatusRequestVM}!");

            return await Task.FromResult(true);
        }
    }
}