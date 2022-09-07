/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;

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
            foreach (var (_, Text, CallbackData) in group)
            {
                if (rowIKB.Count > 1)
                {
                    ikb.Add(rowIKB);
                    rowIKB = new List<InlineKeyboardButton>();
                }

                rowIKB.Add(InlineKeyboardButton.WithCallbackData(Text, CallbackData));
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
        var resources = await pveClient.GetResources(ClusterResourceType.All);

        var nodes = resources.Where(a => a.ResourceType == ClusterResourceType.Node
                                    && a.IsOnline).Select(a => a.Node);

        var vms = resources.Where(a => a.ResourceType == ClusterResourceType.Vm
                                    && nodes.Contains(a.Node)
                                    && a.IsRunning == isRunning);

        var items = vms.Select(a => (a.Node, $"{a.Id} {a.Name}", a.VmId.ToString()));

        await botClient.ChooseInlineKeyboard(chatId, "Choose Vm", items);
    }

    public static async Task ChooseNodeInlineKeyboard(this TelegramBotClient botClient,
                                                      long chatId,
                                                      PveClient pveClient,
                                                      bool isOnline)
    {
        var items = (await pveClient.GetNodes())
                        .Where(a => a.IsOnline == isOnline)
                        .Select(a => ("", a.Node, a.Node));

        await botClient.ChooseInlineKeyboard(chatId, "Choose Node", items);
    }
}