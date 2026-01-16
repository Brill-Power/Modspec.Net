/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System.Text.Json.Serialization;

namespace Modspec.Model;

/// <summary>
/// Represents an entry in an enum or bitfield.
/// </summary>
public class Symbol
{
    /// <summary>
    /// The name of the entry.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// The value of the entry. For enums, this is the desired value;
    /// for bitfields, it is the bit to which the name corresponds.
    /// </summary>
    public required int Value { get; set; }
    /// <summary>
    /// Indicates the warning/error level that this entry represents, if any.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Level? Level { get; set; }
}
