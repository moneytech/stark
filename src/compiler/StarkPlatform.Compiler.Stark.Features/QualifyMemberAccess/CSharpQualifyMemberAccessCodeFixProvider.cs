﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Threading;
using StarkPlatform.Compiler.CodeFixes;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.QualifyMemberAccess;
using StarkPlatform.Compiler.Shared.Extensions;

namespace StarkPlatform.Compiler.Stark.QualifyMemberAccess
{
    [ExportCodeFixProvider(LanguageNames.Stark, Name = PredefinedCodeFixProviderNames.QualifyMemberAccess), Shared]
    [ExtensionOrder(After = PredefinedCodeFixProviderNames.RemoveUnnecessaryCast)]
    internal class CSharpQualifyMemberAccessCodeFixProvider : AbstractQualifyMemberAccessCodeFixprovider<SimpleNameSyntax, InvocationExpressionSyntax>
    {
        protected override SimpleNameSyntax GetNode(Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var node = diagnostic.Location.FindNode(getInnermostNodeForTie: true, cancellationToken);
            switch (node)
            {
                case SimpleNameSyntax simpleNameSyntax:
                    return simpleNameSyntax;
                case InvocationExpressionSyntax invocationExpressionSyntax:
                    return invocationExpressionSyntax.Expression as SimpleNameSyntax;
                default:
                    return null;
            }
        }

        protected override string GetTitle() => CSharpFeaturesResources.Add_this;
    }
}
