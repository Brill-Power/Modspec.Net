/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using Modspec.Model;

namespace Modspec.Client;

/// <summary>
/// Encapsulates the value of one or more registers on a
/// connected Modbus device.
/// </summary>
public interface IModelValue
{
    /// <summary>
    /// The Modicon identifier for the register. This uniquely identifies
    /// the register on this device.
    /// </summary>
    public int ModiconId { get; }
    /// <summary>
    /// The <see cref="Point"/> schema object that contains metadata
    /// describing the value.
    /// </summary>
    public Point Point { get; }
    /// <summary>
    /// The value of the register, if it has been read.
    /// </summary>
    public object? Value { get; set; }
}
