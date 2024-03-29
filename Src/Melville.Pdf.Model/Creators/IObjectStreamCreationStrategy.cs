﻿using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;


internal interface IObjectStreamCreationStrategy
{
    IDisposable EnterObjectStreamContext(IPdfObjectCreatorRegistry creator);
}

[StaticSingleton]
internal partial class EncodeInObjectStream: IObjectStreamCreationStrategy
{
    public IDisposable EnterObjectStreamContext(IPdfObjectCreatorRegistry creator) => 
        creator.ObjectStreamContext();
}

[StaticSingleton]
internal partial class NoObjectStream: IObjectStreamCreationStrategy, IDisposable
{
    public IDisposable EnterObjectStreamContext(IPdfObjectCreatorRegistry creator) => this;
    public void Dispose() { }
}