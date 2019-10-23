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
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api.Extension.Helpers.Shell;
using Corsinvest.ProxmoxVE.Api.Metadata;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Top
{
    public abstract class Base : Command
    {
        protected abstract string Type { get; }

        private static string GetValue(IDictionary<string, object> dic,
                                       string key,
                                       List<ParameterApi> returnParameters)
            => dic.TryGetValue(key, out var value) ?
                TableHelper.RendererValue(value, key, returnParameters) + "" :
                "";

        public override async Task<bool> Execute(Message message, TelegramBotClient botClient)
        {
            var result = PveHelper.GetClient().Cluster.Resources.GetRest(Type);
            var returnParameters = PveHelper.GetClassApiReturnParameters(result.RequestResource);

            string GetDisk(IDictionary<string, object> item)
                => $"Disk: <b>{GetValue(item, "disk", returnParameters)}</b>" +
                   $" of <b>{GetValue(item, "maxdisk", returnParameters)}</b>";

            string GetMem(IDictionary<string, object> item)
                => $" - Mem: <b>{GetValue(item, "mem", returnParameters)}</b>" +
                   $" of <b>{GetValue(item, "maxmem", returnParameters)}</b>";

            string GetStatus(IDictionary<string, object> item)
                => $" - Status: <b>{GetValue(item, "status", returnParameters)}</b> ===";

            string GetCpuUptime(IDictionary<string, object> item)
                => $"Cpu: <b>{GetValue(item, "cpu", returnParameters)}</b>" +
                   $" of <b>{GetValue(item, "maxcpu", returnParameters)}</b>" +
                   $" - Uptime: <b>{GetValue(item, "uptime", returnParameters)}</b>";

            string GetId(IDictionary<string, object> item) => GetValue(item, "id", returnParameters);

            var text = "";
            foreach (IDictionary<string, object> item in (IList)result.Response.data)
            {
                switch (item["type"] + "")
                {
                    case "node":
                        text += $"=== Node: <b>{GetId(item)}</b>" + GetStatus(item) +
                                Environment.NewLine +
                                GetCpuUptime(item) +
                                Environment.NewLine +
                                GetDisk(item) + GetMem(item) +
                                Environment.NewLine;
                        break;

                    case "qemu":
                    case "lxc":
                        text += $"=== Id : <b>{GetValue(item, "node", returnParameters)}/{GetId(item)}</b>" +
                                GetStatus(item) +
                                Environment.NewLine +
                                GetCpuUptime(item) +
                                Environment.NewLine +
                                GetDisk(item) + GetMem(item) +
                                Environment.NewLine;
                        break;

                    case "storage":
                        text += $"=== Id : <b>{GetId(item)}</b>" + GetStatus(item) +
                                Environment.NewLine +
                                GetDisk(item) + Environment.NewLine;
                        break;

                    default: break;
                }

            }

            await botClient.SendTextMessageAsyncNoKeyboard(message.Chat.Id, text);

            return await Task.FromResult(true);
        }
    }
}