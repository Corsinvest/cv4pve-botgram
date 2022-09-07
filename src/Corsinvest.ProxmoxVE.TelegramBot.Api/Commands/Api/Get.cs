/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Api.Api;

internal class Get : Base
{
    public override string Name => "get";
    public override string Description => "Get from resource and result to file";
    protected override MethodType MethodType => MethodType.Get;
}