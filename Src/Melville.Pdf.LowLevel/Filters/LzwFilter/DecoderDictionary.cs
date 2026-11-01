using Melville.Parsing.StreamFilters.AsciiHex;
using System;
using System.Runtime.CompilerServices;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter;


internal class DecoderDictionary
{
    private struct Entry
    {
        public byte Element;
        public short Parent;
    }

    [InlineArray(DictSizeWithExtraSlot + 1 - 256)]
    internal struct DecoderArray
    {
        private Entry element;
    }


    private const int DictSizeWithExtraSlot = LzwConstants.MaxTableSize + 1;

    private DecoderArray entries = new ();
    private int nextEntry;
        
    public DecoderDictionary() => Reset();

    public void Reset() => nextEntry = LzwConstants.EndOfFileCode + 1;

    private Entry GetEntry(int index) =>
        (index > 255) ? 
           entries[index - 256] :
           new Entry() { Element = (byte)index, Parent = -1 };
    
    private ref Entry WritableEntry(int index)
    {
        if ( index > 255)
            return ref entries[index - 256];
        throw new InvalidOperationException("Cannot overwrite the default entries");
    }

    public int WriteChars(int entryIndex, ref Span<byte> destination, int lastWrittenIndex)
    {
        if (entryIndex <= lastWrittenIndex) return entryIndex;
        
        var entry = GetEntry(entryIndex);
        var ret = WriteChars(entry.Parent, ref destination, lastWrittenIndex);
        if (destination.IsEmpty) return ret;
        
        destination.SetAndIncrement(entry.Element);
        return entryIndex;
    }

    public short AddChild(short parent, byte character)
    {
        var index = NextDictionaryPosition();
        AddChild(ref WritableEntry(index), parent, character);
        return index;
    }

    private short NextDictionaryPosition()
    {
        // the generator can never generate a code >= maxtable size so we do not need to store
        // more than the one extra code.
        // We have to store one extra value because we write codes to the output by putting them
        // in the dictionary and then returning the index to write.
        // We immediately write the implied code to the output stream, and then we can reuse
        // the top slot.
        return (short)Math.Min(nextEntry++, LzwConstants.MaxTableSize);
    }

    private void AddChild(ref Entry entry, short parentIndex, byte character)
    {
        entry.Parent = parentIndex;
        entry.Element = character;
    }

    public bool IsDefined(int index) => index < nextEntry && index < LzwConstants.MaxTableSize;

    public byte FirstChar(short index)
    {
        while (index > 255) index = GetEntry(index).Parent;
        return (byte)index;
    }
}