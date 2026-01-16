/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using Modspec.Model;

namespace Modspec.Client;

public interface IReadWriteModbusClient : IModbusClient
{
    void WriteSingleCoil(int register, bool value);
    void WriteRegisters(int startingRegister, Memory<byte> value);
}
