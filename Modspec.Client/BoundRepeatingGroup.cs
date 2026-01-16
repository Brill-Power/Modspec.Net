/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System.Collections.Generic;
using Modspec.Model;

namespace Modspec.Client;

/// <summary>
/// Represents a repeating group on a connected Modbus device.
/// </summary>
public class BoundRepeatingGroup
{
    public BoundRepeatingGroup(IReadWriteModbusClient client, RepeatingGroup repeatingGroup, ReadValue readValue, WriteValue writeValue, Dictionary<int, IModelValue> modelValuesByRegister)
    {
        RepeatingGroup = repeatingGroup;

        List<BoundRepeatingGroupEntry> entries = [];
        for (int i = 0; i < RepeatingGroup.Count.MaxValue; i++)
        {
            ushort offset = (ushort)(repeatingGroup.Every * i);
            entries.Add(new BoundRepeatingGroupEntry(client, i, offset, repeatingGroup.Groups, readValue, writeValue, modelValuesByRegister));
        }
        Entries = entries.AsReadOnly();
    }

    /// <summary>
    /// Gets the <see cref="RepeatingGroup"/> schema object
    /// that describes this repeating group.
    /// </summary>
    public RepeatingGroup RepeatingGroup { get; init; }

    /// <summary>
    /// Gets the elements in this repeating group.
    /// </summary>
    public IReadOnlyList<BoundRepeatingGroupEntry> Entries { get; }
}
