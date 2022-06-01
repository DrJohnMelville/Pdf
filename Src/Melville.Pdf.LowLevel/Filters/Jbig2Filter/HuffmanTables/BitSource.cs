using System.Buffers;
using System.IO;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

public ref struct BitSource
{
    public SequenceReader<byte> Source;
    private BitReader reader;

    public BitSource(SequenceReader<byte> source, BitReader? reader = null) : this()
    {
        Source = source;
        this.reader = reader ?? new BitReader();
    }

    public void AddToPattern(ref int pattern, ref int patternLen)
    {
        patternLen++;
        pattern <<= 1;
        pattern |= ReadInt(1);
    }

    public int ReadInt(int bitLength) => 
        reader.TryRead(bitLength, ref Source)??throw new InvalidDataException("No more input");
}