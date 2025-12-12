/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.TelegramBot.Api.Helpers;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Commands.Node;

internal abstract class Base : Command
{
    protected abstract bool ForReboot { get; }

    public override async Task<bool> Execute(Message message, BotManager botManager)
    {
        await botManager.BotClient.ChooseNodeInlineKeyboard(message.Chat.Id, await botManager.GetPveClientAsync(), true);
        return false;
    }

    public override async Task<bool> Execute(Message message, CallbackQuery callbackQuery, BotManager botManager)
    {
        if (string.IsNullOrWhiteSpace(callbackQuery.Data)) { return true; }

        var action = ForReboot ? "reboot" : "shutdown";
        await (await botManager.GetPveClientAsync()).Nodes[callbackQuery.Data].Status.NodeCmd(action);

        if (callbackQuery.Message != null)
        {
            await botManager.BotClient.SendTextMessageAsyncNoKeyboard(callbackQuery.Message.Chat.Id,
                                                                      $"Node {callbackQuery.Data} action {action}!");
        }

        return true;
    }
}