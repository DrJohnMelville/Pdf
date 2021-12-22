using System.Buffers;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public interface IByteWriter
{
    unsafe void WriteBytes(ref SequenceReader<byte> input, ref byte* output, 
         byte* nextPos);
    int MinimumInputSize { get; }
}


public abstract class ByteWriter: IByteWriter
{
    protected int MaxValue { get; }
    private readonly IComponentWriter componentWriter;

    protected ByteWriter(int maxValue, IComponentWriter componentWriter)
    {
        MaxValue = maxValue;
        this.componentWriter = componentWriter;
    }
    
    public abstract unsafe void WriteBytes(
        ref SequenceReader<byte> input, ref byte* output, byte* nextPos);

    protected unsafe void PushComponent(
        ref byte* output, int numerator) => componentWriter.WriteComponent(ref output, numerator);

    public abstract int MinimumInputSize { get; }
}