using System.IO.Pipelines;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers2;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

internal interface IParsingReader
{
    IPdfObjectParser RootObjectParser { get; }
    #warning == figure out if I  can share this.
    RootObjectParser NewRootObjectParser => new RootObjectParser(this);
    IIndirectObjectResolver IndirectResolver { get; }
    ParsingFileOwner Owner { get; }
    IByteSourceWithGlobalPosition Reader { get; }
    IObjectCryptContext ObjectCryptContext();
}


internal class ParsingReader: IParsingReader
{
    public IPdfObjectParser RootObjectParser => PdfParserParts.Composite;
    public IIndirectObjectResolver IndirectResolver => Owner.IndirectResolver;
    public IObjectCryptContext ObjectCryptContext ()=> NullSecurityHandler.Instance;
    
    public IByteSourceWithGlobalPosition Reader { get; }

    public ParsingFileOwner Owner { get; }

    public ParsingReader(ParsingFileOwner owner, PipeReader reader, long lastSeek)
    {
        Owner = owner;
        this.Reader = new ByteSourceWithGlobalPosition(reader, lastSeek);
    }
}