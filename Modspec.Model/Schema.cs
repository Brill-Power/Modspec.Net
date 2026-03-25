/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Modspec.Model;

/// <summary>
/// Root-level class for a Modspec schema.
/// </summary>
public class Schema
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.Preserve,
    };

    /// <summary>
    /// Name of the schema. Will be used as the namespace.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// A list of <see cref="Count"/> instances, which specify the number of elements in repeating
    /// groups and arrays. These are then referred to using JSON references.
    /// </summary>
    public List<Count> Counts { get; set; } = [];

    /// <summary>
    /// A list of <see cref="Group"/> instances, which specify contiguous ranges of registers.
    /// </summary>
    public required List<Group> Groups { get; set; } = [];

    /// <summary>
    /// A list of <see cref="RepeatingGroup"/> instances, which specify collections of groups
    /// whose elements are repeated at a fixed interval.
    /// </summary>
    public List<RepeatingGroup> RepeatingGroups { get; set; } = [];

    public void Serialise(Stream stream)
    {
        JsonSerializer.Serialize(stream, this, Options);
    }

    public static Schema GetSchema(Stream stream)
    {
        return JsonSerializer.Deserialize<Schema>(stream, Options) ?? throw new InvalidDataException($"Unable to deserialise schema from stream.");
    }

    public static bool TryGetSchema(Stream stream, [NotNullWhen(true)] out Schema? schema)
    {
        try
        {
            schema = JsonSerializer.Deserialize<Schema>(stream, Options);
            return schema is not null;
        }
        catch (Exception)
        {
            schema = default;
            return false;
        }
    }
}
