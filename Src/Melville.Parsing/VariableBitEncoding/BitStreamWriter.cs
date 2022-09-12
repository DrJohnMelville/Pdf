using System.IO.Pipelines;
using Melville.Hacks;

namespace Melville.Parsing.VariableBitEncoding;

public readonly struct BitStreamWriter
{
    private readonly PipeWriter pipe;
    private readonly BitWriter writer;
    private readonly int bits;

    public BitStreamWriter(Stream destination, int bits)
    {
        this.bits = bits;
        pipe = PipeWriter.Create(destination);
        writer = new BitWriter();
    }

    public void Write(uint datum)
    {
        var span = pipe.GetSpan(5);
        pipe.Advance(writer.WriteBits(datum, bits, span));
    }
        
    public ValueTask FinishAsync()
    {
        var span = pipe.GetSpan(1);
        pipe.Advance(writer.FinishWrite(span));
        return pipe.FlushAsync().AsValueTask();
    }
}