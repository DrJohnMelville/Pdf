using System;
using System.IO.Pipelines;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public static class XrefTableWriter
    {
        private static readonly byte[] xrefHeader = new byte[]{120, 114, 101, 102, 13, 10}; // xref\r\n
        /// <summary>
        /// This method writes out an xref table with the offsets indicated.
        /// this method mutates the refs parameter.
        /// </summary>
        public static ValueTask<FlushResult> WriteXrefTable(PipeWriter target, long[] refs)
        {
            target.WriteBytes(xrefHeader);
            AssembleFreeListEntries(refs);
            WriteTableHeader(target, refs.Length);
            WriteAllTableItems(target, refs);
            return target.FlushAsync();
        }

        private static void WriteAllTableItems(PipeWriter target, long[] refs)
        {
            int itemNumber = 0;
            foreach (var item in refs)
            {
                WriteTableEntry(target, item, itemNumber++);
            }
        }

        private static void AssembleFreeListEntries(long[] refs)
        {
            long lastFree = 0;
            for (int i = refs.Length - 1; i >= 0; i--)
            {
                if (IsUnusedEntry(refs, i))
                {
                    refs[i] = lastFree;
                    lastFree = -i;
                }
            }
        }

        private static bool IsUnusedEntry(long[] refs, int i) => refs[i] == 0;

        private static void WriteTableHeader(PipeWriter target, int countOfEntries)
        {
            var span = target.GetSpan(25);
            var position = IntegerWriter.Write(span, 0);
            span[position++] = 32;
            position += IntegerWriter.Write(span.Slice(position), countOfEntries);
            span[position++] = 10;
            target.Advance(position);
        }

        private static void WriteTableEntry(PipeWriter target, long item, int itemNumber)
        {
            var buffer = target.GetSpan(20);
            IntegerWriter.WriteFixedWidthPositiveNumber(buffer, Math.Abs(item), 10);
            buffer[10] = 32;
            IntegerWriter.WriteFixedWidthPositiveNumber(buffer.Slice(11),
                ItemGenerationNumber(itemNumber),5);
            buffer[16] = 32;
            buffer[17] = item > 0 ? (byte) 'n' : (byte) 'f';
            buffer[18] = 13;
            buffer[19] = 10;
            target.Advance(20);
        }

        //For new files the only non zero generation object is the head of the
        // free list at position zero.  This may need to change when we start to
        // implement file updates, which may include nonzero generations 
        private static int ItemGenerationNumber(int item) => item == 0?65535:0;
    }
}