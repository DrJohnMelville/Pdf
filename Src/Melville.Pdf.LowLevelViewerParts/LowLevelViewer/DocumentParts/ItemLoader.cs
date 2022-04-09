using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Melville.MVVM.WaitingServices;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public class ItemLoader : DocumentPart
{
    private readonly Memory<PdfIndirectReference> references;
    private static readonly DocumentPart[] fakeContent = {
        new DocumentPart("Fake--this should never be seen")
    };

    private readonly int minObjectNumber;
    private readonly int maxObjectNumber;
    private static string ComputeName(Memory<PdfIndirectReference> mem)
    {
        var span = mem.Span;
        return $"{span[0].Target.ObjectNumber} ... {span[^1].Target.ObjectNumber}";
    }

    public ItemLoader(Memory<PdfIndirectReference> references) :
        base(ComputeName(references),fakeContent)
    {
        this.references = references;
        var span = references.Span;
        minObjectNumber = span[0].Target.ObjectNumber;
        maxObjectNumber = span[^1].Target.ObjectNumber;
    }

    public override bool CanSkipSearch(int objectNumber) =>
        objectNumber < minObjectNumber || objectNumber > maxObjectNumber;


    public override async ValueTask TryFillTree(IWaitingService waiting)
    {
        if (Children != fakeContent) return;
        Children = await GetItems(waiting);
    }

    private async ValueTask<DocumentPart[]> GetItems(IWaitingService waiting)
    {
        var ret = new DocumentPart[references.Length];
        await FillMemoryWithParts(waiting, ret);
        return ret;
    }

    public async ValueTask FillMemoryWithParts(
        IWaitingService waiting, Memory<DocumentPart> ret)
    {
        var generator = new ViewModelVisitor();
        using var waitHandle = waiting.WaitBlock("Loading File", references.Length);
        for (int i = 0; i < references.Length; i++)
        {
            var item = GetAt(references, i);
            waiting.MakeProgress($"Loading Object ({item.Target.ObjectNumber}, {item.Target.GenerationNumber})");
            SetElement(ret, i, await item.Target.Visit(generator));
        }
    }

    private PdfIndirectReference GetAt(Memory<PdfIndirectReference> memory, int i) => memory.Span[i];

    private void SetElement(in Memory<DocumentPart> mem, int pos, DocumentPart item) => 
        mem.Span[pos] = item;
}