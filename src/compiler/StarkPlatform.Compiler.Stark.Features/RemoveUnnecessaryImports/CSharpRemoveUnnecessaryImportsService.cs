﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using StarkPlatform.Compiler.Host.Mef;
using StarkPlatform.Compiler.RemoveUnnecessaryImports;

namespace StarkPlatform.Compiler.Stark.RemoveUnnecessaryImports
{
    [ExportLanguageService(typeof(IRemoveUnnecessaryImportsService), LanguageNames.Stark), Shared]
    internal partial class CSharpRemoveUnnecessaryImportsService : AbstractCSharpRemoveUnnecessaryImportsService
    {
    }
}
