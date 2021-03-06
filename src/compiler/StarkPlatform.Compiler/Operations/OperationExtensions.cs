﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using StarkPlatform.Compiler.PooledObjects;

namespace StarkPlatform.Compiler.Operations
{
    public static partial class OperationExtensions
    {
        /// <summary>
        /// This will check whether context around the operation has any error such as syntax or semantic error
        /// </summary>
        internal static bool HasErrors(this IOperation operation, Compilation compilation, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (compilation == null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            // once we made sure every operation has Syntax, we will remove this condition
            if (operation.Syntax == null)
            {
                return true;
            }

            // if wrong compilation is given, GetSemanticModel will throw due to tree not belong to the given compilation.
            var model = operation.SemanticModel ?? compilation.GetSemanticModel(operation.Syntax.SyntaxTree);
            if (model.IsSpeculativeSemanticModel)
            {
                // GetDiagnostics not supported for speculative semantic model.
                // https://github.com/dotnet/roslyn/issues/28075
                return false;
            }

            return model.GetDiagnostics(operation.Syntax.Span, cancellationToken).Any(d => d.DefaultSeverity == DiagnosticSeverity.Error);
        }

        /// <summary>
        /// Returns all the descendant operations of the given <paramref name="operation"/> in evaluation order.
        /// </summary>
        /// <param name="operation">Operation whose descendants are to be fetched.</param>
        public static IEnumerable<IOperation> Descendants(this IOperation operation)
        {
            return Descendants(operation, includeSelf: false);
        }

        /// <summary>
        /// Returns all the descendant operations of the given <paramref name="operation"/> including the given <paramref name="operation"/> in evaluation order.
        /// </summary>
        /// <param name="operation">Operation whose descendants are to be fetched.</param>
        public static IEnumerable<IOperation> DescendantsAndSelf(this IOperation operation)
        {
            return Descendants(operation, includeSelf: true);
        }

        private static IEnumerable<IOperation> Descendants(IOperation operation, bool includeSelf)
        {
            if (operation == null)
            {
                yield break;
            }

            if (includeSelf)
            {
                yield return operation;
            }

            var stack = ArrayBuilder<IEnumerator<IOperation>>.GetInstance();
            stack.Push(operation.Children.GetEnumerator());

            while (stack.Any())
            {
                var iterator = stack.Pop();

                if (!iterator.MoveNext())
                {
                    continue;
                }

                var current = iterator.Current;

                // push current iterator back in to the stack
                stack.Push(iterator);

                // push children iterator to the stack
                if (current != null)
                {
                    yield return current;
                    stack.Push(current.Children.GetEnumerator());
                }
            }

            stack.Free();
        }

        /// <summary>
        /// Gets all the declared local variables in the given <paramref name="declarationGroup"/>.
        /// </summary>
        /// <param name="declarationGroup">Variable declaration group</param>
        public static ImmutableArray<ILocalSymbol> GetDeclaredVariables(this IVariableDeclarationGroupOperation declarationGroup)
        {
            if (declarationGroup == null)
            {
                throw new ArgumentNullException(nameof(declarationGroup));
            }

            var arrayBuilder = ArrayBuilder<ILocalSymbol>.GetInstance();
            foreach (IVariableDeclarationOperation group in declarationGroup.Declarations)
            {
                arrayBuilder.Add(group.Symbol);                
            }

            return arrayBuilder.ToImmutableAndFree();
        }

        /// <summary>
        /// Gets the variable initializer for the given <paramref name="declarationOperation"/>, checking to see if there is a parent initializer
        /// if the single variable initializer is null.
        /// </summary>
        /// <param name="declarationOperation">Single variable declaration to retrieve initializer for.</param>
        public static IVariableInitializerOperation GetVariableInitializer(this IVariableDeclarationOperation declarationOperation)
        {
            if (declarationOperation == null)
            {
                throw new ArgumentNullException(nameof(declarationOperation));
            }

            return declarationOperation.Initializer ?? (declarationOperation.Parent as IVariableDeclarationOperation)?.Initializer;
        }

        /// <summary>
        /// Gets the root operation for the <see cref="IOperation"/> tree containing the given <paramref name="operation"/>.
        /// </summary>
        /// <param name="operation">Operation whose root is requested.</param>
        internal static IOperation GetRootOperation(this IOperation operation)
        {
            Debug.Assert(operation != null);

            while (operation.Parent != null)
            {
                operation = operation.Parent;
            }

            return operation;
        }

        /// <summary>
        /// Gets either a loop or a switch operation that corresponds to the given branch operation.
        /// </summary>
        /// <param name="operation">The branch operation for which a corresponding operation is looked up</param>
        /// <returns>The corresponding operation or <c>null</c> in case not found (e.g. no loop or switch syntax, or the branch is not a break or continue)</returns>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null</exception>
        /// <exception cref="InvalidOperationException">The operation is a part of Control Flow Graph</exception>
        public static IOperation GetCorrespondingOperation(this IBranchOperation operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (operation.SemanticModel == null)
            {
                throw new InvalidOperationException(CodeAnalysisResources.OperationMustNotBeControlFlowGraphPart);
            }

            if (operation.BranchKind != BranchKind.Break && operation.BranchKind != BranchKind.Continue)
            {
                return null;
            }

            if (operation.Target == null)
            {
                return null;
            }

            for (IOperation current = operation; current.Parent != null; current = current.Parent)
            {
                switch (current)
                {
                    case ILoopOperation correspondingLoop when operation.Target.Equals(correspondingLoop.ExitLabel) ||
                                                               operation.Target.Equals(correspondingLoop.ContinueLabel):
                        return correspondingLoop;
                    case ISwitchOperation correspondingSwitch when operation.Target.Equals(correspondingSwitch.ExitLabel):
                        return correspondingSwitch;
                }
            }

            return default;
        }
    }
}
