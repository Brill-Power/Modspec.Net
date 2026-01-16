/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
namespace Modspec.Model;

/// <summary>
/// Specifies the type of a value.
/// </summary>
public enum PointType
{
    Int16,
    Int32,
    Int64,
    UInt16,
    UInt32,
    UInt64,
    Acc16,
    Acc32,
    Acc64,
    Bitfield16,
    Bitfield32,
    Bitfield64,
    Enum16,
    Enum32,
    Enum64,
    Float32,
    Float64,
    String,
    Padding
}
