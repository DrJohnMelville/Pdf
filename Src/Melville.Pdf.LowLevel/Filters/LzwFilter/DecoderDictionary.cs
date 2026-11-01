using Melville.Parsing.StreamFilters.AsciiHex;
using System;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter;

internal class DecoderDictionary
{
    private struct Entry
    {
        public byte Element;
        public short Parent;
#warning I think I can get rid of this field.
        public short StringIndex;
    }

    private const int DictSizeWithExtraSlot = LzwConstants.MaxTableSize + 1;

    private readonly Entry[] entries = new Entry[DictSizeWithExtraSlot+1];
    private int nextEntry;
        
    public DecoderDictionary()
    {
        SetupDefaultDictionary();
    }

#warning I do not need to store the first 255 entries, because I can always compute them
    private void SetupDefaultDictionary()
    {
        for (int i = 0; i < 256; i++)
        {
            entries[i].Parent = -1;
            entries[i].Element = (byte) i;
        }
        nextEntry = LzwConstants.EndOfFileCode + 1;
    }

    public void Reset()
    {
        Array.Clear(entries, 0, entries.Length);
        SetupDefaultDictionary();
    }

    public int WriteChars(int entryIndex, ref Span<byte> destination, int lastWrittenIndex)
    {
        if (entryIndex <= lastWrittenIndex) return entryIndex;
        var entry = entries[entryIndex];
        var ret = WriteChars(entry.Parent, ref destination, lastWrittenIndex);
        if (destination.IsEmpty) return ret;
        destination.SetAndIncrement(entry.Element);
        return entryIndex;
    }

    public short AddChild(short parent, byte character)
    {
        var index = NextDictionaryPosition();
        AddChild(ref entries[index], parent, character);
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
        entry.StringIndex = (short)(1 + entries[parentIndex].StringIndex);
    }

    public bool IsDefined(int index) => index < nextEntry && index < LzwConstants.MaxTableSize;

    public byte FirstChar(short index)
    {
        while (index > 255) index = entries[index].Parent;
        return (byte)index;
    }
}