/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections.Generic;
using Modspec.Model;
using Modspec.Model.Extensions;

namespace Modspec.Client;

public class ModelValue : IModelValue
{
    private readonly ReadValue _readValue;
    private readonly ReadOnlyMemory<byte> _buffer;
    protected readonly int _bufferOffset;

    internal ModelValue(Point point, ReadValue readValue, ReadOnlyMemory<byte> buffer, int bufferOffset, int modiconId)
    {
        Point = point;
        ModiconId = modiconId;
        _readValue = readValue;
        _buffer = buffer;
        _bufferOffset = bufferOffset;
    }

    public Point Point { get; }

    public int ModiconId { get; }

    public virtual object? Value
    {
        get
        {
            ReadOnlySpan<byte> slice = _buffer.Span.Slice(_bufferOffset);
            if (Point.Count is not null)
            {
                Type clrType = GetClrType();
                Array array = Array.CreateInstance(clrType, Point.Count.MaxValue);
                int i = 0;
                do
                {
                    array.SetValue(Convert.ChangeType(_readValue(Point, slice), clrType), i++);
                    slice = slice.Slice(Point.SizeInBytes);
                }
                while (i < Point.Count.MaxValue);
                return array;
            }
            object? value = _readValue(Point, slice);
            if (Point.Type.IsEnumOrBitfield() && value is not null && Point.Symbols is not null)
            {
                if (Point.Type.IsEnum())
                {
                    foreach (Symbol symbol in Point.Symbols)
                    {
                        if (symbol.Value.Equals(value))
                        {
                            return symbol.Name;
                        }
                    }
                }
                if (Point.Type.IsBitfield())
                {
                    List<string> values = [];
                    ulong t = (ulong)Convert.ChangeType(value, TypeCode.UInt64);
                    foreach (Symbol symbol in Point.Symbols)
                    {
                        if ((t & (ulong)(1 << symbol.Value)) == (ulong)(1 << symbol.Value))
                        {
                            values.Add(symbol.Name);
                        }
                    }
                    return values.ToArray();
                }
            }
            return value;
        }
        set
        {
            throw new InvalidOperationException($"Cannot set value of read-only register {Point.Name}.");
        }
    }

    private Type GetClrType()
    {
        if ((Point.ScaleFactor ?? 1) != 1 || (Point.Offset ?? 0) != 0) // TODO: consider refining; what if offset is an integer?
        {
            return typeof(double);
        }

        switch (Point.Type)
        {
            case PointType.Enum16:
            case PointType.Bitfield16:
            case PointType.UInt16:
            case PointType.Acc16:
                return typeof(ushort);
            case PointType.Enum32:
            case PointType.Bitfield32:
            case PointType.UInt32:
            case PointType.Acc32:
                return typeof(uint);
            case PointType.Enum64:
            case PointType.Bitfield64:
            case PointType.UInt64:
            case PointType.Acc64:
                return typeof(ulong);
            case PointType.String:
                return typeof(string);
            case PointType.Int16:
                return typeof(short);
            case PointType.Int32:
                return typeof(int);
            case PointType.Int64:
                return typeof(long);
            case PointType.Float32:
                return typeof(float);
            case PointType.Float64:
                return typeof(double);
            default:
                throw new NotSupportedException($"Registers of type {Point.Type} are not supported.");
        }
    }
}