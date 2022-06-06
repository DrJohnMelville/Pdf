namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public ref struct OffsetReader
{
    private readonly byte readOffset;
    private ushort buffer;

    public OffsetReader(byte offset, ushort buffer = 0 ) : this()
    {
        readOffset = (byte)((offset == 0)?0:(8 - offset));
        this.buffer = buffer;
    }

    public unsafe void Initialize(ref byte* src)
    {
        //rejected an alternate explanation where we let readoffser = 8 is rejected because that
        // would read one extra byte off the end of the array.
        if (readOffset > 0) buffer = (ushort)(*src++);
    }

    public unsafe byte ReadBye(ref byte* src)
    {
        buffer <<= 8;
        buffer |= *src++;
        return (byte)(buffer >> readOffset);
    }
}