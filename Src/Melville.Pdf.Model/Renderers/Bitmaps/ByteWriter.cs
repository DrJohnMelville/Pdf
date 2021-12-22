using System.Buffers;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;

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
    private readonly int[] components;
    private int currentComponent;

    protected ByteWriter(int maxValue, IComponentWriter componentWriter)
    {
        MaxValue = maxValue;
        this.componentWriter = componentWriter;
        components = new int[componentWriter.ColorComponentCount];
    }
    
    public abstract unsafe void WriteBytes(
        ref SequenceReader<byte> input, ref byte* output, byte* nextPos);

    protected unsafe void PushComponent(ref byte* output, int numerator)
    {
        components[currentComponent++] = numerator;
        if (currentComponent >= components.Length)
        {
            componentWriter.WriteComponent(ref output, components);
            currentComponent = 0;
        }
    }

    public abstract int MinimumInputSize { get; }
}