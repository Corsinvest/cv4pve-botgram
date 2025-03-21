/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */


/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Commands.Api;

internal class Get : Base
{
    public override string Name => "get";
    public override string Description => "Get from resource and result to file";
    protected override MethodType MethodType => MethodType.Get;
}