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

using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;
using System.Collections.Generic;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Helpers;
using Corsinvest.ProxmoxVE.Api.Extension.Utility;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api
{
    internal class Alias : Command
    {
        private enum RequestType
        {
            Action,
            CreateRequestName,
            CreateRequestDescription,
            CreateRequestCommand,
            DeleteRequestName
        }

        private RequestType _requestType = RequestType.Action;
        private readonly AliasManager _aliasManager;
        private string _name;
        private string _description;

        public Alias()
        {
            _aliasManager = new AliasManager()
            {
                FileName = Path.Combine(ApplicationHelper.GetApplicationDataDirectory("cv4pve-botgram"), "alias.txt")
            };
            _aliasManager.Load();
        }

        public override string Name => "alias";
        public override string Description => "Create alias commands";

        public override async Task<bool> Execute(Message message,
                                                 CallbackQuery callbackQuery,
                                                 TelegramBotClient botClient)
        {
            var endCommand = false;
            switch (_requestType)
            {
                case RequestType.Action:
                    switch (callbackQuery.Data)
                    {
                        case "List":
                            await botClient.SendDocumentAsyncFromText(message.Chat.Id,
                                                                      _aliasManager.ToTable(true,
                                                                                            TableOutputType.Html),
                                                                      "alias.html");
                            endCommand = true;
                            break;

                        case "Delete":
                            //request name
                            _requestType = RequestType.DeleteRequestName;
                            await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, "Name alias");
                            break;

                        case "Create":
                            //request name
                            _requestType = RequestType.CreateRequestName;
                            await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, "Name alias");
                            break;

                        default: break;
                    }
                    break;

                default: break;
            }

            return await Task.FromResult(endCommand);
        }

        public override async Task<bool> Execute(Message message, TelegramBotClient botClient)
        {
            var endCommand = false;

            switch (_requestType)
            {
                case RequestType.Action:
                    await botClient.ChooseInlineKeyboard(message.Chat.Id,
                                                         "Choose action",
                                                         new[] { "List", "Delete", "Create" });
                    break;

                case RequestType.DeleteRequestName:
                    if (_aliasManager.Exists(message.Text))
                    {
                        _aliasManager.Remove(message.Text);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id,
                                                                       $"Name not valid '{message.Text}'");
                    }
                    endCommand = true;
                    break;

                case RequestType.CreateRequestName:
                    _name = message.Text.Trim();
                    if (_aliasManager.Exists(_name))
                    {
                        await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id,
                                                                       $"Name not valid '{_name}'");
                        endCommand = true;
                    }
                    else
                    {
                        await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, "Description");
                        _requestType = RequestType.CreateRequestDescription;
                    }
                    break;

                case RequestType.CreateRequestDescription:
                    _description = message.Text.Trim();
                    await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, "Description");
                    _requestType = RequestType.CreateRequestCommand;
                    break;

                case RequestType.CreateRequestCommand:
                    _aliasManager.Create(_name, _description, message.Text.Trim(), false);
                    endCommand = true;
                    break;

                default: break;
            }

            return await Task.FromResult(endCommand);
        }

        public IReadOnlyList<AliasCommand> GetCommandsFromAlias()
            => _aliasManager.Alias.Select(a => new AliasCommand(a)).ToList().AsReadOnly();
    }
}