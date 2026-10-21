using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;
using Melville.StreamInterfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

internal sealed partial class ParsingFileOwner : IDisposable
{
    private readonly IMultiplexSource source;
    private long preHeaderOffset = 0;
    public long StreamLength => source.Length;
    public IndirectObjectRegistry IndirectResolver { get; }
    private IDocumentCryptContext documentCryptContext = NullSecurityHandler.Instance;
    private readonly IPasswordSource passwordSource;

    public ParsingFileOwner(IMultiplexSource source, IPasswordSource passwordSource)
    {
        this.source = source;
        this.passwordSource = passwordSource;
        IndirectResolver = new IndirectObjectRegistry(this);
    }

    public void SetPreheaderOffset(long offset) => preHeaderOffset = offset;

    private long AdjustOffsetForPreHeaderBytes(long offset) => offset + preHeaderOffset;

    public IAsyncReader SubsetIAsyncReader(long offset) =>
        source.ReaderFrom(AdjustOffsetForPreHeaderBytes(offset));

    public DisposableParsingReader SubsetReader(long offset, int objectNumber = -1, int generation = -1)
    {
        var ret = new DisposableParsingReader(this, source.ReaderFrom(AdjustOffsetForPreHeaderBytes(offset)));
        ret.Buffer.RelativePosition = offset;
        return ret;
    }

    public Stream SubsetStream(long position, long length) =>
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