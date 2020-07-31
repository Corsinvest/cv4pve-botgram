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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.TelegramBot.Commands.Api;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api
{
    /// <summary>
    /// Botmanager
    /// </summary>
    public class BotManager
    {
        private readonly TelegramBotClient _client;
        private readonly long[] _chatsIdValid;
        private readonly TextWriter _out;
        private readonly Dictionary<long, (Message Message, Command Command)> _lastCommandForChat;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pveHostandPortHA">Proxmox VE host and port HA format 10.1.1.90:8006,10.1.1.91:8006,10.1.1.92:8006</param>
        /// <param name="pveApiToken">Proxmox VE Api Token</param>
        /// <param name="pveUsername">Proxmox VE username</param>
        /// <param name="pvePassword">Proxmox VE password</param>
        /// <param name="token">Token Telegram Bot</param>
        /// <param name="chatsIdValid">Valid chats Id</param>
        /// <param name="out">Output write</param>
        public BotManager(string pveHostandPortHA,
                          string pveApiToken,
                          string pveUsername,
                          string pvePassword,
                          string token,
                          long[] chatsIdValid,
                          TextWriter @out)
        {
            PveHelper.HostandPortHA = pveHostandPortHA;
            PveHelper.ApiToken = pveApiToken;
            PveHelper.Username = pveUsername;
            PveHelper.Password = pvePassword;
            PveHelper.Out = @out;

            _chatsIdValid = chatsIdValid;
            _out = @out;
            _lastCommandForChat = new Dictionary<long, (Message, Command)>();

            //create bootgram telegram
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

        /// <summary>
        /// Chat username
        /// </summary>
        /// <value></value>
        public string Username { get; }

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="message"></param>
        public async Task SendMessageAsync(long chatId, string message) => await _client.SendTextMessageAsync(chatId, message);

        /// <summary>
        /// Start chat
        /// </summary>
        public void StartReceiving()
        {
            _client.StartReceiving(Array.Empty<UpdateType>());

            _out.WriteLine($@"Start listening
Telegram
  Bot User: @{Username}
Proxmox VE
  Host: {PveHelper.HostandPortHA}
  Username: {PveHelper.Username}");
        }

        /// <summary>
        /// Stop chat
        /// </summary>
        public void StopReceiving() => _client.StopReceiving();

        private async void OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
            => await _out.WriteLineAsync($"Received error: {e.Exception.Source} — {e.Exception.Message}");

        private async void OnReceiveError(object sender, ReceiveErrorEventArgs e)
            => await _out.WriteLineAsync($"Received error: {e.ApiRequestException.ErrorCode} — {e.ApiRequestException.Message}");

        private async void OnInlineResultChosen(object sender, ChosenInlineResultEventArgs e)
            => await _out.WriteLineAsync($"OnInlineResultChosen: {e.ChosenInlineResult.ResultId}");

        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            await _out.WriteLineAsync($"OnCallbackQuery: {e.CallbackQuery.Data}");

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
                await _out.WriteLineAsync(ex.StackTrace);
                await _client.SendTextMessageAsyncNoKeyboard(chatId, $"Error CallbackQuery! {ex.Message}");
            }
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            var chatId = e.Message.Chat.Id;
            var log = $"{e.Message.Date} - Chat Id: '{chatId}'" +
                      $" User: '{e.Message.Chat.Username}'" +
                      $" - '{e.Message.Chat.FirstName} {e.Message.Chat.LastName}'" +
                      $" Msg Type: '{e.Message.Type}'";
            if (e.Message.Type == MessageType.Text) { log += $"=> '{e.Message.Text}'"; }
            await _out.WriteLineAsync(log);

            //check security Chat
            if (_chatsIdValid.Count() > 0 && !_chatsIdValid.Contains(chatId))
            {
                await _out.WriteLineAsync($"Security: Chat Id '{chatId}' - Username '{e.Message.Chat.Username}' not permitted access!");
                await _client.SendTextMessageAsync(chatId, "You not have permission in this Chat!");
                return;
            }

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
                await _out.WriteLineAsync(ex.StackTrace);
                await _client.SendTextMessageAsyncNoKeyboard(chatId, $"Error execute command! {ex.Message}");
            }
        }
    }
}
