/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using System.Text;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Helpers;

internal static class BotHelper
{
    public static async Task<Message> SendTextMessageAsyncNoKeyboard(this TelegramBotClient botClient,
                                                                     long chatId,
                                                                     string text)
        => await botClient.SendTextMessageAsync(chatId: chatId,
                                                text: text,
                                                parseMode: ParseMode.Html,
                                                replyMarkup: new ReplyKeyboardRemove());

    public static async Task<Message> SendDocumentAsyncFromText(this TelegramBotClient botClient,
                                                                long chatId,
                                                                string text,
                                                                string fileName)
    {

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        return await botClient.SendDocumentAsync(chatId, InputFile.FromStream(stream, fileName));
    }

    public static List<List<InlineKeyboardButton>> CreateInlineKeyboardButtons(IEnumerable<(string group, string text, string callbackData)> items)
    {
        var ikb = new List<List<InlineKeyboardButton>>();

        foreach (var group in items.GroupBy(a => a.group))
        {
            var rowIKB = new List<InlineKeyboardButton>();
            foreach (var (_, text, callbackData) in group)
            {
                if (rowIKB.Count > 1)
                {
                    ikb.Add(rowIKB);
                    rowIKB = [];
                }

                rowIKB.Add(InlineKeyboardButton.WithCallbackData(text, callbackData));
            }

            ikb.Add(rowIKB);
        }

        return ikb;
    }

    public static async Task ChooseInlineKeyboard(this TelegramBotClient botClient,
                                                  long chatId,
                                                  string title,
                                                  string[] items)
        => await botClient.ChooseInlineKeyboard(chatId, title, items.Select(a => ("", a, a)));

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
        var resources = await pveClient.GetResourcesAsync(ClusterResourceType.All);

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
        var items = (await pveClient.GetNodesAsync())
                        .Where(a => a.IsOnline == isOnline)
                        .Select(a => ("", a.Node, a.Node));

        await botClient.ChooseInlineKeyboard(chatId, "Choose Node", items);
    }
}