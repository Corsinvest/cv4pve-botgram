/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using System.Text;
using Microsoft.Extensions.Logging;

namespace Corsinvest.ProxmoxVE.TelegramBot;

/// <summary>Writes each line to a logger, for when there is no console to write to.</summary>
internal sealed class LoggerTextWriter(ILogger logger) : TextWriter
{
    private readonly StringBuilder _line = new();

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        lock (_line)
        {
            if (value == '\n') { Flush(); }
            else if (value != '\r') { _line.Append(value); }
        }
    }

    public override void Flush()
    {
        lock (_line)
        {
            if (_line.Length == 0) { return; }
            logger.LogInformation("{Line}", _line.ToString());
            _line.Clear();
        }
    }
}
