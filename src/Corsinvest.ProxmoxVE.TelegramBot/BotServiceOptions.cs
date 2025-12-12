/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

internal sealed class BotServiceOptions
{
    public string ChatToken { get; set; } = string.Empty;
    public List<long> ChatIds { get; set; } = [];
    public string Host { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? ApiToken { get; set; }
    public bool ValidateCertificate { get; set; }
    public bool ServiceMode { get; set; }
}

