using System;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter;

internal class DecoderDictionary
{
    private struct Entry
    {
        public byte Element;
        public short Parent;
        public short StringIndex;
    }

    private const int DictSizeWithExtraSlot = LzwConstants.MaxTableSize + 1;

    private readonly Entry[] entries = new Entry[DictSizeWithExtraSlot+1];
    private int nextEntry;
        
    public DecoderDictionary()
    {
        SetupDefaultDictionary();
    }

    private void SetupDefaultDictionary()
    {
        for (int i = 0; i < 256; i++)
        {
            entries[i].Element = (byte) i;
        }
        nextEntry = LzwConstants.EndOfFileCode + 1;
    }

    public void Reset()
    {
        Array.Clear(entries, 0, entries.Length);
        SetupDefaultDictionary();
    }

    public int WriteChars(int entryIndex, int firstToWrite, in Span<byte> destination) => 
        WriteChars(ref entries[entryIndex], firstToWrite, destination);

    private int WriteChars(
        ref Entry entry, int firstToWrite, in Span<byte> destination)
    {
        int destNext = 0;
        if (firstToWrite > entry.StringIndex) return 0;
        if (firstToWrite < entry.StringIndex)
        {
            destNext =
                WriteChars(ref entries[entry.Parent], firstToWrite, destination);
        }

        if (destNext >= destination.Length) return destNext;

        destination[destNext] = entry.Element;
        return destNext + 1;
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

    public bool IsDefined(int index) => index < 256 || entries[index].StringIndex > 0;

    public byte FirstChar(short index) => index < 256 ? (byte)index : FirstChar(entries[index].Parent);
}