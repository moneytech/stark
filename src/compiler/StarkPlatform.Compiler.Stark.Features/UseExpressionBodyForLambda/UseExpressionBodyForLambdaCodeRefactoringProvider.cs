﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.CodeActions;
using StarkPlatform.Compiler.CodeRefactorings;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.Formatting;
using StarkPlatform.Compiler.Options;

namespace StarkPlatform.Compiler.Stark.UseExpressionBodyForLambda
{
    using static UseExpressionBodyForLambdaHelpers;

    [ExportCodeRefactoringProvider(LanguageNames.Stark,
        Name = PredefinedCodeRefactoringProviderNames.UseExpressionBody), Shared]
    internal sealed class UseExpressionBodyForLambdaCodeRefactoringProvider : CodeRefactoringProvider
    {
        public UseExpressionBodyForLambdaCodeRefactoringProvider()
        {
        }

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (context.Span.Length > 0)
            {
                return;
            }

            var position = context.Span.Start;
            var document = context.Document;
            var cancellationToken = context.CancellationToken;

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var lambdaNode = root.FindToken(position).Parent.FirstAncestorOrSelf<LambdaExpressionSyntax>();
            if (lambdaNode == null)
            {
                return;
            }

            var optionSet = await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false);

            if (CanOfferUseExpressionBody(optionSet, lambdaNode, forAnalyzer: false))
            {
                context.RegisterRefactoring(new MyCodeAction(
                    UseExpressionBodyTitle.ToString(),
                    c => UpdateDocumentAsync(
                        document, root, lambdaNode,
                        useExpressionBody: true, c)));
            }

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var (canOffer, _) = CanOfferUseBlockBody(
                semanticModel, optionSet, lambdaNode, forAnalyzer: false, cancellationToken);
            if (canOffer)
            {
                context.RegisterRefactoring(new MyCodeAction(
                    UseBlockBodyTitle.ToString(),
                    c => UpdateDocumentAsync(
                        document, root, lambdaNode,
                        useExpressionBody: false, c)));
            }
        }

        private async Task<Document> UpdateDocumentAsync(
            Document document, SyntaxNode root, LambdaExpressionSyntax declaration,
            bool useExpressionBody, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            // We're only replacing a single declaration in the refactoring.  So pass 'declaration'
            // as both the 'original' and 'current' declaration.
            var updatedDeclaration = Update(semanticModel, useExpressionBody, declaration, declaration);

            var newRoot = root.ReplaceNode(declaration, updatedDeclaration);
            return document.WithSyntaxRoot(newRoot);
        }

        private class MyCodeAction : CodeAction.DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument)
            {
            }
        }
    }
}
