/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */


/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Commands.Vm;

internal class Shutdown : Base
{
    public override string Name => "vmshutdown";
    public override string Description => "Shutdown VM/CT";
    protected override VmStatus StatusToChange => VmStatus.Shutdown;
    protected override bool StatusVmIsRunning => true;
}