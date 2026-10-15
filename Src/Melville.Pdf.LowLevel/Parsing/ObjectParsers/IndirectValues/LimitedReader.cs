using Melville.StreamInterfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;

internal class LimitedReader(IAsyncReader internalReader) : IAsyncReader, IStreamPosition
{
    public long LengthLimit { get; set; } = long.MaxValue;

    public long Position { get; private set; }

    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
        var ret = await internalReader.ReadAsync(
            buffer.TakeMax((int)(LengthLimit - Position)), cancellation);
        Position += ret;
        return ret;

    }
}