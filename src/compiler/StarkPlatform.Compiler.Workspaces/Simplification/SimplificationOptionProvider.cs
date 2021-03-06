﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using StarkPlatform.Compiler.Options;
using StarkPlatform.Compiler.Options.Providers;

namespace StarkPlatform.Compiler.Simplification
{
    [ExportOptionProvider, Shared]
    internal class SimplificationOptionsProvider : IOptionProvider
    {
        public ImmutableArray<IOption> Options { get; } = ImmutableArray.Create<IOption>(
            SimplificationOptions.PreferAliasToQualification,
                SimplificationOptions.PreferOmittingModuleNamesInQualification,
                SimplificationOptions.PreferImplicitTypeInference,
                SimplificationOptions.PreferImplicitTypeInLocalDeclaration,
                SimplificationOptions.AllowSimplificationToGenericType,
                SimplificationOptions.AllowSimplificationToBaseType,
                SimplificationOptions.NamingPreferences);
    }
}
