using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Primitives.PipeReaderWithPositions;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

public interface IParsingReader : IDisposable, IPipeReaderWithPosition
{
    IPdfObjectParser RootObjectParser { get; }
    IIndirectObjectResolver IndirectResolver { get; }
    ParsingFileOwner Owner { get; }

    IObjectCryptContext ObjectCryptContext();
}


public partial class ParsingReader : IParsingReader
{
    public IPdfObjectParser RootObjectParser => Owner.RootObjectParser;
    public IIndirectObjectResolver IndirectResolver => Owner.IndirectResolver;
    public IObjectCryptContext ObjectCryptContext ()=> NullSecurityHandler.Instance;

    [DelegateTo()] private IPipeReaderWithPosition reader;

    public ParsingFileOwner Owner { get; }

    public ParsingReader(
        ParsingFileOwner owner, PipeReader reader, long lastSeek)
    {
        Owner = owner;
        this.reader = new PipeReaderWithPosition(reader, lastSeek);
    }

    public void Dispose()
    {
        Owner.ReturnReader(this);
    }


}