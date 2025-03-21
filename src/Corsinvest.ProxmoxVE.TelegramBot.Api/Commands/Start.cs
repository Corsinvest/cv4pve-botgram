/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Commands;

internal class Start : Help
{
    public override bool ShowInHelp => false;
    public override string Name => "start";
    public override string Description => "Start";

    protected override string GetText(Message message, BotManager botManager)
        => $@"Welcome {message.Chat.FirstName}
You are connect to Proxmox VE {botManager.PveHostAndPortHA}

{base.GetText(message, botManager)}

Remember these things:
- Think before typing.
- From great power comes great responsibility.

Good job";
}