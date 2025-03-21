/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */


/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Commands.Node;

internal class Shutdown : Base
{
    public override string Name => "nodeshutdown";
    public override string Description => "Shutdown node";
    protected override bool ForReboot => false;
}