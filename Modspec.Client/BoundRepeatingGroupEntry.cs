/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System.Collections.Generic;
using System.Threading.Tasks;
using Modspec.Model;

namespace Modspec.Client;

/// <summary>
/// Represents an entry in a repeating group.
/// </summary>
public class BoundRepeatingGroupEntry
{
    public BoundRepeatingGroupEntry(IReadWriteModbusClient client, int index, ushort offset, IReadOnlyList<Group> groups, ReadValue readValue, WriteValue writeValue, Dictionary<int, IModelValue> modelValuesByRegister)
    {
        Index = index;
        Groups = ModspecClient.GetBoundGroups(client, offset, groups, readValue, writeValue, modelValuesByRegister);
    }

    /// <summary>
    /// The index of the entry in the repeating group.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the bound groups for this entry.
    /// </summary>
    public IReadOnlyList<BoundGroup> Groups { get; }

    /// <summary>
    /// Reads the values of all the <see cref="Group"/> instances in this entry.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/>.</returns>
    public async ValueTask ReadAllAsync()
    {
        foreach (BoundGroup group in Groups)
        {
            await group.ReadAsync();
        }
    }
}
