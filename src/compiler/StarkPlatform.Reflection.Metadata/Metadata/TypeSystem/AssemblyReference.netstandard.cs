// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;

namespace StarkPlatform.Reflection.Metadata
{
    public readonly partial struct AssemblyReference
    {
        public AssemblyName GetAssemblyName()
        {
            return _reader.GetAssemblyName(Name, Version, Culture, PublicKeyOrToken, AssemblyHashAlgorithm.None, Flags);
        }
    }
}
