/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
namespace Modspec.Model.Extensions;

public static class PointTypeExtensions
{
    public static bool IsEnum(this PointType self)
    {
        return self == PointType.Enum16 || self == PointType.Enum32 || self == PointType.Enum64;
    }

    public static bool IsBitfield(this PointType self)
    {
        return self == PointType.Bitfield16 || self == PointType.Bitfield32 || self == PointType.Bitfield64;
    }

    public static bool IsEnumOrBitfield(this PointType self)
    {
        return self.IsEnum() || self.IsBitfield();
    }
}