/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Vm.Api;

internal class Start : Base
{
    public override string Name => "vmstart";
    public override string Description => "Start VM/CT";
    protected override VmStatus StatusRequest => VmStatus.Start;
}