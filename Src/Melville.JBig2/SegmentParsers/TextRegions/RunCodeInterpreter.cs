using System.Buffers;
using Melville.JBig2.HuffmanTables;
using Melville.Parsing.VariableBitEncoding;

namespace Melville.JBig2.SegmentParsers.TextRegions;

public ref struct RunCodeInterpreter
{
    public SequenceReader<byte> source;
    private BitReader reader;
    private readonly ReadOnlySpan<HuffmanLine> huffmanTable;
    private int repeatedResult = 0;
    private int remainingRepeats = 0;

    public RunCodeInterpreter(in SequenceReader<byte> source, BitReader reader, ReadOnlySpan<HuffmanLine> huffmanTable)
    {
        this.source = source;
        this.reader = reader;
        this.huffmanTable = huffmanTable;
    }

    public ReadOnlySequence<byte> UnexaminedSequence() => source.UnreadSequence;

    public int GetNextCode() => HasRepeatedResultPending() ? 
        ReturnRepeatedResult() : 
        ReadResultFromBitstream();


    private bool HasRepeatedResultPending() => remainingRepeats > 0;

    private int ReturnRepeatedResult()
    {
        remainingRepeats--;
        return repeatedResult;
    }

    private int ReadResultFromBitstream() =>
        InterpretCode( source.ReadHuffmanInt(reader, huffmanTable));
    
    private int InterpretCode(int nextCode) =>
        nextCode switch
        {
            32 => ReturnNResults(2, 3, repeatedResult),
            33 => ReturnNResults(3, 3, 0),
            34 => ReturnNResults(7, 11, 0),
            _ => SaveResult(nextCode)
        };

    private int ReturnNResults(int bitsToRead, int offset, int value)
    {
        var repeats = reader.ForceRead(bitsToRead, ref source) + offset;
        remainingRepeats = repeats - 1; // we are going to return the first repeat from this call.
        return repeatedResult = value;
    }
    private int SaveResult(int argument) => repeatedResult = argument;
}