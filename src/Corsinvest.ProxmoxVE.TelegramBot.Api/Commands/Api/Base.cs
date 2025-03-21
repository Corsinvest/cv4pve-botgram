﻿/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using System.CommandLine.Parsing;
using System.Globalization;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using Corsinvest.ProxmoxVE.TelegramBot.Api.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Commands.Api;

internal abstract class Base : Command
{
    public static readonly string DEFAULT_MSG = "Insert <b>resource</b> (eg nodes)" +
                                                Environment.NewLine +
                                                "More <b>info</b> see documentation <a href = 'https://pve.proxmox.com/pve-docs/api-viewer/index.html'>API</a>";

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

    public override async Task<bool> Execute(Message message, CallbackQuery callbackQuery, BotManager botManager)
    {
        if (string.IsNullOrWhiteSpace(_messageText))
        {
            await botManager.BotClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, "No command!");
            await Task.CompletedTask;
        }

        ReplaceArg(callbackQuery.Data);

        return await ExecuteOrChoose(message, botManager);
    }

    private async Task<bool> ExecuteOrChoose(Message message, BotManager botManager)
    {
        var endCommand = false;
        var cmdArgs = new Parser().Parse(_messageText).Tokens.Select(a => a.Value).ToList();
        if (cmdArgs.Count == 0)
        {
            //request resource
            _typeRequest = TypeRequest.Resource;
            await botManager.BotClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, DEFAULT_MSG);
        }
        else
        {
            var resource = cmdArgs[0];
            if (!resource.StartsWith('/')) { resource = "/" + resource; }
            var requestArgs = ApiExplorerHelper.GetArgumentTags(resource);
            var parameters = cmdArgs.Skip(1).ToArray();
            var parametersArgs = parameters.SelectMany(a => ApiExplorerHelper.GetArgumentTags(a)).ToList();

            if (requestArgs.Any())
            {
                //fix request
                resource = resource[..(resource.IndexOf(ApiExplorerHelper.CreateArgumentTag(requestArgs[0])) - 1)];

                var pveClient = await botManager.GetPveClientAsync();
                var (Values, Error) = await ApiExplorerHelper.ListValuesAsync(pveClient, await GetClassApiRoot(pveClient), resource);
                if (!string.IsNullOrWhiteSpace(Error))
                {
                    //return error
                    await botManager.BotClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, Error);
                    endCommand = true;
                }
                else
                {
                    _typeRequest = TypeRequest.ArgResource;

                    await botManager.BotClient.ChooseInlineKeyboard(message.Chat.Id,
                                                         $"Choose {requestArgs[0]}",
                                                         Values.Select(a => ("", a.Value, a.Value)));
                }
            }
            else if (parametersArgs.Any())
            {
                //request parameter value
                _typeRequest = TypeRequest.ArgParameter;

                await botManager.BotClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id,
                                                                          $"Insert value for parametr <b>{parametersArgs[0]}</b>");
            }
            else if (requestArgs.Length == 0)
            {
                var pveClient = await botManager.GetPveClientAsync();
                //execute request
                var (ResultCode, ResultText) = await ApiExplorerHelper.ExecuteAsync(pveClient,
                                                                                    await GetClassApiRoot(pveClient),
                                                                                    resource,
                                                                                    MethodType,
                                                                                    ApiExplorerHelper.CreateParameterResource(parameters),
                                                                                    false,
                                                                                    TableGenerator.Output.Html);

                if (ResultCode != 200)
                {
                    await botManager.BotClient.SendTextMessageAsync(message.Chat.Id, $"Error: {ResultText}");
                }
                else
                {
                    var filename = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resource).Replace("/", "-");
                    await botManager.BotClient.SendDocumentAsyncFromText(message.Chat.Id, ResultText, $"{filename}.html");
                }

                endCommand = true;
            }
        }

        if (endCommand) { _messageText = ""; }

        return endCommand;
    }

    private void ReplaceArg(string value)
        => _messageText = _messageText.Replace(
                            ApiExplorerHelper.CreateArgumentTag(ApiExplorerHelper.GetArgumentTags(_messageText)[0]),
                            value);

    public override async Task<bool> Execute(Message message, BotManager botManager)
    {
        switch (_typeRequest)
        {
            case TypeRequest.Start:
                _messageText = message.Text;
                var args = new Parser().Parse(_messageText).Tokens.Select(a => a.Value).ToList();
                if (args.Count > 1) { _messageText = string.Join(" ", [.. args.Skip(1)]); }
                break;

            case TypeRequest.Resource: _messageText = message.Text; break;
            case TypeRequest.ArgParameter: ReplaceArg(message.Text); break;
            default: break;
        }

        return await ExecuteOrChoose(message, botManager);
    }
}