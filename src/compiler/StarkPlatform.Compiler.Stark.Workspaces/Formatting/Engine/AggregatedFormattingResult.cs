﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StarkPlatform.Compiler.Stark.Symbols;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.Formatting;
using StarkPlatform.Compiler.Shared.Collections;
using StarkPlatform.Compiler.Text;
using Roslyn.Utilities;

namespace StarkPlatform.Compiler.Stark.Formatting
{
    internal class AggregatedFormattingResult : AbstractAggregatedFormattingResult
    {
        public AggregatedFormattingResult(SyntaxNode node, IList<AbstractFormattingResult> results, SimpleIntervalTree<TextSpan> formattingSpans) :
            base(node, results, formattingSpans)
        {
        }

        protected override SyntaxNode Rewriter(Dictionary<ValueTuple<SyntaxToken, SyntaxToken>, TriviaData> map, CancellationToken cancellationToken)
        {
            var rewriter = new TriviaRewriter(this.Node, GetFormattingSpans(), map, cancellationToken);
            return rewriter.Transform();
        }
    }
}
