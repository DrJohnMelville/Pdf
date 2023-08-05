using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters;

internal class XRefTable: IInternalObjectTarget
{
    private readonly int extraSlots;
    private XRefTableEntry[] entries;
    public XRefTableEntry[] Entries => entries;

    public XRefTable(int maxObject, int extraSlots)
    {
        this.extraSlots = extraSlots;
        entries = new XRefTableEntry[ArraySizeForMaxObject(maxObject)];
    }

    public void DeclareIndirectObject(int objNumber, long offset, long generation = 0) =>
        StoreObjectEntry(objNumber, XRefTableEntry.IndirectEntry(offset, generation));

    public ValueTask DeclareObjectStreamObjectAsync(
        int objectNumber, int streamObjectNumber, int streamOrdinal, int streamOffset)
    {
        StoreObjectEntry(objectNumber,
            XRefTableEntry.ObjStreamEntry(streamObjectNumber, streamOrdinal));
        return ValueTask.CompletedTask;
    }

    private void StoreObjectEntry(int objectNumber, XRefTableEntry item)
    {
        EnsureEntriesIsBigEnoughToHoldValue(objectNumber);
        Entries[objectNumber] = item;
    }

    private void EnsureEntriesIsBigEnoughToHoldValue(int objectNumber)
    {
        // this is a ridiculously slow implementation, but this ought to be a fairly rare problem.  Usually
        // an object stream will have a higher object number than the objects inside of it and thus the 
        // default array length will work.
        if (objectNumber >= Entries.Length)
        {
            Array.Resize(ref entries, ArraySizeForMaxObject(objectNumber));
        }
    }

    private  int ArraySizeForMaxObject(int objectNumber) => objectNumber + 1 + extraSlots;

    public void AssembleFreeList()
    {
        long lastFree = 0;
        for (int i = Entries.Length - 1; i >= 0; i--)
        {
            if (Entries[i].IsFreeEntry)
            {
                Entries[i] = XRefTableEntry.FreeEntry(lastFree, 0);
                lastFree = i;
            }
        }
    }
        
    //For new files the only non zero generation object is the head of the
    // free list at position zero.  

    public (int,int,int) ColumnByteWidths()
    {
        long col0 = 0;
        long col1 = 0;
        long col2 = 0;
        foreach (var entry in Entries)
        {
            col0 = Math.Max(col0, entry.Type);
            col1 = Math.Max(col1, entry.Column1);
            col2 = Math.Max(col2, entry.Column2);
        }

        return (NeededBytes(col0), NeededBytes(col1), NeededBytes(col2));
    }

    private int NeededBytes(long value)
    {
        var ret = 0;
        while( value > 0)
        {
            ret++;
            value >>= 8;
        }

        return ret;
    }

}