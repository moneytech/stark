﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using StarkPlatform.Compiler.Text;
using Roslyn.Utilities;

namespace StarkPlatform.Compiler
{
    public partial class Workspace
    {
        /// <summary>
        /// A class that responds to text buffer changes
        /// </summary>
        private class TextTracker
        {
            private readonly Workspace _workspace;
            private readonly DocumentId _documentId;
            internal readonly SourceTextContainer TextContainer;
            private readonly EventHandler<TextChangeEventArgs> _weakOnTextChanged;
            private readonly Action<Workspace, DocumentId, SourceText, PreservationMode> _onChangedHandler;

            internal TextTracker(
                Workspace workspace,
                DocumentId documentId,
                SourceTextContainer textContainer,
                Action<Workspace, DocumentId, SourceText, PreservationMode> onChangedHandler)
            {
                _workspace = workspace;
                _documentId = documentId;
                this.TextContainer = textContainer;
                _onChangedHandler = onChangedHandler;

                // use weak event so TextContainer cannot accidentally keep workspace alive.
                _weakOnTextChanged = WeakEventHandler<TextChangeEventArgs>.Create(this, (target, sender, args) => target.OnTextChanged(sender, args));
            }

            public void Connect()
            {
                this.TextContainer.TextChanged += _weakOnTextChanged;
            }

            public void Disconnect()
            {
                this.TextContainer.TextChanged -= _weakOnTextChanged;
            }

            private void OnTextChanged(object sender, TextChangeEventArgs e)
            {
                // ok, the version changed.  Report that we've got an edit so that we can analyze
                // this source file and update anything accordingly.
                _onChangedHandler(_workspace, _documentId, e.NewText, PreservationMode.PreserveIdentity);
            }
        }
    }
}
