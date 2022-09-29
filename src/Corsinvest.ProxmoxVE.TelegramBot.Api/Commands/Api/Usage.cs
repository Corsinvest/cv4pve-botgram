/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api.Api;

internal class Usage : Command
{
    private enum TypeRequest
    {
        Start,
        Resource,
    }

    private TypeRequest _typeRequest;
    public override string Name => "usage";
    public override string Description => "Usage resource api";

    public override async Task<bool> Execute(Message message, TelegramBotClient botClient)
    {
        var endCommand = false;

        switch (_typeRequest)
        {
            case TypeRequest.Start:
                _typeRequest = TypeRequest.Resource;
                await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, Base.DEFAULT_MSG);
                break;

            case TypeRequest.Resource:
                var ret = ApiExplorerHelper.Usage(await GetClassApiRoot(await GetClient()),
                                                  message.Text.Trim(),
                                                  TableGenerator.Output.Html,
                                                  true,
                                                  null,
                                                  true);

                await botClient.SendDocumentAsyncFromText(message.Chat.Id, ret, "Usage.html");
                endCommand = true;
                break;

            default: break;
        }

        return await Task.FromResult(endCommand);
    }
}