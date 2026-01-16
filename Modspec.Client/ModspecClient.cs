/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modspec.Model;

namespace Modspec.Client;

public delegate object? ReadValue(Point point, ReadOnlySpan<byte> slice);
public delegate void WriteValue(Point point, object value, Span<byte> slice);
public delegate void CommitValue(int register, Memory<byte> bytes);

/// <summary>
/// Core Modspec client.
/// </summary>
public class ModspecClient : IDisposable
{
    private readonly IReadWriteModbusClient _client;
    private readonly Dictionary<int, IModelValue> _modelValuesByRegister = [];

    public ModspecClient(IReadWriteModbusClient client, bool isBigEndian, Schema schema)
    {
        _client = client;
        Schema = schema;

        ReadValue readValue;
        WriteValue writeValue;
        if (isBigEndian)
        {
            readValue = ReadBigEndian;
            writeValue = WriteBigEndian;
        }
        else
        {
            readValue = ReadLittleEndian;
            writeValue = WriteLittleEndian;
        }

        Groups = GetBoundGroups(client, 0, schema.Groups, readValue, writeValue, _modelValuesByRegister);
        RepeatingGroups = schema.RepeatingGroups.Select(rg => new BoundRepeatingGroup(client, rg, readValue, writeValue, _modelValuesByRegister)).ToList().AsReadOnly();
    }

    internal static IReadOnlyList<BoundGroup> GetBoundGroups(IReadWriteModbusClient client, ushort offset, IReadOnlyList<Group> groups, ReadValue readValue, WriteValue writeValue,
        Dictionary<int, IModelValue> modelValuesByRegister)
    {
        List<BoundGroup> result = [];
        foreach (Group group in groups)
        {
            ReadValue groupReadValue = readValue;
            WriteValue groupWriteValue = writeValue;
            if (group.Table == Table.Coils || group.Table == Table.DiscreteInputs)
            {
                groupReadValue = ReadLittleEndian;
                groupWriteValue = WriteLittleEndian;
            }
            BoundGroup boundGroup = new BoundGroup(client, offset, group, groupReadValue, groupWriteValue);
            result.Add(boundGroup);
            foreach (IModelValue modelValue in boundGroup.Values)
            {
                // Modicon gives us a unique ID per register, including table
                modelValuesByRegister.Add(modelValue.ModiconId, modelValue);
            }
        }
        return result.AsReadOnly();
    }

    /// <summary>
    /// Gets the schema from which the bound values (in <see cref="Groups"/>) are
    /// created.
    /// </summary>
    public Schema Schema { get; }

    /// <summary>
    /// Gets all the bound groups.
    /// </summary>
    public IReadOnlyList<BoundGroup> Groups { get; }

    /// <summary>
    /// Gets all the bound repeating groups.
    /// </summary>
    public IReadOnlyList<BoundRepeatingGroup> RepeatingGroups { get; }

    /// <summary>
    /// Tries to get the <see cref="IModelValue"/> instance representing the value at the
    /// given register.
    /// </summary>
    /// <param name="registerId">The register - in Modicon notation - whose value is sought.</param>
    /// <param name="modelValue">If the value was found, when the method returns, this parameter will
    /// contain the <see cref="IModelValue"/> instance representing the value sought.</param>
    /// <returns>True if the register was found, otherwise false.</returns>
    public bool TryGetValue(int registerId, [NotNullWhen(true)] out IModelValue? modelValue)
    {
        return _modelValuesByRegister.TryGetValue(registerId, out modelValue);
    }

    /// <summary>
    /// Reads all values in all groups.
    /// </summary>
    public async ValueTask ReadAllAsync()
    {
        foreach (BoundGroup group in Groups)
        {
            await group.ReadAsync();
        }
    }

