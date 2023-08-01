using System.IO.Pipelines;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers2;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

internal interface IParsingReader
{
    #warning == figure out if I  can share this.
    RootObjectParser NewRootObjectParser => new RootObjectParser(this);
    ParsingFileOwner Owner { get; }
    IByteSourceWithGlobalPosition Reader { get; }
    #warning need to get rid of this
    IObjectCryptContext ObjectCryptContext();
}


internal class ParsingReader: IParsingReader
{
    public IObjectCryptContext ObjectCryptContext ()=> NullSecurityHandler.Instance;
    
    public IByteSourceWithGlobalPosition Reader { get; }

    public ParsingFileOwner Owner { get; }

    public ParsingReader(ParsingFileOwner owner, PipeReader reader, long lastSeek)
    {
        Owner = owner;
        this.Reader = new ByteSourceWithGlobalPosition(reader, lastSeek);
    }
}