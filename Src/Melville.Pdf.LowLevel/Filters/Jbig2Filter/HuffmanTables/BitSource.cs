using System.Buffers;
using System.IO;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

public ref struct BitSource
{
    public SequenceReader<byte> Source;
    private BitReader reader = new();

    public BitSource(SequenceReader<byte> source) : this()
    {
        Source = source;
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