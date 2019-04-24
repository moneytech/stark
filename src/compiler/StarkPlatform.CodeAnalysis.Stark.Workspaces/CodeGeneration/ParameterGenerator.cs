﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using StarkPlatform.CodeAnalysis.CodeGeneration;
using StarkPlatform.CodeAnalysis.Stark.Extensions;
using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.Shared.Extensions;

using StarkPlatform.CodeAnalysis.PooledObjects;
using static StarkPlatform.CodeAnalysis.CodeGeneration.CodeGenerationHelpers;

namespace StarkPlatform.CodeAnalysis.Stark.CodeGeneration
{
    internal static class ParameterGenerator
    {
        public static ParameterListSyntax GenerateParameterList(
            ImmutableArray<IParameterSymbol> parameterDefinitions,
            bool isExplicit,
            CodeGenerationOptions options)
        {
            return GenerateParameterList((IEnumerable<IParameterSymbol>)parameterDefinitions, isExplicit, options);
        }

        public static ParameterListSyntax GenerateParameterList(
            IEnumerable<IParameterSymbol> parameterDefinitions,
            bool isExplicit,
            CodeGenerationOptions options)
        {
            var parameters = GetParameters(parameterDefinitions, isExplicit, options);

            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));
        }

        public static BracketedParameterListSyntax GenerateBracketedParameterList(
            ImmutableArray<IParameterSymbol> parameterDefinitions,
            bool isExplicit,
            CodeGenerationOptions options)
        {
            return GenerateBracketedParameterList((IList<IParameterSymbol>)parameterDefinitions, isExplicit, options);
        }

        public static BracketedParameterListSyntax GenerateBracketedParameterList(
            IEnumerable<IParameterSymbol> parameterDefinitions,
            bool isExplicit,
            CodeGenerationOptions options)
        {
            // Bracketed parameter lists come from indexers.  Those don't have type parameters, so we
            // could never have a typeParameterMapping.
            var parameters = GetParameters(parameterDefinitions, isExplicit, options);

            return SyntaxFactory.BracketedParameterList(
                parameters: SyntaxFactory.SeparatedList(parameters));
        }

        internal static ImmutableArray<ParameterSyntax> GetParameters(
            IEnumerable<IParameterSymbol> parameterDefinitions,
            bool isExplicit,
            CodeGenerationOptions options)
        {
            var result = ArrayBuilder<ParameterSyntax>.GetInstance();
            var seenOptional = false;
            var isFirstParam = true;

            foreach (var p in parameterDefinitions)
            {
                var parameter = GetParameter(p, options, isExplicit, isFirstParam, seenOptional);
                result.Add(parameter);
                seenOptional = seenOptional || parameter.Default != null;
                isFirstParam = false;
            }

            return result.ToImmutableAndFree();
        }

        internal static ParameterSyntax GetParameter(IParameterSymbol p, CodeGenerationOptions options, bool isExplicit, bool isFirstParam, bool seenOptional)
        {
            var reusableSyntax = GetReuseableSyntaxNodeForSymbol<ParameterSyntax>(p, options);
            if (reusableSyntax != null)
            {
                return reusableSyntax;
            }

            return SyntaxFactory.Parameter(p.Name.ToIdentifierToken())
                    .WithAttributeLists(GenerateAttributes(p, isExplicit, options))
                    //.WithModifiers(GenerateModifiers(p, isFirstParam))
                    .WithType(p.Type.GenerateTypeSyntax())
                    .WithDefault(GenerateEqualsValueClause(p, isExplicit, seenOptional));
        }

        private static EqualsValueClauseSyntax GenerateEqualsValueClause(
            IParameterSymbol parameter,
            bool isExplicit,
            bool seenOptional)
        {
            if (!parameter.IsParams && !isExplicit && !parameter.IsRefOrOut())
            {
                if (parameter.HasExplicitDefaultValue || seenOptional)
                {
                    var defaultValue = parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : null;
                    if (defaultValue is DateTime)
                    {
                        return null;
                    }

                    return SyntaxFactory.EqualsValueClause(
                        GenerateEqualsValueClauseWorker(parameter, defaultValue));
                }
            }

            return null;
        }

        private static ExpressionSyntax GenerateEqualsValueClauseWorker(
            IParameterSymbol parameter,
            object value)
        {
            return ExpressionGenerator.GenerateExpression(parameter.Type, value, canUseFieldReference: true);
        }

        private static SyntaxList<AttributeSyntax> GenerateAttributes(
            IParameterSymbol parameter, bool isExplicit, CodeGenerationOptions options)
        {
            if (isExplicit)
            {
                return default;
            }

            var attributes = parameter.GetAttributes();
            if (attributes.Length == 0)
            {
                return default;
            }

            return AttributeGenerator.GenerateAttributeLists(attributes, options);
        }
    }
}
