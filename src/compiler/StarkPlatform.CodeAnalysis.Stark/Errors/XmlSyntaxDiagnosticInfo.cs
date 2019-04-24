﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using Roslyn.Utilities;

namespace StarkPlatform.CodeAnalysis.Stark
{
    internal sealed class XmlSyntaxDiagnosticInfo : SyntaxDiagnosticInfo
    {
        static XmlSyntaxDiagnosticInfo()
        {
            ObjectBinder.RegisterTypeReader(typeof(XmlSyntaxDiagnosticInfo), r => new XmlSyntaxDiagnosticInfo(r));
        }

        private readonly XmlParseErrorCode _xmlErrorCode;

        internal XmlSyntaxDiagnosticInfo(XmlParseErrorCode code, params object[] args)
            : this(0, 0, code, args)
        {
        }

        internal XmlSyntaxDiagnosticInfo(int offset, int width, XmlParseErrorCode code, params object[] args)
            : base(offset, width, ErrorCode.WRN_XMLParseError, args)
        {
            _xmlErrorCode = code;
        }

        #region Serialization

        protected override void WriteTo(ObjectWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteUInt32((uint)_xmlErrorCode);
        }

        private XmlSyntaxDiagnosticInfo(ObjectReader reader)
            : base(reader)
        {
            _xmlErrorCode = (XmlParseErrorCode)reader.ReadUInt32();
        }

        #endregion

        public override string GetMessage(IFormatProvider formatProvider = null)
        {
            var culture = formatProvider as CultureInfo;

            string messagePrefix = this.MessageProvider.LoadMessage(this.Code, culture);
            string message = ErrorFacts.GetMessage(_xmlErrorCode, culture);

            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(message));

            if (this.Arguments == null || this.Arguments.Length == 0)
            {
                return String.Format(formatProvider, messagePrefix, message);
            }

            return String.Format(formatProvider, String.Format(formatProvider, messagePrefix, message), GetArgumentsToUse(formatProvider));
        }
    }
}
