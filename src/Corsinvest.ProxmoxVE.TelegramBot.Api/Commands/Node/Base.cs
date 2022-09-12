/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.TelegramBot.Commands.Api;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Node.Api;

internal abstract class Base : Command
{
    protected abstract bool ForReboot { get; }

    public override async Task<bool> Execute(Message message, TelegramBotClient botClient)
    {
        await botClient.ChooseNodeInlineKeyboard(message.Chat.Id, await GetClient(), true);
        return false;
    }

    public override async Task<bool> Execute(Message message,
                                             CallbackQuery callbackQuery,
                                             TelegramBotClient botClient)
    {
        var action = ForReboot ? "reboot" : "shutdown";
        await (await GetClient()).Nodes[callbackQuery.Data].Status.NodeCmd(action);

        await botClient.SendTextMessageAsyncNoKeyboard(callbackQuery.Message.Chat.Id,
                                                       $"Node {callbackQuery.Data} action {action}!");

        return await Task.FromResult(true);
    }
}