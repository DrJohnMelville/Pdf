using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;

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

    public async ValueTask<ReadResult> ReadAsync() =>
        ClipResult(await inner.ReadAsync().CA());

    private ReadResult ClipResult(ReadResult result) =>
        ResultDoesNotOverflowAllowedLength(result) ? 
            result : 
            SliceToAllowedLength(result);

    private long MaxReadLength() => ExclusiveEndPosition - inner.Position;

    private bool ResultDoesNotOverflowAllowedLength(ReadResult result) => 
        result.IsCanceled ||result.Buffer.Length <= MaxReadLength();

    private ReadResult SliceToAllowedLength(ReadResult result) =>
        new(result.Buffer.Slice(0, MaxReadLength()), false, true);
}