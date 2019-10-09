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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands
{
    public abstract class Command
    {
        public virtual bool ShowInHelp => true;
        public abstract string Name { get; }
        public abstract string Description { get; }
        public List<string> Names { get; } = new List<string>();

        public static IReadOnlyList<Command> GetCommands()
        {
            var commands = new List<Command>();
            commands.AddRange(typeof(Command).Assembly
                                    .GetTypes()
                                    .Where(a => !a.IsAbstract &&
                                                a.IsSubclassOf(typeof(Command)) &&
                                                !a.IsAssignableFrom(typeof(AliasCommand)))
                                    .Select(a => (Command)Activator.CreateInstance(a))
                                    .ToList());

            commands.AddRange(new Alias().GetCommandsFromAlias());
            
            return commands.AsReadOnly();
        }

        public abstract Task<bool> Execute(Message message, TelegramBotClient botClient);

        public virtual async Task<bool> Execute(Message message,
                                                CallbackQuery callbackQuery,
                                                TelegramBotClient botClient)
            => await Task.FromResult(true);

        public static Command GetCommand(string messageText)
        {
            Command command = null;
            Console.Out.WriteLine("Message: " + messageText);

            if (messageText.Trim().StartsWith("/"))
            {
                var name = messageText.Trim().Substring(1);
                command = Command.GetCommands()
                                 .Where(a => a.Names.Contains(name) || name == a.Name)
                                 .FirstOrDefault();
            }

            return command;
        }
    }
}