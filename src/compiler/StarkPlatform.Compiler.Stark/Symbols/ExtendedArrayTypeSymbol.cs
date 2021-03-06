﻿using System.Collections.Immutable;
using Roslyn.Utilities;
using StarkPlatform.Reflection.Metadata;

namespace StarkPlatform.Compiler.Stark.Symbols
{
    internal sealed class ExtendedArrayTypeSymbol : ArrayTypeSymbol, IExtendedTypeSymbol
    {
        private readonly TypeAccessModifiers _accessModifiers;
        private readonly ArrayTypeSymbol _underlyingArray;


        public ExtendedArrayTypeSymbol(TypeSymbolWithAnnotations arrayWithAnnotations, ArrayTypeSymbol underlyingArray, TypeAccessModifiers accessModifiers) : base(underlyingArray.ElementType, underlyingArray.BaseTypeNoUseSiteDiagnostics)
        {
            _underlyingArray = underlyingArray;
            _accessModifiers = accessModifiers;
        }


        internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol> basesBeingResolved = null)
        {
            return _underlyingArray.InterfacesNoUseSiteDiagnostics(basesBeingResolved);
        }

        protected internal override ArrayTypeSymbol WithElementTypeCore(TypeSymbolWithAnnotations elementType)
        {
            return _underlyingArray.WithElementTypeCore(elementType);
        }

        public override bool IsRefLikeType => (_accessModifiers & TypeAccessModifiers.Transient) != 0;

        public override TypeAccessModifiers AccessModifiers => _accessModifiers;
    }
}