    public void Dispose()
    {
        if (_client is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private static void WriteLittleEndian(Point point, object value, Span<byte> slice)
    {
        switch (point.Type)
        {
            case PointType.Enum16:
            case PointType.Bitfield16:
                BinaryPrimitives.WriteUInt16LittleEndian(slice, (ushort)value);
                break;
            case PointType.Enum32:
            case PointType.Bitfield32:
                BinaryPrimitives.WriteUInt32LittleEndian(slice, (uint)value);
                break;
            case PointType.Enum64:
            case PointType.Bitfield64:
                BinaryPrimitives.WriteUInt64LittleEndian(slice, (ulong)value);
                break;
            case PointType.String:
                string? s = value as string;
                if (s?.Length > point.Length)
                {
                    throw new ArgumentException($"Specified value is too long; maximum length is {point.Length}.");
                }
                Encoding.UTF8.GetBytes((string)value, slice);
                break;
            case PointType.UInt16:
            case PointType.Acc16:
                BinaryPrimitives.WriteUInt16LittleEndian(slice, (ushort)Descale(point, value));
                break;
            case PointType.UInt32:
            case PointType.Acc32:
                BinaryPrimitives.WriteUInt32LittleEndian(slice, (uint)Descale(point, value));
                break;
            case PointType.UInt64:
            case PointType.Acc64:
                BinaryPrimitives.WriteUInt64LittleEndian(slice, (ulong)Descale(point, value));
                break;
            case PointType.Int16:
                BinaryPrimitives.WriteInt16LittleEndian(slice, (short)Descale(point, value));
                break;
            case PointType.Int32:
                BinaryPrimitives.WriteInt32LittleEndian(slice, (int)Descale(point, value));
                break;
            case PointType.Int64:
                BinaryPrimitives.WriteInt64LittleEndian(slice, (long)Descale(point, value));
                break;
            case PointType.Float32:
                BinaryPrimitives.WriteSingleLittleEndian(slice, (float)Descale(point, value));
                break;
            case PointType.Float64:
                BinaryPrimitives.WriteDoubleLittleEndian(slice, Descale(point, value));
                break;
            default:
                throw new NotSupportedException($"Registers of type {point.Type} are not supported.");
        }
    }

    private static void WriteBigEndian(Point point, object value, Span<byte> slice)
    {
        switch (point.Type)
        {
            case PointType.Enum16:
            case PointType.Bitfield16:
                BinaryPrimitives.WriteUInt16BigEndian(slice, (ushort)value);
                break;
            case PointType.Enum32:
            case PointType.Bitfield32:
                BinaryPrimitives.WriteUInt32BigEndian(slice, (uint)value);
                break;
            case PointType.Enum64:
            case PointType.Bitfield64:
                BinaryPrimitives.WriteUInt64BigEndian(slice, (ulong)value);
                break;
            case PointType.String:
                string? s = value as string;
                if (s?.Length > point.Length)
                {
                    throw new ArgumentException($"Specified value is too long; maximum length is {point.Length}.");
                }
                Encoding.UTF8.GetBytes((string)value, slice);
                break;
            case PointType.UInt16:
            case PointType.Acc16:
                BinaryPrimitives.WriteUInt16BigEndian(slice, (ushort)Descale(point, value));
                break;
            case PointType.UInt32:
            case PointType.Acc32:
                BinaryPrimitives.WriteUInt32BigEndian(slice, (uint)Descale(point, value));
                break;
            case PointType.UInt64:
            case PointType.Acc64:
                BinaryPrimitives.WriteUInt64BigEndian(slice, (ulong)Descale(point, value));
                break;
            case PointType.Int16:
                BinaryPrimitives.WriteInt16BigEndian(slice, (short)Descale(point, value));
                break;
            case PointType.Int32:
                BinaryPrimitives.WriteInt32BigEndian(slice, (int)Descale(point, value));
                break;
            case PointType.Int64:
                BinaryPrimitives.WriteInt64BigEndian(slice, (long)Descale(point, value));
                break;
            case PointType.Float32:
                BinaryPrimitives.WriteSingleBigEndian(slice, (float)Descale(point, value));
                break;
            case PointType.Float64:
                BinaryPrimitives.WriteDoubleBigEndian(slice, Descale(point, value));
                break;
            default:
                throw new NotSupportedException($"Registers of type {point.Type} are not supported.");
        }
    }

    private static object? ReadLittleEndian(Point point, ReadOnlySpan<byte> slice)
    {
        switch (point.Type)
        {
            case PointType.Enum16:
            case PointType.Bitfield16:
                return BinaryPrimitives.ReadUInt16LittleEndian(slice);
            case PointType.Enum32:
            case PointType.Bitfield32:
                return BinaryPrimitives.ReadUInt32LittleEndian(slice);
            case PointType.Enum64:
            case PointType.Bitfield64:
                return BinaryPrimitives.ReadUInt64LittleEndian(slice);
            case PointType.String:
                return Encoding.UTF8.GetString(slice.Slice(0, point.Length ?? 0));
            case PointType.UInt16:
            case PointType.Acc16:
                return Scale(point, BinaryPrimitives.ReadUInt16LittleEndian(slice));
            case PointType.UInt32:
            case PointType.Acc32:
                return Scale(point, BinaryPrimitives.ReadUInt32LittleEndian(slice));
            case PointType.UInt64:
            case PointType.Acc64:
                return Scale(point, BinaryPrimitives.ReadUInt64LittleEndian(slice));
            case PointType.Int16:
                return Scale(point, BinaryPrimitives.ReadInt16LittleEndian(slice));
            case PointType.Int32:
                return Scale(point, BinaryPrimitives.ReadInt32LittleEndian(slice));
            case PointType.Int64:
                return Scale(point, BinaryPrimitives.ReadInt64LittleEndian(slice));
            case PointType.Float32:
                return Scale(point, BinaryPrimitives.ReadSingleLittleEndian(slice));
            case PointType.Float64:
                return Scale(point, BinaryPrimitives.ReadDoubleLittleEndian(slice));
            default:
                throw new NotSupportedException($"Registers of type {point.Type} are not supported.");
        }
    }

    private static object? ReadBigEndian(Point point, ReadOnlySpan<byte> slice)
    {
        switch (point.Type)
        {
            case PointType.Enum16:
            case PointType.Bitfield16:
                return BinaryPrimitives.ReadUInt16BigEndian(slice);
            case PointType.Enum32:
            case PointType.Bitfield32:
                return BinaryPrimitives.ReadUInt32BigEndian(slice);
            case PointType.Enum64:
            case PointType.Bitfield64:
                return BinaryPrimitives.ReadUInt64BigEndian(slice);
            case PointType.String:
                return Encoding.UTF8.GetString(slice.Slice(0, point.Length ?? 0));
            case PointType.UInt16:
            case PointType.Acc16:
                return Scale(point, BinaryPrimitives.ReadUInt16BigEndian(slice));
            case PointType.UInt32:
            case PointType.Acc32:
                return Scale(point, BinaryPrimitives.ReadUInt32BigEndian(slice));
            case PointType.UInt64:
            case PointType.Acc64:
                return Scale(point, BinaryPrimitives.ReadUInt64BigEndian(slice));
            case PointType.Int16:
                return Scale(point, BinaryPrimitives.ReadInt16BigEndian(slice));
            case PointType.Int32:
                return Scale(point, BinaryPrimitives.ReadInt32BigEndian(slice));
            case PointType.Int64:
                return Scale(point, BinaryPrimitives.ReadInt64BigEndian(slice));
            case PointType.Float32:
                return Scale(point, BinaryPrimitives.ReadSingleBigEndian(slice));
            case PointType.Float64:
                return Scale(point, BinaryPrimitives.ReadDoubleBigEndian(slice));
            default:
                throw new NotSupportedException($"Registers of type {point.Type} are not supported.");
        }
    }

    private static double Scale(Point point, double value)
    {
        return value * (point.ScaleFactor ?? 1) + (point.Offset ?? 0);
    }

    private static double Descale(Point point, object value)
    {
        double d = (double)Convert.ChangeType(value, typeof(double));
        return (d - (point.Offset ?? 0)) / (point.ScaleFactor ?? 1);
    }
}
