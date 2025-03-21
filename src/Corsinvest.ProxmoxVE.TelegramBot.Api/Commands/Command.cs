/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Metadata;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Commands;

/// <summary>
/// Public command base
/// </summary>
public abstract class Command
{
    private static ClassApi _classApiRoot;

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
    public List<string> Names { get; } = [];

    /// <summary>
    /// Get Class Api Root
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    protected static async Task<ClassApi> GetClassApiRoot(PveClient client)
        => _classApiRoot ??= await GeneratorClassApi.GenerateAsync(client.Host, client.Port);

    internal static IReadOnlyList<Command> GetCommands()
    {
        var commands = typeof(Command).Assembly
                                      .GetTypes()
                                      .Where(a => !a.IsAbstract &&
                                                  a.IsSubclassOf(typeof(Command)) &&
                                                  !a.IsAssignableFrom(typeof(AliasCommand)))
                                      .Select(a => (Command)Activator.CreateInstance(a))
                                      .ToList();

        commands.AddRange(new Alias().GetCommandsFromAlias());

        return commands.AsReadOnly();
    }

    /// <summary>
    /// Execute
    /// </summary>
    /// <param name="message"></param>
    /// <param name="botManager"></param>
    /// <returns></returns>
    public abstract Task<bool> Execute(Message message, BotManager botManager);

    /// <summary>
    /// Execute
    /// </summary>
    /// <param name="message"></param>
    /// <param name="callbackQuery"></param>
    /// <param name="botManager"></param>
    /// <returns></returns>
    public virtual async Task<bool> Execute(Message message, CallbackQuery callbackQuery, BotManager botManager)
        => await Task.FromResult(true);

    internal static Command GetCommand(string messageText)
    {
        Command command = null;

        if (messageText.Trim().StartsWith("/"))
        {
            var name = messageText.Trim()[1..];
            var pos = name.IndexOf(" ");
            if (pos > 0) { name = name[..pos]; }

            command = GetCommands().FirstOrDefault(a => a.Names.Contains(name) || name == a.Name);
        }

        return command;
    }
}