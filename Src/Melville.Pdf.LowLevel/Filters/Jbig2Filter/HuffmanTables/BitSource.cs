using System.Buffers;
using System.IO;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

public ref struct BitSource
{
    public SequenceReader<byte> Source;
    public readonly BitReader Reader;

    public BitSource(SequenceReader<byte> source, BitReader? reader = null) : this()
    {
        Source = source;
        this.Reader = reader ?? new BitReader();
    }

    public int ReadInt(int bitLength) => Reader.ForceRead(bitLength, ref Source);
}