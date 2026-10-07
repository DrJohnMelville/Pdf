using Melville.StreamInterfaces;
using System.Buffers;

namespace Melville.Parsing.CountingReaders;

/// <summary>
/// this is a temporary shim to change a IByteSource into an IStreamReader.  It lets me try
/// some new parsing from the old infrasructure.
/// </summary>
[Obsolete("This is a temporary shim for replacing the IByteSource with the ParsingBuffer")]
public class IByteSourceToIAsyncReadStream(IByteSource source) : IAsyncReader
{
    ///<inheritdoc/>
    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
        var result = await source.ReadAsync();
        var ret = TransferBuffer(result.Buffer, buffer.Span);
        source.AdvanceTo(result.Buffer.GetPosition(ret));
        return (int)ret;
    }
    long TransferBuffer(ReadOnlySequence<byte> buffer, Span<byte> span)
    {
        var retLen = Math.Min(buffer.Length, span.Length);
        buffer.Slice(0, retLen).CopyTo(span);
        return retLen;
    }
}
