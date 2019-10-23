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

using System;
using System.Collections.Generic;
using Corsinvest.ProxmoxVE.TelegramBot.Commands;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Corsinvest.ProxmoxVE.TelegramBot
{
    public class BotManager
    {
        private readonly TelegramBotClient _client;
        private readonly Dictionary<long, (Message Message, Command Command)> _lastCommandForChat;

        public BotManager(string token)
        {
            _lastCommandForChat = new Dictionary<long, (Message, Command)>();

            _client = new TelegramBotClient(token);

            var result = _client.GetMeAsync().Result;
            Username = result.Username;

            _client.OnMessage += OnMessage;
            _client.OnMessageEdited += OnMessage;
            _client.OnCallbackQuery += OnCallbackQuery;
            //Bot.OnInlineQuery += BotOnInlineQueryReceived;
            _client.OnInlineResultChosen += OnInlineResultChosen;
            _client.OnReceiveError += OnReceiveError;
            _client.OnReceiveGeneralError += OnReceiveGeneralError;
        }

        public string Username { get; }

        public void StartReceiving() => _client.StartReceiving(Array.Empty<UpdateType>());
        public void StopReceiving() => _client.StopReceiving();

        private static async void OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
            => await Console.Out.WriteLineAsync($"Received error: {e.Exception.Source} — {e.Exception.Message}");

        private static async void OnReceiveError(object sender, ReceiveErrorEventArgs e)
            => await Console.Out.WriteLineAsync($"Received error: {e.ApiRequestException.ErrorCode} — {e.ApiRequestException.Message}");

        private static async void OnInlineResultChosen(object sender, ChosenInlineResultEventArgs e)
            => await Console.Out.WriteLineAsync($"OnInlineResultChosen: {e.ChosenInlineResult.ResultId}");

        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            await Console.Out.WriteLineAsync($"OnCallbackQuery: {e.CallbackQuery.Data}");

            var chatId = e.CallbackQuery.Message.Chat.Id;
            await _client.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

            try
            {
                if (_lastCommandForChat.TryGetValue(chatId, out var value))
                {
                    if (await value.Command.Execute(value.Message, e.CallbackQuery, _client))
                    {
                        _lastCommandForChat.Remove(chatId);
                    }
                }
                else
                {
                    _lastCommandForChat.Remove(chatId);
                    await _client.SendTextMessageAsyncNoKeyboard(chatId, $"Data not recognized '{e.CallbackQuery.Data}'");
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.StackTrace);
                await _client.SendTextMessageAsyncNoKeyboard(chatId, $"Error CallbackQuery! {ex.Message}");
            }
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            var chatId = e.Message.Chat.Id;
            await Console.Out.WriteLineAsync($"Chat Id: {chatId}");
            await Console.Out.WriteLineAsync($"User: {e.Message.Chat.Username} - " +
                                             $"{e.Message.Chat.FirstName} {e.Message.Chat.LastName}");
            await Console.Out.WriteLineAsync($"Message Type: {e.Message.Type}");


            var command = e.Message.Type == MessageType.Text ?
                                Command.GetCommand(e.Message.Text) :
                                null;

            var exists = _lastCommandForChat.ContainsKey(chatId);
            if (command == null && exists) { command = _lastCommandForChat[chatId].Command; }
            if (command == null) { command = new Help(); }

            //save last command and chat
            if (exists) { _lastCommandForChat.Remove(chatId); }
            _lastCommandForChat.Add(chatId, (e.Message, command));

            try
            {
                if (await command.Execute(e.Message, _client))
                {
                    _lastCommandForChat.Remove(chatId);
                }
            }
            catch (Exception ex)
            {
                _lastCommandForChat.Remove(chatId);
                await Console.Out.WriteLineAsync(ex.StackTrace);
                await _client.SendTextMessageAsyncNoKeyboard(chatId, $"Error execute command! {ex.Message}");
            }
        }
    }
}
