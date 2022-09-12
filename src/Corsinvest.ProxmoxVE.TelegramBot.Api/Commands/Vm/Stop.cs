/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Vm.Api;

internal class Stop : Base
{
    public override string Name => "vmstop";
    public override string Description => "Stop VM/CT";
    protected override VmStatus StatusToChange => VmStatus.Stop;
    protected override bool StatusVmIsRunning => true;
}