/*
 * This file is part of the cv4pve-botgram https://github.com/Corsinvest/cv4pve-botgram,
 * Copyright (C) 2016 Corsinvest Srl
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
        private AliasDef _aliasDef;
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