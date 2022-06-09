using System.IO.Pipelines;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

public interface IParsingReader
{
    IPdfObjectParser RootObjectParser { get; }
    IIndirectObjectResolver IndirectResolver { get; }
    ParsingFileOwner Owner { get; }
    IPipeReaderWithPosition Reader { get; }
    IObjectCryptContext ObjectCryptContext();
}


public class ParsingReader: IParsingReader
{
    public IPdfObjectParser RootObjectParser => PdfParserParts.Composite;
    public IIndirectObjectResolver IndirectResolver => Owner.IndirectResolver;
    public IObjectCryptContext ObjectCryptContext ()=> NullSecurityHandler.Instance;
    
    public IPipeReaderWithPosition Reader { get; }

    public ParsingFileOwner Owner { get; }

    public ParsingReader(ParsingFileOwner owner, PipeReader reader, long lastSeek)
    {
        Owner = owner;
        this.Reader = new PipeReaderWithPosition(reader, lastSeek);
    }
}