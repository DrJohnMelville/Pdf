using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public sealed partial class AsyncBitSource
{
    [FromConstructor] private IAsyncByteSource source;
    private uint residue;
    private int bitsRemaining;

    public static async ValueTask<AsyncBitSource> Create(PipeReader source)
    {
        var byteReader = new RemoveStuffedBytes(new AsyncByteSource(source));
        await byteReader.Initialize().CA();
        return new AsyncBitSource(byteReader);
    }

    public async ValueTask<int> ReadBitAsync() => (int) await ReadBitsAsync(1).CA();

    public async ValueTask<uint> ReadBitsAsync(int bits)
    {
        Debug.Assert(bits <= 32);
        while (bits > bitsRemaining) await GetMoreBitsAsync().CA();
        bitsRemaining -= bits;
        (var ret, residue) = residue.SplitHighAndLowBits(bitsRemaining);
        return ret;
    }

    private async ValueTask GetMoreBitsAsync()
    {
        residue = residue.AddLeastSignificantByte(await source.GetByte().CA());
        bitsRemaining += 8;
    }
}