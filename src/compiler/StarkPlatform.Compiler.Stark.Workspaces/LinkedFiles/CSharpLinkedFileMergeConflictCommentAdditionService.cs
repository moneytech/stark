﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using StarkPlatform.Compiler.Host.Mef;

namespace StarkPlatform.Compiler.Stark
{
    [ExportLanguageService(typeof(ILinkedFileMergeConflictCommentAdditionService), LanguageNames.Stark), Shared]
    internal sealed class CSharpLinkedFileMergeConflictCommentAdditionService : AbstractLinkedFileMergeConflictCommentAdditionService
    {
        internal override string GetConflictCommentText(string header, string beforeString, string afterString)
        {
            if (beforeString == null && afterString == null)
            {
                // Whitespace only
                return null;
            }
            else if (beforeString == null)
            {
                // New code
                return string.Format(@"
/* {0}
{1}
{2}
*/
",
                header,
                WorkspacesResources.Added_colon,
                afterString);
            }
            else if (afterString == null)
            {
                // Removed code
                return string.Format(@"
/* {0}
{1}
{2}
*/
",
                header,
                WorkspacesResources.Removed_colon,
                beforeString);
            }
            else
            {
                // Changed code
                return string.Format(@"
/* {0}
{1}
{2}
{3}
{4}
*/
",
                header,
                WorkspacesResources.Before_colon,
                beforeString,
                WorkspacesResources.After_colon,
                afterString);
            }
        }
    }
}
