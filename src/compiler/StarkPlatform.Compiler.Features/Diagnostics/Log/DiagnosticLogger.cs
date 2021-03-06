﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using StarkPlatform.Compiler.Internal.Log;

namespace StarkPlatform.Compiler.Diagnostics.Log
{
    internal static class DiagnosticLogger
    {
        private const string From = nameof(From);
        private const string Id = nameof(Id);
        private const string HasDescription = nameof(HasDescription);
        private const string Uri = nameof(Uri);

        public static void LogHyperlink(
            string from,
            string id,
            bool description,
            bool telemetry,
            string uri)
        {
            Logger.Log(FunctionId.Diagnostics_HyperLink, KeyValueLogMessage.Create(m =>
            {
                m[From] = from;
                m[Id] = telemetry ? id : id.GetHashCode().ToString();
                m[HasDescription] = description;
                m[Uri] = telemetry ? uri : uri.GetHashCode().ToString();
            }));
        }
    }
}
