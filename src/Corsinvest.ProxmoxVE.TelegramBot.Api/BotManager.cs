/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.TelegramBot.Commands.Api;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api;

/// <summary>
/// Bot Manager
/// </summary>
public class BotManager
{
    private readonly long[] _chatsIdValid;
    private readonly TextWriter _out;
    private readonly Dictionary<long, (Message Message, Command Command)> _lastCommandForChat;
    private CancellationTokenSource Cts;
    private Dictionary<long, string> _chats;
    private readonly string _pveUsername;
    private readonly string _pvePassword;
    private readonly bool _pveValidateCertificate;
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _pveApiToken;
    internal TelegramBotClient BotClient { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="pveHostAndPortHA">Proxmox VE host and port HA format 10.1.1.90:8006,10.1.1.91:8006,10.1.1.92:8006</param>
    /// <param name="pveApiToken">Proxmox VE Api Token</param>
    /// <param name="pveUsername">Proxmox VE username</param>
    /// <param name="pvePassword">Proxmox VE password</param>
    /// <param name="pveValidateCertificate"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="token">Token Telegram Bot</param>
    /// <param name="chatsIdValid">Valid chats Id</param>
    /// <param name="out">Output write</param>
    public BotManager(string pveHostAndPortHA,
                      string pveApiToken,
                      string pveUsername,
                      string pvePassword,
                      bool pveValidateCertificate,
                      ILoggerFactory loggerFactory,
                      string token,
                      long[] chatsIdValid,
                      TextWriter @out)
    {
        PveHostAndPortHA = pveHostAndPortHA;
        _pveApiToken = pveApiToken;
        _pveUsername = pveUsername;
        _pvePassword = pvePassword;
        _pveValidateCertificate = pveValidateCertificate;
        _loggerFactory = loggerFactory;

        _chatsIdValid = chatsIdValid;
        _out = @out;
        _lastCommandForChat = [];
        _chats = [];

        //create telegram
        BotClient = new TelegramBotClient(token);
    }

    internal string PveHostAndPortHA { get; set; }

    /// <summary>
    /// Get client
    /// </summary>
    /// <returns></returns>
    internal async Task<PveClient> GetPveClientAsync()
        => await ClientHelper.GetClientAndTryLoginAsync(PveHostAndPortHA,
                                                        _pveUsername,
                                                        _pvePassword,
                                                        _pveApiToken,
                                                        _pveValidateCertificate,
                                                       _loggerFactory);

    /// <summary>
    /// Bot Id
    /// </summary>
    public long? BootId => BotClient.BotId;

    /// <summary>
    /// Chat username
    /// </summary>
    /// <value></value>
    public string Username { get; private set; }

    /// <summary>
    /// Send message
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="message"></param>
    public async Task SendMessageAsync(long chatId, string message) => await BotClient.SendTextMessageAsync(chatId, message);

    /// <summary>
    /// Chat info
    /// </summary>
    public IReadOnlyDictionary<long, string> Chats => _chats;

    /// <summary>
    /// Start chat
    /// </summary>
    public void StartReceiving()
    {
        Cts = new CancellationTokenSource();
        _chats = [];

        BotClient.StartReceiving(updateHandler: HandleUpdateAsync,
                               pollingErrorHandler: HandlePollingErrorAsync,
                               receiverOptions: new ReceiverOptions
                               {
                                   AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
                               },
                               cancellationToken: Cts.Token);

        var result = BotClient.GetMeAsync().Result;
        Username = result.Username;

        _out.WriteLine($@"Start listening
Telegram
  Bot User: @{Username}
  Bot UserId: @{result.Id}
Proxmox VE
  Host: {PveHostAndPortHA}
  Username: {_pveUsername}");
    }

    /// <summary>
    /// Stop chat
    /// </summary>
    public void StopReceiving()
    {
        //Send cancellation request to stop bot
        Cts.Cancel();
        Cts = null;
        _chats = [];
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is not null)
        {
            await ProcessCallbackQuery(update.CallbackQuery);
        }
        else if (update.Message is not null)
        {
            await ProcessMessage(update.Message);
        }
    }

    private async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        await _out.WriteLineAsync(ErrorMessage);
    }

    private async Task ProcessCallbackQuery(CallbackQuery callbackQuery)
    {
        await _out.WriteLineAsync($"OnCallbackQuery: {callbackQuery.Data}");

        var chatId = callbackQuery.Message.Chat.Id;
        await BotClient.DeleteMessageAsync(chatId, callbackQuery.Message.MessageId);

        try
        {
            if (_lastCommandForChat.TryGetValue(chatId, out var value))
            {
                if (await value.Command.Execute(value.Message, callbackQuery, this))
                {
                    _lastCommandForChat.Remove(chatId);
                }
            }
            else
            {
                _lastCommandForChat.Remove(chatId);
                await BotClient.SendTextMessageAsyncNoKeyboard(chatId, $"Data not recognized '{callbackQuery.Data}'");
            }
        }
        catch (Exception ex)
        {
            await _out.WriteLineAsync(ex.StackTrace);
            await BotClient.SendTextMessageAsyncNoKeyboard(chatId, $"Error CallbackQuery! {ex.Message}");
        }
    }

    private async Task ProcessMessage(Message message)
    {
        if (message.Text is not { } messageText) { return; }

        _chats.TryAdd(message.Chat.Id, $"{message.Chat.Username} - {message.Chat.LastName} {message.Chat.FirstName}");

        var chatId = message.Chat.Id;
        var log = $"{message.Date} - Chat Id: '{chatId}'" +
                  $" User: '{message.Chat.Username}'" +
                  $" - '{message.Chat.FirstName} {message.Chat.LastName}'" +
                  $" Msg Type: '{message.Type}'";
        if (message.Type == MessageType.Text) { log += $" => '{message.Text}'"; }
        await _out.WriteLineAsync(log);

        //check security Chat
        if (_chatsIdValid.Any() && !_chatsIdValid.Contains(chatId))
        {
            await _out.WriteLineAsync($"Security: Chat Id '{chatId}' - Username '{message.Chat.Username}' not permitted access!");
            await BotClient.SendTextMessageAsync(chatId, "You not have permission in this Chat!");
            return;
        }

        var command = message.Type == MessageType.Text
                        ? Command.GetCommand(message.Text)
                        : null;

        var exists = _lastCommandForChat.ContainsKey(chatId);
        if (command == null && exists) { command = _lastCommandForChat[chatId].Command; }
        command ??= new Help();

        //save last command and chat
        if (exists) { _lastCommandForChat.Remove(chatId); }
        _lastCommandForChat.Add(chatId, (message, command));

        try
        {
            if (await command.Execute(message, this))
            {
                _lastCommandForChat.Remove(chatId);
            }
        }
        catch (Exception ex)
        {
            _lastCommandForChat.Remove(chatId);
            await _out.WriteLineAsync(ex.StackTrace);
            await BotClient.SendTextMessageAsyncNoKeyboard(chatId, $"Error execute command! {ex.Message}");
        }
    }
}