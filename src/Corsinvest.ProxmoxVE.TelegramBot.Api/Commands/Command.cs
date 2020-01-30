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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api
{
    /// <summary>
    /// Public command base
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Show in Help
        /// </summary>
        public virtual bool ShowInHelp => true;

        /// <summary>
        /// Name
        /// </summary>
        /// <value></value>
        public abstract string Name { get; }

        /// <summary>
        /// Description
        /// </summary>
        /// <value></value>
        public abstract string Description { get; }

        /// <summary>
        /// Names alias
        /// </summary>
        /// <returns></returns>
        public List<string> Names { get; } = new List<string>();

        internal static IReadOnlyList<Command> GetCommands()
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

        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="message"></param>
        /// <param name="botClient"></param>
        /// <returns></returns>
        public abstract Task<bool> Execute(Message message, TelegramBotClient botClient);

        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="message"></param>
        /// <param name="callbackQuery"></param>
        /// <param name="botClient"></param>
        /// <returns></returns>
        public virtual async Task<bool> Execute(Message message,
                                                CallbackQuery callbackQuery,
                                                TelegramBotClient botClient)
            => await Task.FromResult(true);

        internal static Command GetCommand(string messageText)
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