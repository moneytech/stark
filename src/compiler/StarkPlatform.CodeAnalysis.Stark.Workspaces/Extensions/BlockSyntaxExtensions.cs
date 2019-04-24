﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using StarkPlatform.CodeAnalysis.CodeStyle;
using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.Shared.Extensions;
using Roslyn.Utilities;

namespace StarkPlatform.CodeAnalysis.Stark.Extensions
{
    internal static class BlockSyntaxExtensions
    {
        public static bool TryConvertToExpressionBody(
            this BlockSyntax block, SyntaxKind declarationKind,
            ParseOptions options, ExpressionBodyPreference preference,
            out ExpressionSyntax expression,
            out SyntaxToken eosToken)
        {
            if (preference != ExpressionBodyPreference.Never &&
                block != null && block.Statements.Count == 1)
            {
                var version = ((CSharpParseOptions)options).LanguageVersion;
                var acceptableVersion =
                    version >= LanguageVersion.CSharp7 ||
                    (version >= LanguageVersion.CSharp6 && IsSupportedInCSharp6(declarationKind));

                if (acceptableVersion)
                {
                    var firstStatement = block.Statements[0];

                    if (TryGetExpression(version, firstStatement, out expression, out eosToken) &&
                        MatchesPreference(expression, preference))
                    {
                        // The close brace of the block may have important trivia on it (like 
                        // comments or directives).  Preserve them on the semicolon when we
                        // convert to an expression body.
                        eosToken = eosToken.WithAppendedTrailingTrivia(
                            block.CloseBraceToken.LeadingTrivia.Where(t => !t.IsWhitespaceOrEndOfLine()));
                        return true;
                    }
                }
            }

            expression = null;
            eosToken = default;
            return false;
        }

        public static bool TryConvertToArrowExpressionBody(
            this BlockSyntax block, SyntaxKind declarationKind,
            ParseOptions options, ExpressionBodyPreference preference,
            out ArrowExpressionClauseSyntax arrowExpression,
            out SyntaxToken eosToken)
        {
            if (!block.TryConvertToExpressionBody(
                    declarationKind, options, preference,
                    out var expression, out eosToken))
            {
                arrowExpression = default;
                return false;
            }

            arrowExpression = SyntaxFactory.ArrowExpressionClause(expression);
            return true;
        }

        private static bool IsSupportedInCSharp6(SyntaxKind declarationKind)
        {
            switch (declarationKind)
            {
                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.DestructorDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                    return false;
            }

            return true;
        }

        public static bool MatchesPreference(
            ExpressionSyntax expression, ExpressionBodyPreference preference)
        {
            if (preference == ExpressionBodyPreference.WhenPossible)
            {
                return true;
            }

            Contract.ThrowIfFalse(preference == ExpressionBodyPreference.WhenOnSingleLine);
            return CSharpSyntaxFactsService.Instance.IsOnSingleLine(expression, fullSpan: false);
        }

        private static bool TryGetExpression(
            LanguageVersion version, StatementSyntax firstStatement,
            out ExpressionSyntax expression, out SyntaxToken eosToken)
        {
            if (firstStatement is ExpressionStatementSyntax exprStatement)
            {
                expression = exprStatement.Expression;
                eosToken = exprStatement.EosToken;
                return true;
            }
            else if (firstStatement is ReturnStatementSyntax returnStatement)
            {
                if (returnStatement.Expression != null)
                {
                    // If there are any comments or directives on the return keyword, move them to
                    // the expression.
                    expression = firstStatement.GetLeadingTrivia().Any(t => t.IsDirective || t.IsSingleOrMultiLineComment())
                        ? returnStatement.Expression.WithLeadingTrivia(returnStatement.GetLeadingTrivia())
                        : returnStatement.Expression;
                    eosToken = returnStatement.EosToken;
                    return true;
                }
            }
            else if (firstStatement is ThrowStatementSyntax throwStatement)
            {
                if (version >= LanguageVersion.CSharp7 && throwStatement.Expression != null)
                {
                    expression = SyntaxFactory.ThrowExpression(throwStatement.ThrowKeyword, throwStatement.Expression);
                    eosToken = throwStatement.EosToken;
                    return true;
                }
            }

            expression = null;
            eosToken = default;
            return false;
        }
    }
}
