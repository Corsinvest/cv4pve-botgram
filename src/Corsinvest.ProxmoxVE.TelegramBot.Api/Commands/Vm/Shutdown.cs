/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands.Vm.Api;

internal class Shutdown : Base
{
    public override string Name => "vmshutdown";
    public override string Description => "Shutdown VM/CT";
    protected override VmStatus StatusRequest => VmStatus.Shutdown;
}