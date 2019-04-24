﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using StarkPlatform.CodeAnalysis.Host;

namespace StarkPlatform.CodeAnalysis.CodeFixes
{
    internal interface IFixMultipleOccurrencesService : IWorkspaceService
    {
        /// <summary>
        /// Get the fix multiple occurrences code fix for the given diagnostics with source locations.
        /// NOTE: This method does not apply the fix to the workspace.
        /// </summary>
        Solution GetFix(
            ImmutableDictionary<Document, ImmutableArray<Diagnostic>> diagnosticsToFix,
            Workspace workspace,
            CodeFixProvider fixProvider,
            FixAllProvider fixAllProvider,
            string equivalenceKey,
            string waitDialogTitle,
            string waitDialogMessage,
            CancellationToken cancellationToken);

        /// <summary>
        /// Get the fix multiple occurrences code fix for the given diagnostics with source locations.
        /// NOTE: This method does not apply the fix to the workspace.
        /// </summary>
        Solution GetFix(
            ImmutableDictionary<Project, ImmutableArray<Diagnostic>> diagnosticsToFix,
            Workspace workspace,
            CodeFixProvider fixProvider,
            FixAllProvider fixAllProvider,
            string equivalenceKey,
            string waitDialogTitle,
            string waitDialogMessage,
            CancellationToken cancellationToken);
    }
}
