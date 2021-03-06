﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StarkPlatform.Compiler.Stark.Symbols;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.DocumentationComments;
using StarkPlatform.Compiler.LanguageServices;
using StarkPlatform.Compiler.Shared.Extensions;
using StarkPlatform.Compiler.SignatureHelp;
using Roslyn.Utilities;

namespace StarkPlatform.Compiler.Stark.SignatureHelp
{
    internal partial class ObjectCreationExpressionSignatureHelpProvider
    {
        private (IList<SignatureHelpItem> items, int? selectedItem) GetNormalTypeConstructors(
            Document document,
            ObjectCreationExpressionSyntax objectCreationExpression,
            SemanticModel semanticModel,
            ISymbolDisplayService symbolDisplayService,
            IAnonymousTypeDisplayService anonymousTypeDisplayService,
            IDocumentationCommentFormattingService documentationCommentFormattingService,
            INamedTypeSymbol normalType,
            ISymbol within,
            CancellationToken cancellationToken)
        {
            var accessibleConstructors = normalType.InstanceConstructors
                                                   .WhereAsArray(c => c.IsAccessibleWithin(within))
                                                   .WhereAsArray(s => s.IsEditorBrowsable(document.ShouldHideAdvancedMembers(), semanticModel.Compilation))
                                                   .Sort(symbolDisplayService, semanticModel, objectCreationExpression.SpanStart);

            var symbolInfo = semanticModel.GetSymbolInfo(objectCreationExpression, cancellationToken);
            var selectedItem = TryGetSelectedIndex(accessibleConstructors, symbolInfo);

            var items = accessibleConstructors.SelectAsArray(c =>
                ConvertNormalTypeConstructor(c, objectCreationExpression, semanticModel, symbolDisplayService, anonymousTypeDisplayService, documentationCommentFormattingService, cancellationToken));

            return (items, selectedItem);
        }

        private SignatureHelpItem ConvertNormalTypeConstructor(
            IMethodSymbol constructor,
            ObjectCreationExpressionSyntax objectCreationExpression,
            SemanticModel semanticModel,
            ISymbolDisplayService symbolDisplayService,
            IAnonymousTypeDisplayService anonymousTypeDisplayService,
            IDocumentationCommentFormattingService documentationCommentFormattingService,
            CancellationToken cancellationToken)
        {
            var position = objectCreationExpression.SpanStart;
            var item = CreateItem(
                constructor, semanticModel, position,
                symbolDisplayService, anonymousTypeDisplayService,
                constructor.IsParams(),
                constructor.GetDocumentationPartsFactory(semanticModel, position, documentationCommentFormattingService),
                GetNormalTypePreambleParts(constructor, semanticModel, position),
                GetSeparatorParts(),
                GetNormalTypePostambleParts(constructor),
                constructor.Parameters.Select(p => Convert(p, semanticModel, position, documentationCommentFormattingService, cancellationToken)).ToList());

            return item;
        }

        private IList<SymbolDisplayPart> GetNormalTypePreambleParts(
            IMethodSymbol method,
            SemanticModel semanticModel,
            int position)
        {
            var result = new List<SymbolDisplayPart>();

            result.AddRange(method.ContainingType.ToMinimalDisplayParts(semanticModel, position));
            result.Add(Punctuation(SyntaxKind.OpenParenToken));

            return result;
        }

        private IList<SymbolDisplayPart> GetNormalTypePostambleParts(IMethodSymbol method)
        {
            return SpecializedCollections.SingletonList(
                Punctuation(SyntaxKind.CloseParenToken));
        }
    }
}
