using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

public readonly struct BufferFromPipe
{
    private readonly PipeReader reader;
    private readonly ReadResult result;
    private readonly SequencePosition startAt;

    public static async ValueTask<BufferFromPipe> Create(PipeReader reader)
    {
        var readResult = await reader.ReadAsync();
        return new BufferFromPipe(reader, readResult, readResult.Buffer.Start);
    }

    private BufferFromPipe(PipeReader reader, ReadResult result, SequencePosition startAt)
    {
        this.reader = reader;
        this.result = result;
        this.startAt = startAt;
    }

    public bool Done => result.IsCompleted;
    public SequenceReader<byte> CreateReader() => new (result.Buffer.Slice(startAt));
    public void Consume(SequencePosition pos) => reader.AdvanceTo(pos);

    public bool NeedMoreBytes()
    {
        reader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
        return false;
    }

    public BufferFromPipe WithStartingPosition(SequencePosition sp) => new(reader, result, sp);

    public bool LogSuccess(bool succeed, ref SequenceReader<byte> reader)
    {
        if (succeed)
        {
            Consume(reader.Position);
            return true;
        }
        NeedMoreBytes();
        return false;
    }
}