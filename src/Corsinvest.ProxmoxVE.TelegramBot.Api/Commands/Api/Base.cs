/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using System.CommandLine.Parsing;
using System.Globalization;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Shell;
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
    protected string FileName { get; set; } = string.Empty;
    private string _messageText = string.Empty;
    private TypeRequest _typeRequest = TypeRequest.Start;

    public override async Task<bool> Execute(Message message, CallbackQuery callbackQuery, BotManager botManager)
    {
        if (string.IsNullOrWhiteSpace(_messageText))
        {
            await botManager.BotClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, "No command!");
            await Task.CompletedTask;
        }

        if (!string.IsNullOrWhiteSpace(callbackQuery.Data))
        {
            ReplaceArg(callbackQuery.Data);
        }

        return await ExecuteOrChoose(message, botManager);
    }

    private async Task<bool> ExecuteOrChoose(Message message, BotManager botManager)
    {
        var endCommand = false;

        // Parse command line arguments - handle quoted strings properly
        var cmdArgs = CommandLineParser.SplitCommandLine(_messageText).ToList();
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
            var requestArgs = ApiCommandLine.GetPlaceholders(resource);
            var parameters = cmdArgs.Skip(1).ToArray();
            var parametersArgs = parameters.SelectMany(ApiCommandLine.GetPlaceholders).ToList();

            if (requestArgs.Count != 0)
            {
                //fix request
                resource = resource[..(resource.IndexOf(CreateArgumentTag(requestArgs[0])) - 1)];

                var pveClient = await botManager.GetPveClientAsync();
                var children = await ApiSchema.GetChildrenAsync(pveClient, await GetClassApiRoot(pveClient), resource);
                if (!string.IsNullOrWhiteSpace(children.Error))
                {
                    //return error
                    await botManager.BotClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, children.Error);
                    endCommand = true;
                }
                else
                {
                    _typeRequest = TypeRequest.ArgResource;

                    await botManager.BotClient.ChooseInlineKeyboard(message.Chat.Id,
                                                         $"Choose {requestArgs[0]}",
                                                         children.Children.Select(a => ("", a.Name, a.Name)));
                }
            }
            else if (parametersArgs.Count != 0)
            {
                //request parameter value
                _typeRequest = TypeRequest.ArgParameter;

                await botManager.BotClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id,
                                                                          $"Insert value for parametr <b>{parametersArgs[0]}</b>");
            }
            else if (requestArgs.Count == 0)
            {
                var pveClient = await botManager.GetPveClientAsync();
                //execute request
                var response = await ApiRequest.ExecuteAsync(pveClient, new ApiCommand(MethodType, resource, CreateParameters(parameters)));

                if (!response.IsSuccess)
                {
                    var error = string.Join(Environment.NewLine,
                                            new[] { response.Error ?? string.Empty }
                                                .Concat(response.ParameterErrors.Select(a => $"{a.Key} : {a.Value}")));
                    await botManager.BotClient.SendTextMessageAsync(message.Chat.Id, $"Error: {error}");
                }
                else
                {
                    var table = ApiSchema.ToTable(response.Data, await GetClassApiRoot(pveClient), resource);
                    var text = table?.To(TableGenerator.Output.Html) ?? (response.Data + string.Empty);
                    var filename = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resource).Replace("/", "-");
                    await botManager.BotClient.SendDocumentAsyncFromText(message.Chat.Id, text, $"{filename}.html");
                }

                endCommand = true;
            }
        }

        if (endCommand) { _messageText = ""; }

        return endCommand;
    }

    private void ReplaceArg(string value)
        => _messageText = _messageText.Replace(CreateArgumentTag(ApiCommandLine.GetPlaceholders(_messageText)[0]), value);

    private static string CreateArgumentTag(string name) => "{" + name + "}";

    //parameters written as key:value
    private static Dictionary<string, object> CreateParameters(IEnumerable<string> items)
    {
        var parameters = new Dictionary<string, object>();
        foreach (var item in items)
        {
            var pos = item.IndexOf(':');
            if (pos < 0) { continue; }

            var key = item[..pos];
            if (!parameters.TryAdd(key, item[(pos + 1)..]))
            {
                throw new ArgumentException($"Parameter '{key}' is given more than once.");
            }
        }
        return parameters;
    }

    public override async Task<bool> Execute(Message message, BotManager botManager)
    {
        switch (_typeRequest)
        {
            case TypeRequest.Start:
                _messageText = message.Text!;
                var args = CommandLineParser.SplitCommandLine(_messageText).ToList();
                if (args.Count > 1) { _messageText = string.Join(" ", [.. args.Skip(1)]); }
                break;

            case TypeRequest.Resource: _messageText = message.Text!; break;
            case TypeRequest.ArgParameter: ReplaceArg(message.Text!); break;
            default: break;
        }

        return await ExecuteOrChoose(message, botManager);
    }
}