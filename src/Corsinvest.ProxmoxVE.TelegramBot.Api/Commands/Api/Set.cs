/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Commands.Api;

internal class Set : Base
{
    public override string Name => "set";
    public override string Description => "Set from resource and result to file";
    protected override MethodType MethodType => MethodType.Set;
}