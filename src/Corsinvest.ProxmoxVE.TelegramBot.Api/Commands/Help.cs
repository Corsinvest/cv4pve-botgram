/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.TelegramBot.Api.Helpers;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Commands;

internal class Help : Command
{
    public override string Name => "help";
    public override string Description => "Show help";

    protected virtual async Task<string> GetTextAsync(Message message, BotManager botManager)
    {
        await Task.CompletedTask;
        return "Usage:" +
               Environment.NewLine +
               string.Join(Environment.NewLine,
                           GetCommands().Where(a => a.ShowInHelp)
                                        .Select(a => $"/{a.Name} - {a.Description}"));
    }

    public override async Task<bool> Execute(Message message, BotManager botManager)
    {
        await botManager.BotClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, await GetTextAsync(message, botManager));
        return true;
    }
}