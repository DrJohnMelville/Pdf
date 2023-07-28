using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal class ObjectStreamIndirectObject : OwnedLocationIndirectObject
{
    private readonly long referredOrdinal;

    public ObjectStreamIndirectObject(
        int objectNumber, int generationNumber, ParsingFileOwner owner, 
        long referredOrdinal) : 
        base(objectNumber, generationNumber, owner)
    {
        this.referredOrdinal = referredOrdinal;
    }

    protected override async ValueTask ComputeValueAsync(ParsingFileOwner owner)
    {
        var referredObject = await owner.IndirectResolver
            .FindIndirect((int)referredOrdinal, 0).DirectValueAsync().CA();
        if (referredObject is PdfStream stream)
            await LoadObjectStreamAsync(owner, stream).CA();
    }

    public static async ValueTask LoadObjectStreamAsync(ParsingFileOwner owner, PdfStream source)
    {
        throw new InvalidOperationException("Obsolete object");
        // await TryLoadExtendedBaseStreamAsync(owner, source).CA();
        //
        // await using var data = await source.StreamContentAsync().CA();
        // var reader = new SubsetParsingReader( owner.ParsingReaderForStream(data, 0));
        // var objectLocations = await ObjectStreamOperations.GetIncludedObjectNumbersAsync(
        //     source, reader.Reader).CA();
        // var first = (await source.GetAsync<PdfNumber>(KnownNames.First).CA()).IntValue;
        // foreach (var location in objectLocations)
        // {
        //     await reader.Reader.AdvanceToLocalPositionAsync(first + location.Offset).CA();
        //     reader.ExclusiveEndPosition = first + location.NextOffset;
        //     var obj = await PdfParserParts.Composite.ParseAsync(reader).CA();
        //     AcceptObject(owner.IndirectResolver,location.ObjectNumber,obj);
        // }
    }

    private static async ValueTask TryLoadExtendedBaseStreamAsync(ParsingFileOwner owner, PdfStream source)
    {
        var refstr = await source.GetOrNullAsync<PdfStream>(KnownNames.Extends).CA();
        if (refstr is { } referredStream)
            await LoadObjectStreamAsync(owner, referredStream).CA();
    }

    private static void AcceptObject(IIndirectObjectResolver resolver,
        int objectNumber, PdfObject pdfObject)
    {
        var obj = resolver.FindIndirect(objectNumber, 0)as ObjectStreamIndirectObject;
        obj?.SetFinalValue(pdfObject);
    }
}

#warning -- get rid of this -- good stuff when to SubsetByteSource
internal partial class SubsetParsingReader : IParsingReader, IByteSourceWithGlobalPosition
{
    public long ExclusiveEndPosition { get; set; } = long.MaxValue;
    
    [DelegateTo] [FromConstructor] private readonly IParsingReader inner;
    [DelegateTo] private IByteSourceWithGlobalPosition innerReader => inner.Reader;
    public IByteSourceWithGlobalPosition Reader => this;

    public bool TryRead(out ReadResult result)
    {
        if (!inner.Reader.TryRead(out result)) return false;
        result = ClipResult(result);
        return true;
    }

    public async ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default) =>
        ClipResult(await innerReader.ReadAsync(cancellationToken).CA());

    private ReadResult ClipResult(ReadResult result) =>
        ResultDoesNotOverflowAllowedLength(result) ? 
            result : 
            SliceToAllowedLength(result);

    private long MaxReadLength() => ExclusiveEndPosition - innerReader.Position;

    private bool ResultDoesNotOverflowAllowedLength(ReadResult result) => 
        result.IsCanceled ||result.Buffer.Length <= MaxReadLength();

    private ReadResult SliceToAllowedLength(ReadResult result) => 
        new ReadResult(result.Buffer.Slice(0, MaxReadLength()), false, true);
}