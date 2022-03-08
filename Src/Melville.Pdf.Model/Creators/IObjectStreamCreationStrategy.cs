using System;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public interface IObjectStreamCreationStrategy
{
    IDisposable EnterObjectStreamContext(ILowLevelDocumentCreator creator);
}

public class EncodeInObjectStream: IObjectStreamCreationStrategy
{
    public static readonly EncodeInObjectStream Instance = new();
    private EncodeInObjectStream() {}
    public IDisposable EnterObjectStreamContext(ILowLevelDocumentCreator creator) => 
        creator.ObjectStreamContext();
}

public class NoObjectStream: IObjectStreamCreationStrategy, IDisposable
{
    public static readonly NoObjectStream Instance = new();
    private NoObjectStream() { }
    public IDisposable EnterObjectStreamContext(ILowLevelDocumentCreator creator) => this;
    public void Dispose() { }
}