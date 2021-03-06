﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace StarkPlatform.Compiler.Stark
{
    internal enum ArgumentAnalysisResultKind : byte
    {
        Normal,
        Expanded,
        NoCorrespondingParameter,
        FirstInvalid = NoCorrespondingParameter,
        NoCorrespondingNamedParameter,
        DuplicateNamedArgument,
        RequiredParameterMissing,
        NameUsedForPositional,
        BadNonTrailingNamedArgument // if a named argument refers to a different position, all following arguments must be named
    }
}
