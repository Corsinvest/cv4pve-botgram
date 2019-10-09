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
        private TelegramBotClient _client;
        private Dictionary<long, (Message Message, Command Command)> _lastCommandForChat;

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

        private async void OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
            => await Console.Out.WriteLineAsync($"Received error: {e.Exception.Source} — {e.Exception.Message}");

        private async void OnReceiveError(object sender, ReceiveErrorEventArgs e)
            => await Console.Out.WriteLineAsync($"Received error: {e.ApiRequestException.ErrorCode} — {e.ApiRequestException.Message}");

        private async void OnInlineResultChosen(object sender, ChosenInlineResultEventArgs e)
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
                                Command.GetCommand(e.Message.Text):
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
