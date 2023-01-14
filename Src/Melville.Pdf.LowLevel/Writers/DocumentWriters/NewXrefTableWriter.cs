using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters;

internal static class NewXrefTableWriter
{
    /// <summary>
    /// This method writes out an xref table with the offsets indicated.
    /// this method mutates the refs parameter.
    /// </summary>
    public static ValueTask<FlushResult> WriteXrefsForNewFile(PipeWriter target, in XRefTable refs)
    {
        XrefTableElementWriter.WriteXrefTitleLine(target);
        refs.AssembleFreeList();
        XrefTableElementWriter.WriteTableHeader(target, 0, refs.Entries.Length);
        WriteAllTableItems(target, refs);
        return target.FlushAsync();
    }

    private static void WriteAllTableItems(PipeWriter target, in XRefTable refs)
    {
        foreach (var item in refs.Entries)
        {
            item.WriteXrefTableLine(target);
        }
    }
}