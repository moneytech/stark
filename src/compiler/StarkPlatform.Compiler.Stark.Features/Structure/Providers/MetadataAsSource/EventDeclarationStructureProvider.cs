﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.PooledObjects;

namespace StarkPlatform.Compiler.Stark.Structure.MetadataAsSource
{
    internal class MetadataEventDeclarationStructureProvider : AbstractMetadataAsSourceStructureProvider<EventDeclarationSyntax>
    {
        protected override SyntaxToken GetEndToken(EventDeclarationSyntax node)
        {
            return node.Modifiers.Count > 0
                    ? node.Modifiers.First()
                    : node.EventKeyword;
        }
    }
}
