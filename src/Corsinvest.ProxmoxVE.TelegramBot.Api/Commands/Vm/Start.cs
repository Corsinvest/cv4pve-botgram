﻿/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */


/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Commands.Vm;

internal class Start : Base
{
    public override string Name => "vmstart";
    public override string Description => "Start VM/CT";
    protected override VmStatus StatusToChange => VmStatus.Start;
    protected override bool StatusVmIsRunning => false;
}