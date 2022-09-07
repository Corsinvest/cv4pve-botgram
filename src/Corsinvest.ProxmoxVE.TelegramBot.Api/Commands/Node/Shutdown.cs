/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Node.Api;

internal class Shutdown : Base
{
    public override string Name => "nodeshutdown";
    public override string Description => "Shutdown node";
    protected override bool ForReboot => false;
}