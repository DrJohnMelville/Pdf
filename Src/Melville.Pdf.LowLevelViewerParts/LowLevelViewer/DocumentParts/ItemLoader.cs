using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Melville.MVVM.WaitingServices;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public class ItemLoader : DocumentPart
{
    private readonly Memory<PdfIndirectValue> references;
    private static readonly DocumentPart[] fakeContent = {
        new DocumentPart("Fake--this should never be seen")
    };

    private readonly int minObjectNumber;
    private readonly int maxObjectNumber;
    private static string ComputeName(Memory<PdfIndirectValue> mem)
    {
        var span = mem.Span;
        return $"{span[0].Memento.UInt64s[0]} ... {span[^1].Memento.UInt64s[0]}";
    }

    public ItemLoader(Memory<PdfIndirectValue> references) :
        base(ComputeName(references),fakeContent)
    {
        this.references = references;
        var span = references.Span;
        minObjectNumber = (int)span[0].Memento.UInt64s[0];
        maxObjectNumber = (int)span[^1].Memento.UInt64s[1];
    }

    public override bool CanSkipSearch(int objectNumber) =>
        objectNumber < minObjectNumber || objectNumber > maxObjectNumber;


    public override async ValueTask TryFillTreeAsync(IWaitingService waiting)
    {
        if (Children != fakeContent) return;
        Children = await GetItemsAsync(waiting);
    }

    private async ValueTask<DocumentPart[]> GetItemsAsync(IWaitingService waiting)
    {
        var ret = new DocumentPart[references.Length];
        await FillMemoryWithPartsAsync(waiting, ret);
        return ret;
    }

    public async ValueTask FillMemoryWithPartsAsync(
        IWaitingService waiting, Memory<DocumentPart> ret)
    {
        var generator = new ViewModelVisitor();
        using var waitHandle = waiting.WaitBlock("Loading File", references.Length);
        for (int i = 0; i < references.Length; i++)
        {
            var item = GetAt(references, i);
            waiting.MakeProgress($"Loading Object ({item.Memento.UInt64s[0]}, {item.Memento.UInt64s[1]})");
            SetElement(ret, i, await generator.VisitTopLevelObject(item));
        }
    }

    private PdfIndirectValue GetAt(Memory<PdfIndirectValue> memory, int i) => memory.Span[i];

    private void SetElement(in Memory<DocumentPart> mem, int pos, DocumentPart item) => 
        mem.Span[pos] = item;
}