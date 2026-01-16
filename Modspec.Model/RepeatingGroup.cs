/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System.Collections.Generic;

namespace Modspec.Model;

/// <summary>
/// Represents a repeating group.
/// </summary>
public class RepeatingGroup
{
    /// <summary>
    /// The name of a repeating group.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Specifies, in registers, how often a group repeats.
    /// </summary>
    public required ushort Every { get; set; }
    /// <summary>
    /// The number of elements in the group.
    /// </summary>
    public required Count Count { get; set; }
    /// <summary>
    /// The <see cref="Group"/> instances that specify the
    /// values.
    /// </summary>
    public required List<Group> Groups { get; set; } = [];
}