using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public static class NewXrefTableWriter
    {
        /// <summary>
        /// This method writes out an xref table with the offsets indicated.
        /// this method mutates the refs parameter.
        /// </summary>
        public static ValueTask<FlushResult> WriteXrefsForNewFile(PipeWriter target, long[] refs)
        {
            XrefTableElementWriter.WriteXrefTitleLine(target);
            AssembleFreeListEntries(refs);
            XrefTableElementWriter.WriteTableHeader(target, 0, refs.Length);
            WriteAllTableItems(target, refs);
            return target.FlushAsync();
        }

        private static void WriteAllTableItems(PipeWriter target, long[] refs)
        {
            var itemNumber = 0;
            foreach (var item in refs)
            {
                XrefTableElementWriter.WriteTableEntry(
                    target, Math.Abs(item), ItemGenerationNumber(itemNumber++), item > 0);
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

        //For new files the only non zero generation object is the head of the
        // free list at position zero.  
        private static int ItemGenerationNumber(int item) => item == 0?65535:0;
    }
}