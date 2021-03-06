﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace StarkPlatform.Compiler
{
    // members of special types
    internal enum SpecialMember
    {
        System_String__CtorSZArrayChar,

        System_String__ConcatStringString,
        System_String__ConcatStringStringString,
        System_String__ConcatStringStringStringString,
        System_String__ConcatStringArray,

        System_String__ConcatObject,
        System_String__ConcatObjectObject,
        System_String__ConcatObjectObjectObject,
        System_String__ConcatObjectArray,

        System_String__op_Equality,
        System_String__op_Inequality,
        core_String__size,
        core_String__item,
        System_String__Format,

        System_Double__IsNaN,
        System_Single__IsNaN,

        System_Delegate__Combine,
        System_Delegate__Remove,
        System_Delegate__op_Equality,
        System_Delegate__op_Inequality,

        core_Iterable_T_TIterator__iterate_begin,
        core_Iterable_T_TIterator__iterate_has_current,
        core_Iterable_T_TIterator__iterate_current,
        core_Iterable_T_TIterator__iterate_next,
        core_Iterable_T_TIterator__iterate_end,

        core_MutableIterable_T_TIterator__iterate_current,

        System_IDisposable__Dispose,

        core_Array__size,
        core_Array_T__item,
        core_ISizeable__get_size,
        core_IArray_T__get_item,

        core_Index__value,

        System_Object__GetHashCode,
        System_Object__Equals,
        System_Object__ToString,
        System_Object__ReferenceEquals,

        System_IntPtr__op_Explicit_ToPointer,
        System_IntPtr__op_Explicit_ToInt32,
        System_IntPtr__op_Explicit_ToInt64,
        System_IntPtr__op_Explicit_FromPointer,
        System_IntPtr__op_Explicit_FromInt32,
        System_IntPtr__op_Explicit_FromInt64,
        System_UIntPtr__op_Explicit_ToPointer,
        System_UIntPtr__op_Explicit_ToUInt32,
        System_UIntPtr__op_Explicit_ToUInt64,
        System_UIntPtr__op_Explicit_FromPointer,
        System_UIntPtr__op_Explicit_FromUInt32,
        System_UIntPtr__op_Explicit_FromUInt64,

        System_Nullable_T_GetValueOrDefault,
        core_Option_T_get_value,
        core_Option_T_get_has_value,
        System_Nullable_T__ctor,
        System_Nullable_T__op_Implicit_FromT,
        System_Nullable_T__op_Explicit_ToT,

        Count
    }
}
