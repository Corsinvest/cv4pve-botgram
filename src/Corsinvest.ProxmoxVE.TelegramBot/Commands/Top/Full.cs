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

using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Top
{
    public class Cluster : Command
    {
        public Cluster() => Names.Add("❤️");
        public override string Name => "top";
        public override string Description => "Status cluster";
        public override async Task<bool> Execute(Message message, TelegramBotClient botClient)
        {
            await botClient.SendDocumentAsyncFromText(message.Chat.Id,
                                                      PveHelper.GetTableFromResource(PveHelper.GetClient(),
                                                                                     "/cluster/resources"),
                                                      "top.txt");

            return await Task.FromResult(true);
        }
    }
}