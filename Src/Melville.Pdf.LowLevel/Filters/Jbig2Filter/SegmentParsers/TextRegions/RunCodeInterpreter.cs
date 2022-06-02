using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;

public ref struct RunCodeInterpreter
{
    private BitSource source;
    public BitSource Source => source;
    private readonly StructHuffmanTable huffmanTable;
    private int repeatedResult = 0;
    private int remainingRepeats = 0;

    public RunCodeInterpreter(BitSource source, StructHuffmanTable huffmanTable)
    {
        this.source = source;
        this.huffmanTable = huffmanTable;
    }

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
        InterpretCode(huffmanTable.GetInteger(ref source.Source, source.Reader));
    
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
        var repeats = source.ReadInt(bitsToRead) + offset;
        remainingRepeats = repeats - 1; // we are going to return the first repeat from this call.
        return repeatedResult = value;
    }
    private int SaveResult(int argument) => repeatedResult = argument;
}