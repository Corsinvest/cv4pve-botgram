﻿/*
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

using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Helpers;
using Corsinvest.ProxmoxVE.Api.Extension.Utility;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api.Api
{
    internal abstract class Base : Command
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
                var parameters = cmdArgs.Skip(1).ToArray();
                var parametersArgs = parameters.SelectMany(a => StringHelper.GetArgumentTags(a)).ToList();

                if (requestArgs.Count() > 0)
                {
                    //fix request
                    resource = resource.Substring(0, resource.IndexOf(StringHelper.CreateArgumentTag(requestArgs[0])) - 1);

                    var pveClient = PveHelper.GetClient();
                    var ret = ApiExplorer.ListValues(pveClient, PveHelper.GetClassApiRoot(pveClient), resource);
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
                    var pveClient = PveHelper.GetClient();
                    //execute request
                    var ret = ApiExplorer.Execute(pveClient,
                                                  PveHelper.GetClassApiRoot(pveClient),
                                                  resource,
                                                  MethodType,
                                                  ApiExplorer.CreateParameterResource(parameters),
                                                  false,
                                                  ApiExplorer.OutputType.Html);

                    if (ret.ResultCode != 200)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Error: {ret.ResultText}");
                    }
                    else
                    {
                        var filename = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resource).Replace("/", "");
                        await botClient.SendDocumentAsyncFromText(message.Chat.Id, ret.ResultText, $"{filename}.html");
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
                    if (args.Count > 1) { _messageText = string.Join(" ", args.Skip(1).ToArray()); }
                    break;

                case TypeRequest.Resource: _messageText = message.Text; break;
                case TypeRequest.ArgParameter: ReplaceArg(message.Text); break;
                default: break;
            }

            return await ExecuteOrChoose(message, botClient);
        }
    }
}