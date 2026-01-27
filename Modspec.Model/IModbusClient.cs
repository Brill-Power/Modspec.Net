/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Threading.Tasks;

namespace Modspec.Model;

/// <summary>
/// Interface for a Modbus client.
/// </summary>
public interface IModbusClient : IDisposable
{
    void ReadInputRegisters(int startingRegister, Span<byte> destination);
    void ReadHoldingRegisters(int startingRegister, Span<byte> destination);
    void ReadCoils(int startingRegister, Span<byte> destination);
    void ReadDiscreteInputs(int startingRegister, Span<byte> destination);

    /// <summary>
    /// Reads a number of input registers starting at <paramref name="startingRegister"/>.
    /// The number of registers to read is specified by the size of <paramref name="destination"/>.
    /// </summary>
    /// <param name="startingRegister">The first register to read.</param>
    /// <param name="destination">The buffer into which data should be read.</param>
    /// <returns>A <see cref="ValueTask"/>.</returns>
    ValueTask ReadInputRegistersAsync(int startingRegister, Memory<byte> destination);
    /// <summary>
    /// Reads a number of holding registers starting at <paramref name="startingRegister"/>.
    /// The number of registers to read is specified by the size of <paramref name="destination"/>.
    /// </summary>
    /// <param name="startingRegister">The first register to read.</param>
    /// <param name="destination">The buffer into which data should be read.</param>
    /// <returns>A <see cref="ValueTask"/>.</returns>
    ValueTask ReadHoldingRegistersAsync(int startingRegister, Memory<byte> destination);
    /// <summary>
    /// Reads a number of coils starting at <paramref name="startingRegister"/>.
    /// The number of registers to read is specified by the size of <paramref name="destination"/>,
    /// where 8 registers will be read for every byte in <paramref name="destination"/>.
    /// </summary>
    /// <param name="startingRegister">The first register to read.</param>
    /// <param name="destination">The buffer into which data should be read.</param>
    /// <returns>A <see cref="ValueTask"/>.</returns>
    ValueTask ReadCoilsAsync(int startingRegister, Memory<byte> destination);
    /// <summary>
    /// Reads a number of discrete inputs starting at <paramref name="startingRegister"/>.
    /// The number of registers to read is specified by the size of <paramref name="destination"/>,
    /// where 8 registers will be read for every byte in <paramref name="destination"/>.
    /// </summary>
    /// <param name="startingRegister">The first register to read.</param>
    /// <param name="destination">The buffer into which data should be read.</param>
    /// <returns>A <see cref="ValueTask"/>.</returns>
    ValueTask ReadDiscreteInputsAsync(int startingRegister, Memory<byte> destination);
}
