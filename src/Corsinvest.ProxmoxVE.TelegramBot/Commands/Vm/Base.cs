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