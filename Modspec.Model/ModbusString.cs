/*
 * Copyright (c) 2026 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Text;

namespace Modspec.Model;

public static class ModbusString
{
    public static string ReadNullTerminatedString(ReadOnlySpan<byte> buffer)
    {
        int end;
        for (end = 0; end < buffer.Length; end++)
        {
            if (buffer[end] == '\0')
            {
                break;
            }
        }
        return Encoding.UTF8.GetString(buffer.Slice(0, end));
    }
}
