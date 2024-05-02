/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.TelegramBot.Api;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api;

internal class Help : Command
{
    public override string Name => "help";
    public override string Description => "Show help";

    protected virtual string GetText(Message message, BotManager botManager)
        => "Usage:" +
           Environment.NewLine +
           string.Join(Environment.NewLine,
                       GetCommands().Where(a => a.ShowInHelp)
                                    .Select(a => $"/{a.Name} - {a.Description}"));

    public override async Task<bool> Execute(Message message, BotManager botManager)
    {
        await botManager.BotClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, GetText(message, botManager));
        return true;
    }
}