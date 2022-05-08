namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class SymbolDictionarySegment : Segment
{
    private readonly SymbolDictionaryFlags flags;
    private readonly uint exportedSymbols;
    private readonly uint newSymbols;
    
    public SymbolDictionarySegment(uint number, SymbolDictionaryFlags flags, uint exportedSymbols, uint newSymbols) : base(SegmentType.SymbolDictionary, number)
    {
        this.flags = flags;
        this.exportedSymbols = exportedSymbols;
        this.newSymbols = newSymbols;
    }
}