/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.TelegramBot.Api;
using Corsinvest.ProxmoxVE.TelegramBot.Commands.Api;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Vm.Api;

internal abstract class Base : Command
{
    protected abstract VmStatus StatusToChange { get; }
    protected abstract bool StatusVmIsRunning { get; }

    public override async Task<bool> Execute(Message message, BotManager botManager)
    {
        await botManager.BotClient.ChooseVmInlineKeyboard(message.Chat.Id,
                                                          await botManager.GetPveClientAsync(),
                                                          StatusVmIsRunning);
        return false;
    }

    public override async Task<bool> Execute(Message message, CallbackQuery callbackQuery, BotManager botManager)
    {
        var client = await botManager.GetPveClientAsync();
        var vm = await client.GetVmAsync(callbackQuery.Data);
        await VmHelper.ChangeStatusVmAsync(client, vm.Node, vm.VmType, vm.VmId, StatusToChange);

        await botManager.BotClient.SendTextMessageAsyncNoKeyboard(callbackQuery.Message.Chat.Id,
                                                                  $"VM/CT {vm.Id} on node {vm.Node} {StatusToChange}!");

        return true;
    }
}