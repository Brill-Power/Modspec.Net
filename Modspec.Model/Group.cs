/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Modspec.Model;

/// <summary>
/// Represents a continuous range of registers in a Modbus device.
/// </summary>
public class Group
{
    /// <summary>
    /// The name that identifies this group.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// The first register in the group.
    /// </summary>
    public required ushort BaseRegister { get; set; }
    /// <summary>
    /// The Modbus "table" in which the registers are located.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Table Table { get; set; }
    /// <summary>
    /// The list of registers in this group.
    /// </summary>
    public required List<Point> Points { get; set; } = [];
}
