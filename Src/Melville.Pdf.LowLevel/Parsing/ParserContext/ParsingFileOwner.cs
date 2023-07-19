using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers2;
using EncryptingParsingReader = Melville.Pdf.LowLevel.Encryption.CryptContexts.EncryptingParsingReader;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

internal sealed partial class ParsingFileOwner: IDisposable, IIndirectObjectRegistry
{
    private readonly MultiplexedStream source;
    private long preHeaderOffset = 0;
    public long StreamLength => source.Length;
    public IIndirectObjectResolver IndirectResolver { get; }
    public IndirectValueRegistry NewIndirectResolver { get; }
    private IDocumentCryptContext documentCryptContext = NullSecurityHandler.Instance;
    private readonly IPasswordSource passwordSource;

    public ParsingFileOwner(Stream source, IPasswordSource passwordSource,
        IIndirectObjectResolver indirectResolver)
    {
        this.source = new MultiplexedStream(source);
        this.passwordSource = passwordSource;
        IndirectResolver = indirectResolver;
        NewIndirectResolver = new IndirectValueRegistry(this);
    }
    
    public void SetPreheaderOffset(long offset) => preHeaderOffset = offset;

    private long AdjustOffsetForPreHeaderBytes(long offset) => offset + preHeaderOffset;

    public ValueTask<IParsingReader> RentReaderAsync(long offset, int objectNumber=-1, int generation = -1)
    {
        var reader = ParsingReaderForStream(source.ReadFrom(AdjustOffsetForPreHeaderBytes(offset)), offset);
        return new ValueTask<IParsingReader>(TryWrapWithDecryptor(objectNumber, generation, reader));
            
    }

    public ValueTask<Stream> RentStreamAsync(long position, long length)
    {
        var ret = new RentedStream(source.ReadFrom(AdjustOffsetForPreHeaderBytes(position)), length);
        return new ValueTask<Stream>(ret);
    }

    private IParsingReader TryWrapWithDecryptor(int objectNumber, int generation, IParsingReader reader)
    {
        return objectNumber > 0 && generation >= 0
            ? new EncryptingParsingReader(reader,
                documentCryptContext.ContextForObject(objectNumber, generation))
            : reader;
    }

    public IParsingReader ParsingReaderForStream(Stream s, long position) =>
        new ParsingReader(this, PipeReader.Create(s, pipeOptions), position);

    private static readonly StreamPipeReaderOptions pipeOptions = new(leaveOpen: true);
    
    public async ValueTask InitializeDecryptionAsync(PdfValueDictionary trailerDictionary)
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

    public void RegisterNullObject(int number, int next, int generation)
    {
        IndirectResolver.AddLocationHint(new PdfIndirectObject(number, generation, PdfTokenValues.Null));
        NewIndirectResolver.RegisterDirectObject(number, generation, PdfDirectValue.CreateNull());
    }

    public void RegisterIndirectBlock(int number, int generation, long offset)
    {
        IndirectResolver.AddLocationHint(new RawLocationIndirectObject(number, (int)generation, this, (int)offset));
        NewIndirectResolver.RegisterUnenclosedObject(number, generation, offset);
    }

    public void RegisterObjectStreamBlock(int number, int referredStreamOrdinal, int positionInStream)
    {
        IndirectResolver.AddLocationHint(new ObjectStreamIndirectObject(number, 0, this,
            (int)referredStreamOrdinal));
        NewIndirectResolver.RegisterObjectStreamObject(number, referredStreamOrdinal, positionInStream);
    }
}