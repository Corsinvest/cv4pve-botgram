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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Node;
using Corsinvest.ProxmoxVE.Api.Extension.VM;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Corsinvest.ProxmoxVE.TelegramBot.Helpers
{
    public static class BotHelper
    {
        public static async Task<Message> SendTextMessageAsyncNoKeyboard(this TelegramBotClient botClient,
                                                                         long chatId,
                                                                         string text)
            => await botClient.SendTextMessageAsync(chatId,
                                                    text,
                                                    ParseMode.Html,
                                                    replyMarkup: new ReplyKeyboardRemove());

        public static async Task<Message> SendDocumentAsyncFromText(this TelegramBotClient botClient,
                                                                    long chatId,
                                                                    string text,
                                                                    string fileName)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                return await botClient.SendDocumentAsync(chatId, new InputOnlineFile(stream, fileName));
            }
        }

        public static List<List<InlineKeyboardButton>> CreateInlineKeyboardButtons(IEnumerable<(string Group, string Text, string CallbackData)> items)
        {
            var ikb = new List<List<InlineKeyboardButton>>();

            foreach (var group in items.GroupBy(a => a.Group))
            {
                var rowIKB = new List<InlineKeyboardButton>();
                foreach (var item in group)
                {
                    if (rowIKB.Count > 1)
                    {
                        ikb.Add(rowIKB);
                        rowIKB = new List<InlineKeyboardButton>();
                    }

                    rowIKB.Add(InlineKeyboardButton.WithCallbackData(item.Text, item.CallbackData));
                }

                ikb.Add(rowIKB);
            }

            return ikb;
        }

        public static async Task ChooseInlineKeyboard(this TelegramBotClient botClient,
                                                      long chatId,
                                                      string title,
                                                      string[] items)
            => await ChooseInlineKeyboard(botClient, chatId, title, items.Select(a => ("", a, a)));

        public static async Task ChooseInlineKeyboard(this TelegramBotClient botClient,
                                                      long chatId,
                                                      string title,
                                                      IEnumerable<(string Group, string Text, string CallbackData)> items)
        {
            await botClient.SendChatActionAsync(chatId, ChatAction.Typing);

            await botClient.SendTextMessageAsync(chatId,
                                                 title,
                                                 replyMarkup: new InlineKeyboardMarkup(CreateInlineKeyboardButtons(items)));
        }

        public static async Task ChooseVmInlineKeyboard(this TelegramBotClient botClient,
                                                        long chatId,
                                                        PveClient pveClient,
                                                        bool isRunning)
        {
            var items = pveClient.GetVMs()
                                 .Where(a => pveClient.GetNode(a.Node).IsOnline && a.IsRunning == isRunning)
                                 .Select(a => (a.Node, $"{a.Id} {a.Name}", a.Id));

            await botClient.ChooseInlineKeyboard(chatId, "Choose Vm", items);
        }

        public static async Task ChooseNodeInlineKeyboard(this TelegramBotClient botClient,
                                                          long chatId,
                                                          PveClient pveClient,
                                                          bool isOnline)
        {
            var items = pveClient.GetNodes()
                                 .Where(a => pveClient.GetNode(a.Node).IsOnline == isOnline)
                                 .Select(a => ("", a.Id, a.Id));

            await botClient.ChooseInlineKeyboard(chatId, "Choose Node", items);
        }
    }
}