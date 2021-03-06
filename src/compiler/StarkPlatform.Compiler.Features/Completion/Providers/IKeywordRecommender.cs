﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StarkPlatform.Compiler.Completion.Providers
{
    internal interface IKeywordRecommender<TContext>
    {
        Task<IEnumerable<RecommendedKeyword>> RecommendKeywordsAsync(int position, TContext context, CancellationToken cancellationToken);
    }
}
