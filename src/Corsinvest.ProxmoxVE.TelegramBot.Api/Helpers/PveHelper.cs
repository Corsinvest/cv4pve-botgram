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

using System.IO;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Helpers;
using Corsinvest.ProxmoxVE.Api.Metadata;

namespace Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api
{
    internal static class PveHelper
    {
        private static ClassApi _classApiRoot;

        public static string HostandPortHA { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static TextWriter Out { get; internal set; }

        public static ClassApi GetClassApiRoot(PveClient client)
            => _classApiRoot ??= GeneretorClassApi.Generate(client.Hostname, client.Port);

        public static PveClient GetClient()
        {
            var client = ClientHelper.GetClientFromHA(HostandPortHA, Out);
            client.Login(Username, Password);
            return client;
        }
    }
}