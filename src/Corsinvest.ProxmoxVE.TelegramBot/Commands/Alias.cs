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

using System.IO;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api.Extension.Shell;
using Corsinvest.ProxmoxVE.Api.Extension.Helpers.Shell;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers;
using System.Collections.Generic;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands
{
    public class Alias : Command
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
        private AliasManager _aliasManager;
        private string _name;
        private string _description;

        public Alias()
        {
            _aliasManager = new AliasManager()
            {
                FileName = Path.Combine(ShellHelper.GetApplicationDataDirectory(Program.APP_NAME), "alias.txt")
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
                                                                      _aliasManager.ToTable(true),
                                                                      "list.txt");
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