/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Modspec.Model;

/// <summary>
/// Describes a value spanning one or more registers in a Modbus
/// device.
/// </summary>
public class Point
{
    /// <summary>
    /// The name that identifies the value.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// A label that might be used for display purposes.
    /// </summary>
    public string? Label { get; set; }
    /// <summary>
    /// A longer description of the value.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// The type of the value.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required PointType Type { get; set; }
    /// <summary>
    /// The length of the value, only applicable for values of type
    /// <see cref="PointType.String"/> and <see cref="PointType.Padding"/>.
    /// For strings, this is in characters; for padding, this is in
    /// registers.
    /// </summary>
    public ushort? Length { get; set; }
    /// <summary>
    /// For arrays, specifies the number of elements in the array.
    /// </summary>
    public Count? Count { get; set; }
    /// <summary>
    /// Specifies a factor by which the value should be multiplied.
    /// </summary>
    public double? ScaleFactor { get; set; }
    /// <summary>
    /// Specifies an offset which should be added to the value.
    /// </summary>
    public double? Offset { get; set; }
    /// <summary>
    /// Specifies a minimum value.
    /// </summary>
    public double? MinValue { get; set; }
    /// <summary>
    /// Specifies a maximum value.
    /// </summary>
    public double? MaxValue { get; set; }
    /// <summary>
    /// For enums and bitfields, specifies the members of the associated
    /// enumeration.
    /// </summary>
    public List<Symbol>? Symbols { get; set; }

    /// <summary>
    /// Gets the size in bytes of this value.
    /// </summary>
    public int SizeInBytes
    {
        get
        {
            switch (Type)
            {
                case PointType.Acc16:
                case PointType.Bitfield16:
                case PointType.Enum16:
                case PointType.Int16:
                case PointType.UInt16:
                    return 2;
                case PointType.Acc32:
                case PointType.Bitfield32:
                case PointType.Enum32:
                case PointType.Float32:
                case PointType.Int32:
                case PointType.UInt32:
                    return 4;
                case PointType.Acc64:
                case PointType.Bitfield64:
                case PointType.Enum64:
                case PointType.Float64:
                case PointType.Int64:
                case PointType.UInt64:
                    return 8;
                case PointType.String:
                    return Length ?? 0;
                case PointType.Padding:
                    return (Length ?? 1) * 2;
            }
            throw new NotSupportedException($"Values of type {Type} are not supported.");
        }
    }
}
