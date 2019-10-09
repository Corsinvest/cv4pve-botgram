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

using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Helpers;
using Corsinvest.ProxmoxVE.Api.Extension.Utility;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api
{
    public abstract class Base : Command
    {
        private enum TypeRequest
        {
            Start,
            Resource,
            ArgParameter,
            ArgResource,
        }

        protected abstract MethodType MethodType { get; }
        protected string FileName { get; }
        private string _messageText;
        private TypeRequest _typeRequest = TypeRequest.Start;

        public override async Task<bool> Execute(Message message,
                                                 CallbackQuery callbackQuery,
                                                 TelegramBotClient botClient)
        {
            if (string.IsNullOrWhiteSpace(_messageText))
            {
                await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, "No command!");
                await Task.CompletedTask;
            }

            ReplaceArg(callbackQuery.Data);

            return await ExecuteOrChoose(message, botClient);
        }

        private async Task<bool> ExecuteOrChoose(Message message, TelegramBotClient botClient)
        {
            var endCommand = false;
            var cmdArgs = StringHelper.TokenizeCommandLineToList(_messageText);
            if (cmdArgs.Count == 0)
            {
                //request resource
                _typeRequest = TypeRequest.Resource;
                await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id,
                                                               "Insert <b>resource</b> (eg nodes)");
            }
            else
            {
                var resource = cmdArgs[0];
                if (!resource.StartsWith("/")) { resource = "/" + resource; }
                var requestArgs = StringHelper.GetArgumentTags(resource);
                var parameters = cmdArgs.ToArray()[1..];
                var parametersArgs = parameters.SelectMany(a => StringHelper.GetArgumentTags(a)).ToList();

                if (requestArgs.Count() > 0)
                {
                    //fix request
                    resource = resource.Substring(0, resource.IndexOf(StringHelper.CreateArgumentTag(requestArgs[0])) - 1);
                    var ret = ApiExplorer.ListValues(PveHelper.GetClient(), PveHelper.GetClassApiRoot(), resource);
                    if (!string.IsNullOrWhiteSpace(ret.Error))
                    {
                        //return error
                        await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, ret.Error);
                        endCommand = true;
                    }
                    else
                    {
                        _typeRequest = TypeRequest.ArgResource;

                        await botClient.ChooseInlineKeyboard(message.Chat.Id,
                                                             $"Choose {requestArgs[0]}",
                                                             ret.Values.Select(a => ("", a.Value, a.Value)));
                    }
                }
                else if (parametersArgs.Count() > 0)
                {
                    //request parameter value
                    _typeRequest = TypeRequest.ArgParameter;

                    await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id,
                                                                   $"Insert value for parmater <b>{parametersArgs[0]}</b>");
                }
                else if (requestArgs.Count() == 0)
                {
                    //execute request
                    var ret = ApiExplorer.Execute(PveHelper.GetClient(),
                                                  PveHelper.GetClassApiRoot(),
                                                  resource,
                                                  MethodType,
                                                  ApiExplorer.CreateParameterResource(parameters));

                    if (ret.ResultCode != 200)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Error: {ret.ResultText}");
                    }
                    else
                    {
                        var filename = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resource).Replace("/", "");
                        await botClient.SendDocumentAsyncFromText(message.Chat.Id, ret.ResultText, $"{filename}.txt");
                    }

                    endCommand = true;
                }
            }

            if (endCommand) { _messageText = ""; }

            return await Task.FromResult(endCommand);
        }

        private void ReplaceArg(string value)
            => _messageText = _messageText.Replace(
                                StringHelper.CreateArgumentTag(StringHelper.GetArgumentTags(_messageText)[0]),
                                value);

        public override async Task<bool> Execute(Message message, TelegramBotClient botClient)
        {
            switch (_typeRequest)
            {
                case TypeRequest.Start:
                    _messageText = "";
                    var args = StringHelper.TokenizeCommandLineToList(message.Text);
                    if (args.Count > 1) { _messageText = string.Join(" ", args.ToArray()[1..]); }
                    break;

                case TypeRequest.Resource: _messageText = message.Text; break;
                case TypeRequest.ArgParameter: ReplaceArg(message.Text); break;
                default: break;
            }

            return await ExecuteOrChoose(message, botClient);
        }
    }
}