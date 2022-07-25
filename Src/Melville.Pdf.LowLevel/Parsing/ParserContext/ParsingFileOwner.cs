using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using EncryptingParsingReader = Melville.Pdf.LowLevel.Encryption.CryptContexts.EncryptingParsingReader;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

public sealed partial class ParsingFileOwner: IDisposable, IIndirectObjectRegistry
{
    private readonly MultiplexedStream source;
    private long preHeaderOffset = 0;
    public long StreamLength => source.Length;
    public IIndirectObjectResolver IndirectResolver { get; }
    private IDocumentCryptContext documentCryptContext = NullSecurityHandler.Instance;
    private readonly IPasswordSource passwordSource;

    public ParsingFileOwner(Stream source, IPasswordSource? passwordSource = null,
        IIndirectObjectResolver? indirectResolver = null)
    {
        this.source = new MultiplexedStream(source);
        this.passwordSource = passwordSource ?? new NullPasswordSource();
        IndirectResolver = indirectResolver ?? new IndirectObjectResolver();
        if (!source.CanSeek) throw new PdfParseException("PDF Parsing requires a seekable stream");
    }
    
    public void SetPreheaderOffset(long offset) => preHeaderOffset = offset;

    private long AdjustOffsetForPreHeaderBytes(long offset) => offset + preHeaderOffset;

    public ValueTask<IParsingReader> RentReader(long offset, int objectNumber=-1, int generation = -1)
    {
        var reader = ParsingReaderForStream(source.ReadFrom(AdjustOffsetForPreHeaderBytes(offset)), offset);
        return new ValueTask<IParsingReader>(TryWrapWithDecryptor(objectNumber, generation, reader));
            
    }

    public ValueTask<Stream> RentStream(long position, long length)
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
    
    public async ValueTask InitializeDecryption(PdfDictionary trailerDictionary)
    {
        if (AlreadyInitializedDecryption()) return;
        documentCryptContext = await 
            TrailerToDocumentCryptContext.CreateDecryptorFactory(trailerDictionary, passwordSource).CA();
    }

    private bool AlreadyInitializedDecryption()
    {
        return documentCryptContext != NullSecurityHandler.Instance;
    }

    public void Dispose() => source.Dispose();

    public void RegisterDeletedBlock(int number, int next, int generation)
    {
    }

    public void RegistedNullObject(int number, int next, int generation) => 
        IndirectResolver.AddLocationHint(new PdfIndirectObject(number, generation,PdfTokenValues.Null));

    public void RegisterIndirectBlock(int number, long generation, long offset) =>
        IndirectResolver.AddLocationHint(new RawLocationIndirectObject(number, (int)generation, this, offset));

    public void RegisterObjectStreamBlock(int number, long referredStreamOrdinal, long referredStreamGeneration) =>
        IndirectResolver.AddLocationHint(new ObjectStreamIndirectObject(number, 0, this, referredStreamOrdinal));
}