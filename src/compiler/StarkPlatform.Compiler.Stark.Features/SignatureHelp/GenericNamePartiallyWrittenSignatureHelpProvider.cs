﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Threading;
using StarkPlatform.Compiler.Stark.Extensions;
using StarkPlatform.Compiler.LanguageServices;
using StarkPlatform.Compiler.SignatureHelp;
using StarkPlatform.Compiler.Text;
using Roslyn.Utilities;

namespace StarkPlatform.Compiler.Stark.SignatureHelp
{
    [ExportSignatureHelpProvider("GenericNamePartiallyWrittenSignatureHelpProvider", LanguageNames.Stark), Shared]
    internal class GenericNamePartiallyWrittenSignatureHelpProvider : GenericNameSignatureHelpProvider
    {
        protected override bool TryGetGenericIdentifier(SyntaxNode root, int position, ISyntaxFactsService syntaxFacts, SignatureHelpTriggerReason triggerReason, CancellationToken cancellationToken, out SyntaxToken genericIdentifier, out SyntaxToken lessThanToken)
        {
            return root.SyntaxTree.IsInPartiallyWrittenGeneric(position, cancellationToken, out genericIdentifier, out lessThanToken);
        }

        protected override TextSpan GetTextSpan(SyntaxToken genericIdentifier, SyntaxToken lessThanToken)
        {
            var lastToken = genericIdentifier.FindLastTokenOfPartialGenericName();
            var nextToken = lastToken.GetNextNonZeroWidthTokenOrEndOfFile();
            Contract.ThrowIfTrue(nextToken.Kind() == 0);
            return TextSpan.FromBounds(genericIdentifier.SpanStart, nextToken.SpanStart);
        }
    }
}
