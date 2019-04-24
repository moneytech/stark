﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using StarkPlatform.CodeAnalysis.Stark.Extensions.ContextQuery;
using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.Shared.Extensions;

namespace StarkPlatform.CodeAnalysis.Stark.Completion.KeywordRecommenders
{
    internal class EqualsKeywordRecommender : AbstractSyntacticSingleKeywordRecommender
    {
        public EqualsKeywordRecommender()
            : base(SyntaxKind.EqualsKeyword)
        {
        }

        protected override bool IsValidContext(int position, CSharpSyntaxContext context, CancellationToken cancellationToken)
        {
            // cases:
            //   join a in expr o1 |
            //   join a in expr o1 e|

            var token = context.TargetToken;

            var join = token.GetAncestor<JoinClauseSyntax>();
            if (join == null)
            {
                return false;
            }

            var lastToken = join.LeftExpression.GetLastToken(includeSkipped: true);

            // join a in expr |
            if (join.LeftExpression.Width() > 0 &&
                token == lastToken)
            {
                return true;
            }

            return false;
        }
    }
}
