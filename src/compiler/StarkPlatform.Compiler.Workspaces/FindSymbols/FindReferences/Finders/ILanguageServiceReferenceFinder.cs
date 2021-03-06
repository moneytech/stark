﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.Host;

namespace StarkPlatform.Compiler.FindSymbols.Finders
{
    /// <summary>
    /// Extensibility interface to allow individual languages to extend the 'Find References' service. 
    /// Languages can use this to provide specialized cascading logic between symbols that 'Find 
    /// References' is searching for.
    /// </summary>
    internal interface ILanguageServiceReferenceFinder : ILanguageService
    {
        Task<ImmutableArray<SymbolAndProjectId>> DetermineCascadedSymbolsAsync(
            SymbolAndProjectId symbolAndProjectId, Project project, CancellationToken cancellationToken);
    }
}
