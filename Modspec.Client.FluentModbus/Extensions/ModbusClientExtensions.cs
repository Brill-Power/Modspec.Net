/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Threading.Tasks;
using BrillPower.FluentModbus;

namespace Modspec.Client.FluentModbus.Extensions;

public static class ModbusClientExtensions
{
    public const int ModbusPageWidthInRegisters = 125;

    private delegate Span<byte> Read(ModbusClient client, byte unitId, ushort startingRegister, ushort count);
    private delegate Task<Memory<byte>> ReadAsync(ModbusClient client, byte unitId, ushort startingRegister, ushort count);

    public static void ReadManyInputRegisters(this ModbusClient self, byte unitId, int startingRegister, Span<byte> destination)
    {
        self.ReadMany(unitId, startingRegister,
            static (mc, ui, sa, co) => mc.ReadInputRegisters(ui, sa, co), destination);
    }

    public static async ValueTask<Memory<byte>> ReadManyInputRegistersAsync(this ModbusClient self, byte unitId, int startingRegister, int count)
    {
        return await self.ReadManyAsync(unitId, startingRegister, count,
            static (mc, ui, sa, co) => mc.ReadInputRegistersAsync(ui, sa, co));
    }

    public static async ValueTask ReadManyInputRegistersAsync(this ModbusClient self, byte unitId, int startingRegister, Memory<byte> destination)
    {
        await self.ReadManyAsync(unitId, startingRegister,
            static (mc, ui, sa, co) => mc.ReadInputRegistersAsync(ui, sa, co), destination);
    }

    public static Span<byte> ReadManyHoldingRegisters(this ModbusClient self, byte unitId, int startingRegister, int count)
    {
        return self.ReadMany(unitId, startingRegister, count,
            static (mc, ui, sa, co) => mc.ReadHoldingRegisters(ui, sa, co));
    }

    public static void ReadManyHoldingRegisters(this ModbusClient self, byte unitId, int startingRegister, Span<byte> destination)
    {
        self.ReadMany(unitId, startingRegister,
            static (mc, ui, sa, co) => mc.ReadHoldingRegisters(ui, sa, co), destination);
    }

    public static async ValueTask<Memory<byte>> ReadManyHoldingRegistersAsync(this ModbusClient self, byte unitId, int startingRegister, int count)
    {
        return await self.ReadManyAsync(unitId, startingRegister, count,
            static (mc, ui, sa, co) => mc.ReadHoldingRegistersAsync(ui, sa, co));
    }

    public static async ValueTask ReadManyHoldingRegistersAsync(this ModbusClient self, byte unitId, int startingRegister, Memory<byte> destination)
    {
        await self.ReadManyAsync(unitId, startingRegister,
            static (mc, ui, sa, co) => mc.ReadHoldingRegistersAsync(ui, sa, co), destination);
    }

    private static Span<byte> ReadMany(this ModbusClient self, byte unitId, int startingRegister, int count, Read reader)
    {
        Span<byte> result = new byte[count];
        self.ReadMany(unitId, startingRegister, reader, result);
        return result;
    }

    private static void ReadMany(this ModbusClient self, byte unitId, int startingRegister, Read reader, Span<byte> destination)
    {
        int registerCount = destination.Length / 2;
        for (int registerOffset = 0; registerOffset < registerCount; registerOffset += ModbusPageWidthInRegisters)
        {
            int width = Math.Min(ModbusPageWidthInRegisters, registerCount - registerOffset);
            ReadOnlySpan<byte> values = reader(self, unitId, (ushort)(startingRegister + registerOffset), (ushort)width);
            values.CopyTo(destination.Slice(registerOffset * 2));
        }
    }

    private static async ValueTask<Memory<byte>> ReadManyAsync(this ModbusClient self, byte unitId, int startingRegister, int count, ReadAsync reader)
    {
        Memory<byte> result = new byte[count];
        await ReadManyAsync(self, unitId, startingRegister, reader, result);
        return result;
    }

    private static async ValueTask ReadManyAsync(this ModbusClient self, byte unitId, int startingRegister, ReadAsync reader, Memory<byte> destination)
    {
        int registerCount = destination.Length / 2;
        for (int registerOffset = 0; registerOffset < registerCount; registerOffset += ModbusPageWidthInRegisters)
        {
            int width = Math.Min(ModbusPageWidthInRegisters, registerCount - registerOffset);
            ReadOnlyMemory<byte> values = await reader(self, unitId, (ushort)(startingRegister + registerOffset), (ushort)width);
            values.CopyTo(destination.Slice(registerOffset * 2));
        }
    }
}
