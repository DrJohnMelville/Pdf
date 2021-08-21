using System;
using System.IO.Pipelines;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public readonly struct XRefTableEntry
    {
        public int Type { get; }
        public long Column1 { get; }
        public long Column2 { get; }

        public bool IsFreeEntry => Type == 0;

        private XRefTableEntry(int type, long column1, long column2)
        {
            Type = type;
            Column1 = column1;
            Column2 = column2;
        }

        public static XRefTableEntry FreeEntry(long next, long generationNumber) =>
            new(0, next, generationNumber);
        public static XRefTableEntry IndirectEntry(long offset, long generationNumber) =>
            new(1, offset, generationNumber);
        public static XRefTableEntry ObjStreamEntry(long streamNum, long streamIndex) =>
            new(2, streamNum, streamIndex);

        public void WriteXrefTableLine(PipeWriter target)
        {
            XrefTableElementWriter.WriteTableEntry(target, Column1, (int)Column2, OldTableEntryType);
        }

        private bool OldTableEntryType => Type switch
        {
            0 => false,
            1 => true,
            _ => throw new InvalidOperationException(
                "Only free objects and indirect objects are allowed in an xref table")
        };
    }
}