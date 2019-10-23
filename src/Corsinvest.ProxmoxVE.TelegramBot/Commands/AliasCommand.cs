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

using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api.Extension.Shell;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands
{
    public class AliasCommand : Command
    {
        private readonly AliasDef _aliasDef;
        private Command _commandRef;

        public AliasCommand(AliasDef aliasDef)
        {
            _aliasDef = aliasDef;
            Names.AddRange(aliasDef.Names);
        }

        public override string Name => _aliasDef.Names[0];
        public override string Description => _aliasDef.Description;
        public override bool ShowInHelp => false;

        public override async Task<bool> Execute(Message message, TelegramBotClient botClient)
        {
            if (_commandRef == null)
            {
                //command reference
                var command = _aliasDef.Command;
                if (!command.StartsWith("/")) { command = "/" + command; }
                var name = command.Trim().Split(' ')[0];
                _commandRef = message.Type == MessageType.Text ?
                                Command.GetCommand(name) :
                                null;

                message.Text = command;
            }

            if (_commandRef == null) { return await Task.FromResult(true); }

            return await _commandRef.Execute(message, botClient);
        }

        public override async Task<bool> Execute(Message message,
                                                 CallbackQuery callbackQuery,
                                                 TelegramBotClient botClient)
            => await _commandRef.Execute(message, callbackQuery, botClient);
    }
}