﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.Completion;
using StarkPlatform.Compiler.Completion.Providers;
using StarkPlatform.Compiler.Stark.Extensions;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.ErrorReporting;
using StarkPlatform.Compiler.LanguageServices;
using StarkPlatform.Compiler.Options;
using StarkPlatform.Compiler.Shared.Extensions;
using StarkPlatform.Compiler.Text;

namespace StarkPlatform.Compiler.Stark.Completion.Providers
{
    internal partial class ExplicitInterfaceMemberCompletionProvider : CommonCompletionProvider
    {
        private const string InsertionTextOnOpenParen = nameof(InsertionTextOnOpenParen);

        private static readonly SymbolDisplayFormat s_signatureDisplayFormat =
            new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions:
                    SymbolDisplayMemberOptions.IncludeParameters,
                parameterOptions:
                    SymbolDisplayParameterOptions.IncludeName |
                    SymbolDisplayParameterOptions.IncludeType |
                    SymbolDisplayParameterOptions.IncludeParamsRefOut,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        internal override bool IsInsertionTrigger(SourceText text, int characterPosition, OptionSet options)
        {
            return text[characterPosition] == '.';
        }

        public override async Task ProvideCompletionsAsync(CompletionContext context)
        {
            try
            {
                var document = context.Document;
                var position = context.Position;
                var options = context.Options;
                var cancellationToken = context.CancellationToken;

                var span = new TextSpan(position, length: 0);
                var semanticModel = await document.GetSemanticModelForSpanAsync(span, cancellationToken).ConfigureAwait(false);
                var syntaxTree = semanticModel.SyntaxTree;

                var syntaxFacts = document.GetLanguageService<ISyntaxFactsService>();
                var semanticFacts = document.GetLanguageService<ISemanticFactsService>();

                if (syntaxFacts.IsInNonUserCode(syntaxTree, position, cancellationToken) ||
                    semanticFacts.IsPreProcessorDirectiveContext(semanticModel, position, cancellationToken))
                {
                    return;
                }

                if (!syntaxTree.IsRightOfDotOrArrowOrColonColon(position, cancellationToken))
                {
                    return;
                }

                var node = syntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken)
                                     .GetPreviousTokenIfTouchingWord(position)
                                     .Parent;

                if (node.Kind() != SyntaxKind.ExplicitInterfaceSpecifier)
                {
                    return;
                }

                // Bind the interface name which is to the left of the dot
                var name = ((ExplicitInterfaceSpecifierSyntax)node).Name;

                var symbol = semanticModel.GetSymbolInfo(name, cancellationToken).Symbol as ITypeSymbol;
                if (symbol?.TypeKind != TypeKind.Interface)
                {
                    return;
                }

                var members = symbol.GetMembers();

                // We're going to create a entry for each one, including the signature
                var namePosition = name.SpanStart;

                var text = await syntaxTree.GetTextAsync(cancellationToken).ConfigureAwait(false);

                foreach (var member in members)
                {
                    if (member.IsAccessor())
                    {
                        continue;
                    }

                    var displayText = member.ToMinimalDisplayString(
                        semanticModel, namePosition, s_signatureDisplayFormat);
                    var insertionText = displayText;

                    var item = SymbolCompletionItem.CreateWithSymbolId(
                        displayText,
                        displayTextSuffix: "",
                        insertionText: insertionText,
                        symbols: ImmutableArray.Create(member),
                        contextPosition: position,
                        rules: CompletionItemRules.Default);
                    item = item.AddProperty(InsertionTextOnOpenParen, member.Name);

                    context.AddItem(item);
                }
            }
            catch (Exception e) when (FatalError.ReportWithoutCrashUnlessCanceled(e))
            {
                // nop
            }
        }

        protected override Task<CompletionDescription> GetDescriptionWorkerAsync(Document document, CompletionItem item, CancellationToken cancellationToken)
            => SymbolCompletionItem.GetDescriptionAsync(item, document, cancellationToken);

        public override Task<TextChange?> GetTextChangeAsync(
            Document document, CompletionItem selectedItem, char? ch, CancellationToken cancellationToken)
        {
            if (ch == '(')
            {
                if (selectedItem.Properties.TryGetValue(InsertionTextOnOpenParen, out var insertionText))
                {
                    return Task.FromResult<TextChange?>(new TextChange(selectedItem.Span, insertionText));
                }
            }

            return Task.FromResult<TextChange?>(new TextChange(selectedItem.Span, selectedItem.DisplayText));
        }
    }
}
