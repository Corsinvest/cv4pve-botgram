/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api;

internal class Start : Help
{
    public override bool ShowInHelp => false;
    public override string Name => "start";
    public override string Description => "Start";
    protected override string GetText(Message message)
        => $@"Welcome {message.Chat.FirstName}
You are connect to Proxmox VE {PveHelperInt.HostAndPortHA}

{base.GetText(message)}

Remember these things:
- Think before typing.
- From great power comes great responsibility.

Good job";
}