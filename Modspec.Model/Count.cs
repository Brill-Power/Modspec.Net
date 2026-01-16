/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
namespace Modspec.Model;

/// <summary>
/// Specifies the name and maximum number of elements in an
/// array or repeating group.
/// </summary>
public class Count
{
    /// <summary>
    /// The name that identifies this instance.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// The maximum number of elements.
    /// </summary>
    public required ushort MaxValue { get; set; }
}