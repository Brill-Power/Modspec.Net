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

    public void ReadCoils(int startingRegister, Span<byte> destination)
    {
        Span<byte> buffer = _client.ReadCoils(_unitId, startingRegister, destination.Length * 8);
        buffer.CopyTo(destination);
    }

    public async ValueTask ReadCoilsAsync(int startingRegister, Memory<byte> destination)
    {
        Memory<byte> buffer = await _client.ReadCoilsAsync(_unitId, startingRegister, destination.Length * 8);
        buffer.CopyTo(destination);
    }

    public void ReadDiscreteInputs(int startingRegister, Span<byte> destination)
    {
        Span<byte> buffer = _client.ReadDiscreteInputs(_unitId, startingRegister, destination.Length * 8);
        buffer.CopyTo(destination);
    }

    public async ValueTask ReadDiscreteInputsAsync(int startingRegister, Memory<byte> destination)
    {
        Memory<byte> buffer = await _client.ReadDiscreteInputsAsync(_unitId, startingRegister, destination.Length * 8);
        buffer.CopyTo(destination);
    }

    public void ReadHoldingRegisters(int startingRegister, Span<byte> destination)
    {
        _client.ReadManyHoldingRegisters(_unitId, startingRegister, destination);
    }

    public async ValueTask ReadHoldingRegistersAsync(int startingRegister, Memory<byte> destination)
    {
        await _client.ReadManyHoldingRegistersAsync(_unitId, startingRegister, destination);
    }

    public void ReadInputRegisters(int startingRegister, Span<byte> destination)
    {
        _client.ReadManyInputRegisters(_unitId, startingRegister, destination);
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
