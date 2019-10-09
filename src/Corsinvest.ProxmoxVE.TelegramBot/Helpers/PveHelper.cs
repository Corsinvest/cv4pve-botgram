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