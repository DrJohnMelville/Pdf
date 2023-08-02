using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

internal sealed partial class ParsingFileOwner: IDisposable, IIndirectObjectRegistry
{
    private readonly MultiplexedStream source;
    private long preHeaderOffset = 0;
    public long StreamLength => source.Length;
    public IndirectObjectRegistry NewIndirectResolver { get; }
    private IDocumentCryptContext documentCryptContext = NullSecurityHandler.Instance;
    private readonly IPasswordSource passwordSource;

    public ParsingFileOwner(Stream source, IPasswordSource passwordSource)
    {
        this.source = new MultiplexedStream(source);
        this.passwordSource = passwordSource;
        NewIndirectResolver = new IndirectObjectRegistry(this);
    }
    
    public void SetPreheaderOffset(long offset) => preHeaderOffset = offset;

    private long AdjustOffsetForPreHeaderBytes(long offset) => offset + preHeaderOffset;

    #warning make this a synchronous method
    public ValueTask<IParsingReader> RentReaderAsync(long offset, int objectNumber=-1, int generation = -1)
    {
        return new(ParsingReaderForStream(source.ReadFrom(AdjustOffsetForPreHeaderBytes(offset)), offset));

    }

    public ValueTask<Stream> RentStreamAsync(long position, long length)
    {
        var ret = new RentedStream(source.ReadFrom(AdjustOffsetForPreHeaderBytes(position)), length);
        return new ValueTask<Stream>(ret);
    }


    public IObjectCryptContext CryptContextForObject(int objectNumber, int generation) =>
        documentCryptContext.ContextForObject(objectNumber, generation);

    public IParsingReader ParsingReaderForStream(Stream s, long position) =>
        new ParsingReader(this, PipeReader.Create(s, pipeOptions), position);

    private static readonly StreamPipeReaderOptions pipeOptions = new(leaveOpen: true);
    
    public async ValueTask InitializeDecryptionAsync(PdfDictionary trailerDictionary)
    {
        if (AlreadyInitializedDecryption()) return;
        documentCryptContext = await 
            TrailerToDocumentCryptContext.CreateDecryptorFactoryAsync(trailerDictionary, passwordSource).CA();
    }

    private bool AlreadyInitializedDecryption()
    {
        return documentCryptContext != NullSecurityHandler.Instance;
    }

    public void Dispose() => source.Dispose();

    public void RegisterDeletedBlock(int number, int next, int generation)
    {
    }
    #warning -- should be able to get rid of this by passing an IIndirectValueregistry to the parsers
    public void RegisterIndirectBlock(int number, int generation, long offset)
    {
        NewIndirectResolver.RegisterIndirectBlock(number, generation, offset);
    }

    #warning -- should be able to get rid of this by passing an IIndirectValueregistry to the parsers
    public void RegisterObjectStreamBlock(int number, int referredStreamOrdinal, int positionInStream)
    {
        NewIndirectResolver.RegisterObjectStreamBlock(number, referredStreamOrdinal, positionInStream);
    }
}