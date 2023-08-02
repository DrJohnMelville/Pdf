using System.Buffers;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal interface IByteWriter
{
    unsafe void WriteBytes(scoped ref SequenceReader<byte> input, scoped ref byte* output, 
         byte* nextPos);
    int MinimumInputSize { get; }
}


internal abstract class ByteWriter: IByteWriter
{
    protected int MaxValue { get; }
    private readonly ComponentWriter componentWriter;
    private readonly int[] components;
    private int currentComponent;

    protected ByteWriter(int maxValue, ComponentWriter componentWriter)
    {
        MaxValue = maxValue;
        this.componentWriter = componentWriter;
        components = new int[componentWriter.ColorComponentCount];
    }
    
    public abstract unsafe void WriteBytes(
        scoped ref SequenceReader<byte> input, scoped ref byte* output, byte* nextPos);

    protected unsafe void PushComponent(ref byte* output, int numerator)
    {
        components[currentComponent++] = numerator;
        if (currentComponent >= components.Length)
        {
            componentWriter.WriteComponent(ref output, components, 255);
            currentComponent = 0;
        }
    }

    public abstract int MinimumInputSize { get; }
}