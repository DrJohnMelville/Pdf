using System.IO.Pipelines;
using Melville.Hacks;
using Melville.Hacks.Reflection;

namespace Melville.Parsing.VariableBitEncoding;

/// <summary>
/// Pack bits into bytes and write to a stream and 
/// </summary>
public readonly struct BitStreamWriter
{
    private readonly PipeWriter pipe;
    private readonly BitWritingBuffer writer = new();
    private readonly int bits;

    /// <summary>
    /// Create a BitStreamWriter
    /// </summary>
    /// <param name="destination">The pipewriter to write to.</param>
    /// <param name="bits">Number of bits per number to write.</param>
    public BitStreamWriter(Stream destination, int bits)
    {
        this.bits = bits;
        pipe = PipeWriter.Create(destination);
    }

    /// <summary>
    /// Write the number to the stream witih the given number of bits.
    /// </summary>
    /// <param name="datum">The number to write.</param>
    public void Write(uint datum)
    {
        writer.WriteBits(datum, bits);
        AddToPipe();
    }

    private void AddToPipe()
    {
        var span = pipe.GetSpan(5);
        var end = span;
        writer.TryWriteToMovingBuffer(ref end);
        pipe.Advance(span.Length - end.Length);
    }

    /// <summary>
    /// Flush the remaining bits to the output stream
    /// </summary>
    public ValueTask FinishAsync()
    {
        writer.AdvanceToWholeByte();
        AddToPipe();
        return pipe.FlushAsync().AsValueTask();
    }
}