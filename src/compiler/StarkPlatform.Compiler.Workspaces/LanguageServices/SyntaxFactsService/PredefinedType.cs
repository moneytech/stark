﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace StarkPlatform.Compiler.LanguageServices
{
    internal enum PredefinedType
    {
        None = 0,
        Boolean = 1,
        UInt8 = 1 << 1,
        Rune = 1 << 2,
        Decimal = 1 << 4,
        Float64 = 1 << 5,
        Int16 = 1 << 6,
        Int32 = 1 << 7,
        Int64 = 1 << 8,
        Object = 1 << 9,
        Int8 = 1 << 10,
        Float32 = 1 << 11,
        String = 1 << 12,
        UInt16 = 1 << 13,
        UInt32 = 1 << 14,
        UInt64 = 1 << 15,
        Void = 1 << 16,
        Int = 1 << 17,
        UInt = 1 << 18,
    }
}
