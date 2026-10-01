/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */
using Corsinvest.ProxmoxVE.Api;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Commands.Api;

internal class Delete : Base
{
    public override string Name => "delete";
    public override string Description => "Delete from resource and result to file";
    protected override MethodType MethodType => MethodType.Delete;
}
