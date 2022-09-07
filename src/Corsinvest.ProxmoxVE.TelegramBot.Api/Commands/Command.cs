/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api;

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
    /// Get client
    /// </summary>
    /// <returns></returns>
    protected static async Task<PveClient> GetClient() => await PveHelperInt.GetClient();

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
        PveHelperInt.Out.WriteLine("Message: " + messageText);

        if (messageText.Trim().StartsWith("/"))
        {
            var name = messageText.Trim()[1..];
            command = GetCommands().FirstOrDefault(a => a.Names.Contains(name) || name == a.Name);
        }

        return command;
    }
}