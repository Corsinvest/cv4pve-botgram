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
 
namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Top
{
    public class Storage : Base
    {
        public override string Name => "tops";
        public override string Description => "Show storages status";
        protected override string Type => "storage";
    }
}