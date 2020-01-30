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

using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api
{
    internal class Start : Help
    {
        public override bool ShowInHelp => false;
        public override string Name => "start";
        public override string Description => "Start";
        protected override string GetText(Message message)
            => $@"Corsinvest for Proxmox VE with Telegram Bot
Welcome {message.Chat.FirstName}
You are connect to Proxmox VE {PveHelper.HostandPortHA}

{base.GetText(message)}

Remember these things:
- Think before typing.
- From great power comes great responsibility.

Good job";
    }
}