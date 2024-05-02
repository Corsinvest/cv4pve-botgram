/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.TelegramBot.Api;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api;

internal class AliasCommand : Command
{
    private readonly ApiExplorerHelper.AliasDef _aliasDef;
    private Command _commandRef;

    public AliasCommand(ApiExplorerHelper.AliasDef aliasDef)
    {
        _aliasDef = aliasDef;
        Names.AddRange(aliasDef.Names);
    }

    public override string Name => _aliasDef.Names[0];
    public override string Description => _aliasDef.Description;
    public override bool ShowInHelp => false;

    public override async Task<bool> Execute(Message message, BotManager botManager)
    {
        if (_commandRef == null)
        {
            //command reference
            var command = _aliasDef.Command;
            if (!command.StartsWith("/")) { command = "/" + command; }
            var name = command.Trim().Split(' ')[0];
            _commandRef = message.Type == MessageType.Text ?
                            GetCommand(name) :
                            null;

            message.Text = command;
        }

        if (_commandRef == null) { return true; }

        return await _commandRef.Execute(message, botManager);
    }

    public override async Task<bool> Execute(Message message, CallbackQuery callbackQuery, BotManager botManager)
        => await _commandRef.Execute(message, callbackQuery, botManager);
}