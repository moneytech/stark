﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.FindSymbols.Finders;
using StarkPlatform.Compiler.Internal.Log;
using StarkPlatform.Compiler.Remote;

namespace StarkPlatform.Compiler.FindSymbols
{
    public static partial class SymbolFinder
    {
        internal static async Task<ImmutableArray<ReferencedSymbol>> FindRenamableReferencesAsync(
            SymbolAndProjectId symbolAndProjectId,
            Solution solution,
            CancellationToken cancellationToken)
        {
            using (Logger.LogBlock(FunctionId.FindReference_Rename, cancellationToken))
            {
                var streamingProgress = new StreamingProgressCollector(
                    StreamingFindReferencesProgress.Instance);

                IImmutableSet<Document> documents = null;
                var engine = new FindReferencesSearchEngine(
                    solution,
                    documents,
                    ReferenceFinders.DefaultRenameReferenceFinders,
                    streamingProgress,
                    FindReferencesSearchOptions.Default,
                    cancellationToken);

                await engine.FindReferencesAsync(symbolAndProjectId).ConfigureAwait(false);
                return streamingProgress.GetReferencedSymbols();
            }
        }
    }
}
