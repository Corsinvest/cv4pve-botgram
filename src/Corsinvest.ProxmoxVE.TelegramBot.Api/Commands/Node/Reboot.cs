/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Node.Api;

internal class Reboot: Base
{
    public override string Name => "nodereboot";
    public override string Description => "Reboot node";
    protected override bool ForReboot => true;
}