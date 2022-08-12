using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public interface IObjectStreamCreationStrategy
{
    IDisposable EnterObjectStreamContext(ILowLevelDocumentCreator creator);
}

[StaticSingleton]
public partial class EncodeInObjectStream: IObjectStreamCreationStrategy
{
    public IDisposable EnterObjectStreamContext(ILowLevelDocumentCreator creator) => 
        creator.ObjectStreamContext();
}

[StaticSingleton]
public partial class NoObjectStream: IObjectStreamCreationStrategy, IDisposable
{
    public IDisposable EnterObjectStreamContext(ILowLevelDocumentCreator creator) => this;
    public void Dispose() { }
}