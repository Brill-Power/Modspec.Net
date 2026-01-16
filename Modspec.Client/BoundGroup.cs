/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modspec.Model;

namespace Modspec.Client;

/// <summary>
/// Represents a range of registers on a connected Modbus device.
/// </summary>
public class BoundGroup
{
    private readonly IReadWriteModbusClient _client;
    private readonly ushort _offset;
    private readonly Memory<byte> _buffer;
    private readonly int _modelSize;

    public BoundGroup(IReadWriteModbusClient client, ushort offset, Group group, ReadValue readValue, WriteValue writeValue)
    {
        _client = client;
        _offset = offset;

        Group = group;

        _modelSize = group.Points.Sum(p => p.SizeInBytes * (p.Count?.MaxValue ?? 1));
        _buffer = new byte[_modelSize];

        List<IModelValue> values = [];
        int bufferOffset = 0;
        int register = 0;
        foreach (Point point in group.Points)
        {
            int size = point.SizeInBytes * (point.Count?.MaxValue ?? 1);
            int modiconId = (100000 * (int)group.Table) + group.BaseRegister + offset + register;
            if (point.Type != PointType.Padding)
            {
                CommitValue commitValue;
                switch (Group.Table, point.Count?.MaxValue)
                {
                    case (Table.Coils, null):
                        // writeValue = (point, value, slice) => slice[0] = (byte)((value is bool b && b) ? 0x01 : 0x00);
                        // commitValue = (reg, bytes) => _client.WriteSingleCoil(Group.BaseRegister + offset + reg, bytes[0] == 0x01);
                        commitValue = (reg, bytes) => throw new NotSupportedException("Writing coils is not currently supported.");
                        values.Add(new WriteableModelValue(point, readValue, writeValue, commitValue, _buffer, bufferOffset, modiconId));
                        break;
                    case (Table.HoldingRegisters, null):
                        commitValue = (register, bytes) => _client.WriteRegisters(Group.BaseRegister + offset + register, bytes);
                        values.Add(new WriteableModelValue(point, readValue, writeValue, commitValue, _buffer, bufferOffset, modiconId));
                        break;
                    default:
                        values.Add(new ModelValue(point, readValue, _buffer, bufferOffset, modiconId));
                        break;
                }
            }
            bufferOffset += size;
            // when incrementing the register for coils or discrete inputs, multiply the size in bytes by 4, because
            // coils/DIs are counted in bits, not registers (i.e. 16-bit words)
            if (group.Table == Table.Coils || group.Table == Table.DiscreteInputs)
            {
                register += point.SizeInBytes * 4;
            }
            else
            {
                register += point.SizeInBytes / 2;
            }
        }
        Values = values.AsReadOnly();
    }

    /// <summary>
    /// Gets the underlying schema object.
    /// </summary>
    public Group Group { get; init; }

    /// <summary>
    /// The list of values in this group.
    /// </summary>
    public IReadOnlyList<IModelValue> Values { get; }

    /// <summary>
    /// Reads all the registers in this group.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/>.</returns>
    public async ValueTask ReadAsync()
    {
        switch (Group.Table)
        {
            case Table.InputRegisters:
                await _client.ReadInputRegistersAsync(Group.BaseRegister + _offset, _buffer);
                break;
            case Table.HoldingRegisters:
                await _client.ReadHoldingRegistersAsync(Group.BaseRegister + _offset, _buffer);
                break;
            case Table.Coils:
                await _client.ReadCoilsAsync(Group.BaseRegister + _offset, _buffer);
                break;
            case Table.DiscreteInputs:
                await _client.ReadDiscreteInputsAsync(Group.BaseRegister + _offset, _buffer);
                break;
        }
    }
}
