using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;

internal partial class SubsetByteSource : IByteSource
{
    public long ExclusiveEndPosition { get; set; } = long.MaxValue;

    [DelegateTo] [FromConstructor] private readonly IByteSource inner;

    public bool TryRead(out ReadResult result)
    {
        if (!inner.TryRead(out result)) return false;
        result = ClipResult(result);
        return true;
    }

    public async ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default) =>
        ClipResult(await inner.ReadAsync(cancellationToken).CA());

    private ReadResult ClipResult(ReadResult result) =>
        ResultDoesNotOverflowAllowedLength(result) ? 
            result : 
            SliceToAllowedLength(result);

    private long MaxReadLength() => ExclusiveEndPosition - inner.Position;

    private bool ResultDoesNotOverflowAllowedLength(ReadResult result) => 
        result.IsCanceled ||result.Buffer.Length <= MaxReadLength();

    private ReadResult SliceToAllowedLength(ReadResult result) => 
        new ReadResult(result.Buffer.Slice(0, MaxReadLength()), false, true);

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