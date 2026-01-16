/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modspec.Model;

public delegate T ReadValue<T>(ReadOnlySpan<byte> span);

/// <summary>
/// Provides support for arrays in strongly-typed generated models.
/// </summary>
/// <typeparam name="TFrom">The raw type of the value.</typeparam>
/// <typeparam name="TTo">The converted type of the value, after casting or scaling.</typeparam>
public readonly struct ModbusArray<TFrom, TTo> : IReadOnlyList<TTo>
{
    private readonly ReadOnlyMemory<byte> _buffer;
    private readonly int _size;
    private readonly ReadValue<TFrom> _valueReader;
    private readonly Func<TFrom, TTo> _transformer;

    public ModbusArray(ReadOnlyMemory<byte> buffer, int size, ReadValue<TFrom> valueReader, Func<TFrom, TTo> transformer)
    {
        _buffer = buffer;
        _size = size;
        _valueReader = valueReader;
        _transformer = transformer;
    }

    /// <inheritdoc/>
    public TTo this[int index]
    {
        get
        {
            if (index >= Count)
            {
                throw new IndexOutOfRangeException();
            }
            return _transformer(_valueReader(_buffer.Span.Slice(index * _size, _size)));
        }
    }

    /// <inheritdoc/>
    public int Count => _buffer.Length / _size;

    public IEnumerator<TTo> GetEnumerator()
    {
        return Enumerate().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private IEnumerable<TTo> Enumerate()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return this[i];
        }
    }
}
