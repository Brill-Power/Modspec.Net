/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
namespace Modspec.Model;

/// <summary>
/// Specifies the Modbus "table" in which a <see cref="Group"/>'s registers
/// are found.
/// </summary>
public enum Table
{
    Coils = 0x01,
    DiscreteInputs = 0x02,
    HoldingRegisters = 0x03,
    InputRegisters = 0x04,
}