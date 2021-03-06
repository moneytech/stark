﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.Completion;
using StarkPlatform.Compiler.Completion.Providers;
using StarkPlatform.Compiler.Stark.Extensions;
using StarkPlatform.Compiler.Stark.Extensions.ContextQuery;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.Options;
using StarkPlatform.Compiler.Shared.Extensions;
using StarkPlatform.Compiler.Shared.Extensions.ContextQuery;
using StarkPlatform.Compiler.Text;

namespace StarkPlatform.Compiler.Stark.Completion.Providers
{
    internal partial class ObjectCreationCompletionProvider : AbstractObjectCreationCompletionProvider
    {
        internal override bool IsInsertionTrigger(SourceText text, int characterPosition, OptionSet options)
        {
            return CompletionUtilities.IsTriggerAfterSpaceOrStartOfWordCharacter(text, characterPosition, options);
        }

        protected override SyntaxNode GetObjectCreationNewExpression(SyntaxTree tree, int position, CancellationToken cancellationToken)
        {
            if (tree != null)
            {
                if (!tree.IsInNonUserCode(position, cancellationToken))
                {
                    var tokenOnLeftOfPosition = tree.FindTokenOnLeftOfPosition(position, cancellationToken);
                    var newToken = tokenOnLeftOfPosition.GetPreviousTokenIfTouchingWord(position);

                    // Only after 'new'.
                    if (newToken.Kind() == SyntaxKind.NewKeyword)
                    {
                        // Only if the 'new' belongs to an object creation expression (and isn't a 'new'
                        // modifier on a member).
                        if (tree.IsObjectCreationTypeContext(position, tokenOnLeftOfPosition, cancellationToken))
                        {
                            return newToken.Parent as ExpressionSyntax;
                        }
                    }
                }
            }

            return null;
        }

        protected override async Task<SyntaxContext> CreateContext(Document document, int position, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelForSpanAsync(new TextSpan(position, 0), cancellationToken).ConfigureAwait(false);
            return CSharpSyntaxContext.CreateContext(document.Project.Solution.Workspace, semanticModel, position, cancellationToken);
        }

        protected override async Task<ImmutableArray<ISymbol>> GetPreselectedSymbolsWorker(SyntaxContext context, int position, OptionSet options, CancellationToken cancellationToken)
        {
            var result = await base.GetPreselectedSymbolsWorker(context, position, options, cancellationToken).ConfigureAwait(false);
            if (result.Any())
            {
                var type = (ITypeSymbol)result.Single();
                var alias = await type.FindApplicableAlias(position, context.SemanticModel, cancellationToken).ConfigureAwait(false);
                if (alias != null)
                {
                    return ImmutableArray.Create(alias);
                }
            }

            return result;
        }

        protected override (string displayText, string suffix, string insertionText) GetDisplayAndSuffixAndInsertionText(ISymbol symbol, SyntaxContext context)
        {
            if (symbol is IAliasSymbol)
            {
                return (symbol.Name, "", symbol.Name);
            }

            return base.GetDisplayAndSuffixAndInsertionText(symbol, context);
        }

        private static readonly CompletionItemRules s_arrayRules =
            CompletionItemRules.Create(
                commitCharacterRules: ImmutableArray.Create(CharacterSetModificationRule.Create(CharacterSetModificationKind.Replace, ' ', '(', '[')),
                matchPriority: MatchPriority.Default,
                selectionBehavior: CompletionItemSelectionBehavior.SoftSelection);

        private static readonly CompletionItemRules s_objectRules =
            CompletionItemRules.Create(
                commitCharacterRules: ImmutableArray.Create(CharacterSetModificationRule.Create(CharacterSetModificationKind.Replace, ' ', '(', '[')),
                matchPriority: MatchPriority.Preselect,
                selectionBehavior: CompletionItemSelectionBehavior.HardSelection);

        private static readonly CompletionItemRules s_defaultRules =
            CompletionItemRules.Create(
                commitCharacterRules: ImmutableArray.Create(CharacterSetModificationRule.Create(CharacterSetModificationKind.Replace, ' ', '(', '[', '{')),
                matchPriority: MatchPriority.Preselect,
                selectionBehavior: CompletionItemSelectionBehavior.HardSelection);

        protected override CompletionItemRules GetCompletionItemRules(IReadOnlyList<ISymbol> symbols, bool preselect)
        {
            if (!preselect)
            {
                return s_arrayRules;
            }

            // SPECIAL: If the preselected symbol is System.Object, don't commit on '{'.
            // Otherwise, it is cumbersome to type an anonymous object when the target type is object.
            // The user would get 'new object {' rather than 'new {'. Since object doesn't have any
            // properties, the user never really wants to commit 'new object {' anyway.
            var namedTypeSymbol = symbols.Count > 0 ? symbols[0] as INamedTypeSymbol : null;
            if (namedTypeSymbol?.SpecialType == SpecialType.System_Object)
            {
                return s_objectRules;
            }

            return s_defaultRules;
        }
    }
}
