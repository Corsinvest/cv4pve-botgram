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

namespace Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api
{
    internal static class BotHelper
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
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            return await botClient.SendDocumentAsync(chatId, new InputOnlineFile(stream, fileName));
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