using System.IO;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.ComparingReader.REPLs;

public readonly struct ReplViewModelFactory
{
    private readonly IMultiRenderer renderer;
    private readonly IPageSelector pageSel;

    public ReplViewModelFactory(IMultiRenderer renderer, IPageSelector pageSel)
    {
        this.renderer = renderer;
        this.pageSel = pageSel;
    }

    public async ValueTask<ReplViewModel> Create(CrossReference? crossReference)
    {
        var srcReader = renderer.GetCurrentTargetReader();
        var buffer = new byte[srcReader.Length];
        await buffer.FillBufferAsync(0, (int)srcReader.Length, srcReader);
        var doc = await PdfDocument.ReadAsync(new MemoryStream(buffer), new NullPasswordSource());

        if (crossReference.HasValue && doc.LowLevel.Objects.TryGetValue(
                (crossReference.Value.Object, crossReference.Value.Generation), out var indir) &&
            await indir.DirectValueAsync() is PdfStream stream)
        {
            var content = await new StreamReader(await stream.StreamContentAsync()).ReadToEndAsync();
            return new ReplViewModel(content, renderer, buffer, indir, pageSel);
        }
        return await CreateFromCurrentPage(doc, buffer);
    }

    private async Task<ReplViewModel> CreateFromCurrentPage(PdfDocument doc, byte[] buffer)
    {
        var page = await (await doc.PagesAsync()).GetPageAsync(pageSel.Page - 1);
        var content = (PdfIndirectReference)page.LowLevel.RawItems[KnownNames.Contents];
        var replContent = await new StreamReader(await page.GetContentBytes()).ReadToEndAsync();
        return new(replContent, renderer, buffer, content, pageSel);
    }
}