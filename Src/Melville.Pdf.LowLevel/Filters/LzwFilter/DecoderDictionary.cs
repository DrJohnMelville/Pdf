using System;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter;

public class DecoderDictionary
{
    private struct Entry
    {
        public byte Element;
        public short Parent;
        public short StringIndex;
    }

    private readonly Entry[] entries = new Entry[LzwConstants.MaxTableSize];
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

    public int WriteChars(int entryIndex, int firstToWrite, ref Span<byte> destination) => 
        WriteChars(ref entries[entryIndex], firstToWrite, ref destination);

    private int WriteChars(
        ref Entry entry, int firstToWrite, ref Span<byte> destination)
    {
        int destNext = 0;
        if (firstToWrite > entry.StringIndex) return 0;
        if (firstToWrite < entry.StringIndex)
        {
            destNext =
                WriteChars(ref entries[entry.Parent], firstToWrite, ref destination);
        }

        if (destNext >= destination.Length) return destNext;

        destination[destNext] = entry.Element;
        return destNext + 1;
    }

    public short AddChild(short parent, byte character)
    {
        var index = (short)nextEntry++;
        AddChild(ref entries[index], parent, character);
        return index;
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