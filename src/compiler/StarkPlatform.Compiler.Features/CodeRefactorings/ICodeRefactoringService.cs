﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.Text;

namespace StarkPlatform.Compiler.CodeRefactorings
{
    internal interface ICodeRefactoringService
    {
        Task<bool> HasRefactoringsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken);

        Task<ImmutableArray<CodeRefactoring>> GetRefactoringsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken);
    }
}
