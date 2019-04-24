﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.CodeAnalysis.CodeActions;
using StarkPlatform.CodeAnalysis.CodeFixes;
using StarkPlatform.CodeAnalysis.Diagnostics;
using StarkPlatform.CodeAnalysis.Editing;
using StarkPlatform.CodeAnalysis.LanguageServices;
using StarkPlatform.CodeAnalysis.Shared.Extensions;

namespace StarkPlatform.CodeAnalysis.AddRequiredParentheses
{
    [ExportCodeFixProvider(LanguageNames.Stark), Shared]
    internal class AddRequiredParenthesesCodeFixProvider : SyntaxEditorBasedCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(IDEDiagnosticIds.AddRequiredParenthesesDiagnosticId);

        protected override bool IncludeDiagnosticDuringFixAll(FixAllState state, Diagnostic diagnostic, CancellationToken cancellationToken)
            => diagnostic.Properties.ContainsKey(AddRequiredParenthesesConstants.IncludeInFixAll) &&
               diagnostic.Properties[AddRequiredParenthesesConstants.EquivalenceKey] == state.CodeActionEquivalenceKey;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var firstDiagnostic = context.Diagnostics[0];
            context.RegisterCodeFix(
                new MyCodeAction(
                    c => FixAsync(context.Document, firstDiagnostic, c),
                    firstDiagnostic.Properties[AddRequiredParenthesesConstants.EquivalenceKey]),
                context.Diagnostics);
            return Task.CompletedTask;
        }

        protected override Task FixAllAsync(
            Document document, ImmutableArray<Diagnostic> diagnostics,
            SyntaxEditor editor, CancellationToken cancellationToken)
        {
            var syntaxFacts = document.GetLanguageService<ISyntaxFactsService>();

            foreach (var diagnostic in diagnostics)
            {
                var location = diagnostic.AdditionalLocations[0];
                var node = location.FindNode(findInsideTrivia: true, getInnermostNodeForTie: true, cancellationToken);

                // Do not add the simplifier annotation.  We do not want the simplifier undoing the 
                // work we just did.
                editor.ReplaceNode(node,
                    (current, _) => syntaxFacts.Parenthesize(
                        current, includeElasticTrivia: false, addSimplifierAnnotation: false));
            }

            return Task.CompletedTask;
        }

        private class MyCodeAction : CodeAction.DocumentChangeAction
        {
            public MyCodeAction(Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
                : base(FeaturesResources.Add_parentheses_for_clarity, createChangedDocument, equivalenceKey)
            {
            }
        }
    }
}
