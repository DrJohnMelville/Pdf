using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

internal sealed partial class ParsingFileOwner: IDisposable
{
    private readonly IMultiplexSource source;
    private long preHeaderOffset = 0;
    public long StreamLength => source.Length;
    public IndirectObjectRegistry NewIndirectResolver { get; }
    private IDocumentCryptContext documentCryptContext = NullSecurityHandler.Instance;
    private readonly IPasswordSource passwordSource;

    public ParsingFileOwner(Stream source, IPasswordSource passwordSource) : this(
        MultiplexSourceFactory.Create(source), passwordSource)
    {
    }

    public ParsingFileOwner(IMultiplexSource source, IPasswordSource passwordSource)
    {
        this.source = source;
        this.passwordSource = passwordSource;
        NewIndirectResolver = new IndirectObjectRegistry(this);
    }
    
    public void SetPreheaderOffset(long offset) => preHeaderOffset = offset;

    private long AdjustOffsetForPreHeaderBytes(long offset) => offset + preHeaderOffset;

    public ParsingReader RentReader(long offset, int objectNumber=-1, int generation = -1) =>
        new(this, 
            source.ReadPipeFrom(AdjustOffsetForPreHeaderBytes(offset), offset));

    public Stream RentStream(long position, long length) => 
        new RentedStream(source.ReadFrom(AdjustOffsetForPreHeaderBytes(position)), length);


    public IObjectCryptContext CryptContextForObject(int objectNumber, int generation) =>
        documentCryptContext.ContextForObject(objectNumber, generation);

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
}