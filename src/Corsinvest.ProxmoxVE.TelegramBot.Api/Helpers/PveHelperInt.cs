/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.IO;
using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.Api.Metadata;

namespace Corsinvest.ProxmoxVE.TelegramBot.Helpers.Api;

internal static class PveHelperInt
{
    private static ClassApi _classApiRoot;

    public static string HostAndPortHA { get; set; }
    public static string Username { get; set; }
    public static string Password { get; set; }
    public static string ApiToken { get; set; }
    public static TextWriter Out { get; set; }

    public static async Task<ClassApi> GetClassApiRoot(PveClient client)
        => _classApiRoot ??= await GeneratorClassApi.Generate(client.Host, client.Port);

    public static async Task<PveClient> GetClient()
    {
        var client = ClientHelper.GetClientFromHA(HostAndPortHA);
        if (string.IsNullOrWhiteSpace(ApiToken))
        {
            await client.Login(Username, Password);
        }
        else
        {
            client.ApiToken = ApiToken;
        }

        return client;
    }
}