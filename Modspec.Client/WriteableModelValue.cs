/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using Modspec.Model;

namespace Modspec.Client;

public class WriteableModelValue : ModelValue
{
    private readonly WriteValue _writeValue;
    private readonly CommitValue _commitValue;
    private readonly Memory<byte> _buffer;

    internal WriteableModelValue(Point point, ReadValue readValue, WriteValue writeValue, CommitValue commitValue, Memory<byte> buffer, int offset, int modiconId) : base(point, readValue, buffer, offset, modiconId)
    {
        _writeValue = writeValue;
        _commitValue = commitValue;
        _buffer = buffer;
    }

    public override object? Value
    {
        get { return base.Value; }
        set
        {
            if (Point.Count is not null)
            {
                throw new NotSupportedException($"Writing arrays is not currently supported.");
            }
            if (value is null)
            {
                // TODO: may need some concept of null values
                return;
            }
            if (Point.MinValue.HasValue && Point.MinValue.Value.CompareTo(Convert.ToDouble(value)) > 0)
            {
                throw new ArgumentOutOfRangeException($"Specified value is below the minimum valid value ({Point.MinValue}).");
            }
            if (Point.MaxValue.HasValue && Point.MaxValue.Value.CompareTo(Convert.ToDouble(value)) < 0)
            {
                throw new ArgumentOutOfRangeException($"Specified value is above the maximum valid value ({Point.MaxValue}).");
            }
            byte[] bytes = new byte[Point.SizeInBytes];
            Span<byte> span = bytes;
            _writeValue(Point, value, span);
            _commitValue(_bufferOffset / 2, bytes);
            span.CopyTo(_buffer.Span.Slice(_bufferOffset));
        }
    }
}
