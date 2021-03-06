﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Threading;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.Organizing.Organizers;

namespace StarkPlatform.Compiler.Stark.Organizing.Organizers
{
    [ExportSyntaxNodeOrganizer(LanguageNames.Stark), Shared]
    internal class StructDeclarationOrganizer : AbstractSyntaxNodeOrganizer<StructDeclarationSyntax>
    {
        protected override StructDeclarationSyntax Organize(
            StructDeclarationSyntax syntax,
            CancellationToken cancellationToken)
        {
            return syntax.Update(
                syntax.AttributeLists,
                ModifiersOrganizer.Organize(syntax.Modifiers),
                syntax.Keyword,
                syntax.Identifier,
                syntax.TypeParameterList,
                syntax.ExtendList,
                syntax.ImplementList,
                syntax.ConstraintClauses,
                syntax.OpenBraceToken,
                MemberDeclarationsOrganizer.Organize(syntax.Members, cancellationToken),
                syntax.CloseBraceToken,
                syntax.EosToken);
        }
    }
}
