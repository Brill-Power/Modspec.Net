/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Threading.Tasks;
using BrillPower.FluentModbus;
using Modspec.Client.FluentModbus.Extensions;

namespace Modspec.Client.FluentModbus;

public class FluentModbusClient : IReadWriteModbusClient
{
    private readonly ModbusClient _client;
    private readonly byte _unitId;

    public FluentModbusClient(ModbusClient client, byte unitId)
    {
        _client = client;
        _unitId = unitId;
    }

    public async ValueTask ReadCoilsAsync(int startingRegister, Memory<byte> destination)
    {
        Memory<byte> buffer = await _client.ReadCoilsAsync(_unitId, startingRegister, destination.Length * 8);
        buffer.CopyTo(destination);
    }

    public async ValueTask ReadDiscreteInputsAsync(int startingRegister, Memory<byte> destination)
    {
        Memory<byte> buffer = await _client.ReadDiscreteInputsAsync(_unitId, startingRegister, destination.Length * 8);
        buffer.CopyTo(destination);
    }

    public async ValueTask ReadHoldingRegistersAsync(int startingRegister, Memory<byte> destination)
    {
        await _client.ReadManyHoldingRegistersAsync(_unitId, startingRegister, destination);
    }

    public async ValueTask ReadInputRegistersAsync(int startingRegister, Memory<byte> destination)
    {
        await _client.ReadManyInputRegistersAsync(_unitId, startingRegister, destination);
    }

    public void WriteSingleCoil(int register, bool value)
    {
        _client.WriteSingleCoil(_unitId, register, value);
    }

    public void WriteRegisters(int startingRegister, Memory<byte> value)
    {
        _client.WriteMultipleRegisters(_unitId, (ushort)startingRegister, value);
    }
}
