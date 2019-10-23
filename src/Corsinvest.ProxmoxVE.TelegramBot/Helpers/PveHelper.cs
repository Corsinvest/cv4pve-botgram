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

using System.Collections.Generic;
using System.Linq;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Helpers.Shell;
using Corsinvest.ProxmoxVE.Api.Metadata;

namespace Corsinvest.ProxmoxVE.TelegramBot.Helpers
{
    public static class PveHelper
    {
        private static ClassApi _classApiRoot;
        public static string Host { get; set; }
        public static int Port { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }

        public static ClassApi GetClassApiRoot()
            => _classApiRoot ?? (_classApiRoot = GeneretorClassApi.Generate(Host, Port));

        public static PveClient GetClient()
        {
            var client = new PveClient(Host, Port);
            client.Login(Username, Password);
            return client;
        }

        public static ClassApi GetClassApiFromResource(string resource)
        {
            if (_classApiRoot == null) { _classApiRoot = GeneretorClassApi.Generate(Host, Port); }
            return ClassApi.GetFromResource(_classApiRoot, resource);
        }

        public static List<ParameterApi> GetClassApiReturnParameters(string resource)
        {
            return GetClassApiFromResource(resource)?.Methods
                                                     .Where(a => a.IsGet)
                                                     .FirstOrDefault()
                                                     .ReturnParameters;
        }

        public static string GetTableFromResource(PveClient client, string resource)
        {
            var result = client.Get(resource);
            var ret = "";

            if (!result.IsSuccessStatusCode)
            {
                ret = result.ReasonPhrase;
            }
            else
            {
                var returnParameters = GetClassApiReturnParameters(resource);

                if (returnParameters == null || returnParameters.Count == 0)
                {
                    ret = TableHelper.CreateTable(result.Response.data);
                }
                else
                {
                    var keys = returnParameters.OrderBy(a => a.Optional)
                                               .ThenBy(a => a.Name)
                                               .Select(a => a.Name)
                                               .ToArray();

                    ret = TableHelper.CreateTable(result.Response.data, keys, returnParameters);
                }
            }

            return ret;
        }
    }
}