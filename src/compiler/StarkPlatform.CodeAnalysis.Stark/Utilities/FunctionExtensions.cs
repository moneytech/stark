﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using StarkPlatform.CodeAnalysis.Stark.Symbols;
using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.Text;

namespace StarkPlatform.CodeAnalysis.Stark
{
    internal static partial class FunctionExtensions
    {
        public static HashSet<T> TransitiveClosure<T>(
            this Func<T, IEnumerable<T>> relation,
            T item)
        {
            var closure = new HashSet<T>();
            var stack = new Stack<T>();
            stack.Push(item);
            while (stack.Count > 0)
            {
                T current = stack.Pop();
                foreach (var newItem in relation(current))
                {
                    if (closure.Add(newItem))
                    {
                        stack.Push(newItem);
                    }
                }
            }

            return closure;
        }
    }
}
