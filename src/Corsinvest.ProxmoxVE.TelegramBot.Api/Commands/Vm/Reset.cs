/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Vm.Api;

internal class Reset : Base
{
    public override string Name => "vmreset";
    public override string Description => "Reset VM/CT";
    protected override VmStatus StatusToChange => VmStatus.Reset;
    protected override bool StatusVmIsRunning => true;
}