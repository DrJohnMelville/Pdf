namespace Melville.JBig2.BinaryBitmaps;

public ref struct OffsetReader
{
    private readonly int readOffset;
    private int buffer;

    public OffsetReader(int offset, int buffer = 0 ) : this()
    {
        readOffset = (byte)((offset == 0)?0:(8 - offset));
        this.buffer = buffer;
    }

    public unsafe void Initialize(scoped ref byte* src)
    {
        //rejected an alternate implementation where we let readoffser = 8 is rejected because that
        // would read one extra byte off the end of the array.
        if (readOffset > 0) ReadBye(ref src);
    }

    public unsafe byte ReadBye(scoped ref byte* src)
    {
        buffer = (buffer << 8) | *src++;
        return (byte)(buffer >> readOffset);
    }
}