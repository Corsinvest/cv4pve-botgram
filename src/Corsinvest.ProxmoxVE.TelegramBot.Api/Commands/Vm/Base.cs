/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.TelegramBot.Commands.Api;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Vm.Api;

internal abstract class Base : Command
{
    protected abstract VmStatus StatusToChange { get; }
    protected abstract bool StatusVmIsRunning { get; }

    public override async Task<bool> Execute(Message message, TelegramBotClient botClient)
    {
        await botClient.ChooseVmInlineKeyboard(message.Chat.Id,
                                               await GetClient(),
                                               StatusVmIsRunning);
        return false;
    }

    public override async Task<bool> Execute(Message message,
                                             CallbackQuery callbackQuery,
                                             TelegramBotClient botClient)
    {
        var client = await GetClient();
        var vm = await client.GetVm(callbackQuery.Data);
        await VmHelper.ChangeStatusVm(client, vm.Node, vm.VmType, vm.VmId, StatusToChange);

        await botClient.SendTextMessageAsyncNoKeyboard(callbackQuery.Message.Chat.Id,
                                                       $"VM/CT {vm.Id} on node {vm.Node} {StatusToChange}!");

        return await Task.FromResult(true);
    }
}