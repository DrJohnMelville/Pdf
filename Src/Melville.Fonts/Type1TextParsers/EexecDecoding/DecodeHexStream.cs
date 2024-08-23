using System.Buffers;
using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;

internal class DecodeHexStream(Stream readFrom) : DefaultBaseStream(true, false, false)
{
    PipeReader reader = PipeReader.Create(readFrom);

    public override int Read(Span<byte> buffer)
    {
        throw new NotImplementedException("Must read async");
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        int pos = 0;
        while (pos < buffer.Length)
        {
            var result = await reader.ReadAsync(cancellationToken).CA();
//            if (result.Buffer.Length == 0) break;
            var (consumed, examined, written) = CopyToBuffer(buffer.Span[pos..], result.Buffer);
            pos += written;
            if (result.IsCompleted) break;
            reader.AdvanceTo(
                result.Buffer.GetPosition(consumed),
                result.Buffer.GetPosition(examined));
        }

        return pos;
    }

    private (int BytesConsumed, int BytesExamined, int BytesWritten)
        CopyToBuffer(Span<byte> span, ReadOnlySequence<byte> resultBuffer)
    {
        var bytesWritten = 0;
        var bytesExamined = 0;
        var bytesConsumed = 0;
        int partial = -1;
        foreach (var inputMemory in resultBuffer)
        {
            foreach (var singleByte in inputMemory.Span)
            {
                switch (partial, ValueFromDigit(singleByte))
                {
                    case (_, 255): break;
                    case (-1, var high):
                        partial = high << 4;
                        break;
                    case (_, var low):
                        span[bytesWritten++] = (byte)(partial | low);
                        partial = -1;
                        bytesConsumed = bytesExamined + 1;
                        if (bytesWritten == span.Length)
                            return (bytesConsumed, bytesExamined, bytesWritten);
                        break;
                }

                bytesExamined++;
            }
        }

        return (bytesConsumed, bytesExamined, bytesWritten);
    }

    byte ValueFromDigit(byte digitChar) => digitChar switch
    {
        >= (byte)'0' and <= (byte)'9' => (byte)(digitChar - '0'),
        >= (byte)'A' and <= (byte)'F' => (byte)(digitChar - 'A' + 10),
        >= (byte)'a' and <= (byte)'f' => (byte)(digitChar - 'a' + 10),
        _ => byte.MaxValue
    };

    protected override void Dispose(bool disposing)
    {
        readFrom.Dispose();
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await readFrom.DisposeAsync().CA();
        await base.DisposeAsync().CA();
    }
}