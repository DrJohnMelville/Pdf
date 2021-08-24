﻿using System;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public readonly struct XRefTable
    {
        public XRefTableEntry[] Entries { get; }

        public XRefTable(int length) : this()
        {
            Entries = new XRefTableEntry[length];
        }

        public void DeclareIndirectObject(int objNumber, long offset, long generation = 0) =>
            Entries[objNumber] = XRefTableEntry.IndirectEntry(offset, generation);
        

        public void DeclareObjectStreamObject(
            int objectNumber, int streamObjectNumber, int streamOrdinal) =>
            Entries[objectNumber] = XRefTableEntry.ObjStreamEntry(streamObjectNumber, streamOrdinal);

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
}