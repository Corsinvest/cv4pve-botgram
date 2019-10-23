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
using Corsinvest.ProxmoxVE.Api.Extension.Utility;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api
{
    public class Usage : Command
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
                    await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id,
                                                                   "Insert <b>resource</b> (eg nodes)");
                    break;

                case TypeRequest.Resource:
                    var ret = ApiExplorer.Usage(PveHelper.GetClassApiRoot(),
                                                message.Text.Trim(),
                                                true,
                                                null,
                                                true);

                    await botClient.SendDocumentAsyncFromText(message.Chat.Id, ret, "Usage.txt");
                    endCommand = true;
                    break;

                default: break;
            }

            return await Task.FromResult(endCommand);
        }
    }
}