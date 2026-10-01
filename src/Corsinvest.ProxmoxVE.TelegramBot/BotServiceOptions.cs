/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

namespace Corsinvest.ProxmoxVE.TelegramBot;

internal sealed class BotServiceOptions
{
    public string ChatToken { get; set; } = string.Empty;
    public List<long> ChatIds { get; set; } = [];
    public string Host { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? ApiToken { get; set; }
    public bool ValidateCertificate { get; set; }

    /// <summary>--service-mode was passed: it has no effect, the bot warns about it at start.</summary>
    public bool DeprecatedServiceMode { get; set; }

    /// <summary>Exit code of the process: 1 when the bot could not start or had to stop.</summary>
    public int ExitCode { get; set; }
}
