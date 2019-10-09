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