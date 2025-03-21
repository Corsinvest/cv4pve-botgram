/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.TelegramBot.Api.Commands;
using Corsinvest.ProxmoxVE.TelegramBot.Api.Helpers;
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
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="pveHostAndPortHA">Proxmox VE host and port HA format 10.1.1.90:8006,10.1.1.91:8006,10.1.1.92:8006</param>
/// <param name="pveApiToken">Proxmox VE Api Token</param>
/// <param name="pveUsername">Proxmox VE username</param>
/// <param name="pvePassword">Proxmox VE password</param>
/// <param name="pveValidateCertificate"></param>
/// <param name="loggerFactory"></param>
/// <param name="token">Token Telegram Bot</param>
/// <param name="chatsIdValid">Valid chats Id</param>
/// <param name="out">Output write</param>
public class BotManager(string pveHostAndPortHA,
                        string pveApiToken,
                        string pveUsername,
                        string pvePassword,
                        bool pveValidateCertificate,
                        ILoggerFactory loggerFactory,
                        string token,
                        long[] chatsIdValid,
                        TextWriter @out)
{
    private readonly Dictionary<long, (Message Message, Command Command)> _lastCommandForChat = [];
    private CancellationTokenSource Cts;
    private Dictionary<long, string> _chats = [];
    internal TelegramBotClient BotClient { get; private set; } = new TelegramBotClient(token);

    internal string PveHostAndPortHA { get; set; } = pveHostAndPortHA;

    /// <summary>
    /// Get client
    /// </summary>
    /// <returns></returns>
    internal async Task<PveClient> GetPveClientAsync()
        => await ClientHelper.GetClientAndTryLoginAsync(PveHostAndPortHA,
                                                        pveUsername,
                                                        pvePassword,
                                                        pveApiToken,
                                                        pveValidateCertificate,
                                                       loggerFactory);

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
                                     AllowedUpdates = [] // receive all update types
                                 },
                                 cancellationToken: Cts.Token);

        var result = BotClient.GetMeAsync().Result;
        Username = result.Username;

        @out.WriteLine($@"Start listening
Telegram
  Bot User: @{Username}
  Bot UserId: @{result.Id}
Proxmox VE
  Host: {PveHostAndPortHA}
  Username: {pveUsername}");
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

        await @out.WriteLineAsync(ErrorMessage);
    }

    private async Task ProcessCallbackQuery(CallbackQuery callbackQuery)
    {
        await @out.WriteLineAsync($"OnCallbackQuery: {callbackQuery.Data}");

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
            await @out.WriteLineAsync(ex.StackTrace);
            await BotClient.SendTextMessageAsyncNoKeyboard(chatId, $"Error CallbackQuery! {ex.Message}");
        }
    }

    private async Task ProcessMessage(Message message)
    {
        if (message.Text is not { }) { return; }

        _chats.TryAdd(message.Chat.Id, $"{message.Chat.Username} - {message.Chat.LastName} {message.Chat.FirstName}");

        var chatId = message.Chat.Id;
        var log = $"{message.Date} - Chat Id: '{chatId}'" +
                  $" User: '{message.Chat.Username}'" +
                  $" - '{message.Chat.FirstName} {message.Chat.LastName}'" +
                  $" Msg Type: '{message.Type}'";
        if (message.Type == MessageType.Text) { log += $" => '{message.Text}'"; }
        await @out.WriteLineAsync(log);

        //check security Chat
        if (chatsIdValid.Any() && !chatsIdValid.Contains(chatId))
        {
            await @out.WriteLineAsync($"Security: Chat Id '{chatId}' - Username '{message.Chat.Username}' not permitted access!");
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
            await @out.WriteLineAsync(ex.StackTrace);
            await BotClient.SendTextMessageAsyncNoKeyboard(chatId, $"Error execute command! {ex.Message}");
        }
    }
}