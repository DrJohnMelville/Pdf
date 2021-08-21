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
        
        public void AssembleFreeList()
        {
            long lastFree = 0;
            for (int i = Entries.Length - 1; i >= 0; i--)
            {
                if (Entries[i].IsFreeEntry)
                {
                    Entries[i] = XRefTableEntry.FreeEntry(lastFree, ItemGenerationNumber(i));
                    lastFree = i;
                }
            }
        }
        
        //For new files the only non zero generation object is the head of the
        // free list at position zero.  
        private static int ItemGenerationNumber(int item) => item == 0?65535:0;
        
        
    }
}