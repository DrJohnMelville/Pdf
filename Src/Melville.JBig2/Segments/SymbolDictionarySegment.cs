using System;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;

namespace Melville.JBig2.Segments;

public class SymbolDictionarySegment : DictionarySegment
{
    public SymbolDictionarySegment(IBinaryBitmap[] allSymbols) :
        this(allSymbols.AsMemory()){}
    public SymbolDictionarySegment(Memory<IBinaryBitmap> exportedSymbols) : 
        base(SegmentType.SymbolDictionary, exportedSymbols)
    {
    }
}