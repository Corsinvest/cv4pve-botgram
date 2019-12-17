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

using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Metadata;
using Corsinvest.ProxmoxVE.Api.Shell.Helpers;
using McMaster.Extensions.CommandLineUtils;

namespace Corsinvest.ProxmoxVE.TelegramBot.Helpers
{
    public static class PveHelper
    {
        private static ClassApi _classApiRoot;
        public static CommandLineApplication App { get; internal set; }

        public static ClassApi GetClassApiRoot(PveClient client)
            => _classApiRoot ?? (_classApiRoot = GeneretorClassApi.Generate(client.Hostname, client.Port));

        public static PveClient GetClient() => App.ClientTryLogin();
   }
}