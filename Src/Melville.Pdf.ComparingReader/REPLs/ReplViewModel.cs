using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.INPC;
using Melville.Parsing.Streams;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.ComparingReader.REPLs;

public class ReplViewModelFactory
{
    private readonly IMultiRenderer renderer;
    private readonly IPageSelector pageSel;

    public ReplViewModelFactory(IMultiRenderer renderer, IPageSelector pageSel)
    {
        this.renderer = renderer;
        this.pageSel = pageSel;
    }

    public async ValueTask<ReplViewModel> Create()
    {
        var srcReader = renderer.GetCurrentTargetReader();
        var buffer = new byte[srcReader.Length];
        await buffer.FillBufferAsync(0, (int)srcReader.Length, srcReader);
        var doc = await PdfDocument.ReadAsync(new MemoryStream(buffer), new NullPasswordSource());
        var page = await (await doc.PagesAsync()).GetPageAsync(pageSel.Page - 1);

        var content = (PdfIndirectReference)page.LowLevel.RawItems[KnownNames.Contents];
        
        return new(await new StreamReader(await page.GetContentBytes()).ReadToEndAsync(), renderer, buffer, 
            content, pageSel.Page);
    }
}

public partial class ReplViewModel
{
    private readonly IMultiRenderer renderer;
    private readonly byte[] buffer;
    private readonly PdfIndirectReference contentStream;
    private int page;
    [AutoNotify] private string contentStreamText;
    
    public ReplViewModel(
        string contentStreamText, IMultiRenderer renderer, byte[] buffer, PdfIndirectReference contentStream, int page)
    {
        this.contentStreamText = contentStreamText;
        this.renderer = renderer;
        this.buffer = buffer;
        this.contentStream = contentStream;
        this.page = page;
    }

    private async void OnContentStreamTextChanged(string newValue)
    {
        var target = new MultiBufferStream();
        await target.WriteAsync(buffer.AsMemory());
        var doc = await RandomAccessFileParser.Parse(new MemoryStream(buffer));
        
        var modifier = new LowLevelDocumentModifier(doc);
        modifier.AssignValueToReference(contentStream, new DictionaryBuilder().AsStream(newValue));
        await modifier.WriteModificationTrailer(target);
        
        renderer.SetTarget(target, page);
    }
}