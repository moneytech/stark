﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using StarkPlatform.Compiler.Options;

namespace StarkPlatform.Compiler.GenerateEqualsAndGetHashCodeFromMembers
{
    internal static class GenerateEqualsAndGetHashCodeFromMembersOptions
    {
        public static readonly PerLanguageOption<bool> GenerateOperators = new PerLanguageOption<bool>(
            nameof(GenerateEqualsAndGetHashCodeFromMembersOptions),
            nameof(GenerateOperators), defaultValue: false,
            storageLocations: new RoamingProfileStorageLocation(
                $"TextEditor.%LANGUAGE%.Specific.{nameof(GenerateEqualsAndGetHashCodeFromMembersOptions)}.{nameof(GenerateOperators)}"));

        public static readonly PerLanguageOption<bool> ImplementIEquatable = new PerLanguageOption<bool>(
            nameof(GenerateEqualsAndGetHashCodeFromMembersOptions),
            nameof(ImplementIEquatable), defaultValue: false,
            storageLocations: new RoamingProfileStorageLocation(
                $"TextEditor.%LANGUAGE%.Specific.{nameof(GenerateEqualsAndGetHashCodeFromMembersOptions)}.{nameof(ImplementIEquatable)}"));
    }
}
