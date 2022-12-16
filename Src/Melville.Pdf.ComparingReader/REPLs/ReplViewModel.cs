using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.Parsing.Streams;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.ComparingReader.SavePagesImpl;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.ComparingReader.REPLs;

public partial class ReplViewModel
{
    private readonly IMultiRenderer renderer;
    private readonly byte[] buffer;
    private readonly PdfIndirectObject contentStream;
    readonly IPageSelector page;
    [AutoNotify] private string contentStreamText;
    
    public ReplViewModel(
        string contentStreamText, IMultiRenderer renderer, byte[] buffer, PdfIndirectObject contentStream, 
        IPageSelector page)
    {
        this.contentStreamText = contentStreamText;
        this.renderer = renderer;
        this.buffer = buffer;
        this.contentStream = contentStream;
        this.page = page;
    }

    private async void OnContentStreamTextChanged(string newValue)
    {
        if (buffer.Length == 0) return; // heppens in testing
        var target = await CopyOriginalFile();
        var doc = await new PdfLowLevelReader().ReadFromAsync(buffer);
        await WriteStreamModificationBlock(doc, await CreateReplacementStream(newValue), target);                                                                       
        renderer.SetTarget(target, page.Page);
    }

    private async Task<PdfStream> CreateReplacementStream(string newValue)
    {
        var source = await contentStream.DirectValueAsync();
        var newStream = StreamWithContent(source, newValue);
        return newStream;
    }

    private async Task WriteStreamModificationBlock(PdfLoadedLowLevelDocument doc, PdfStream newStream,
        MultiBufferStream target)
    {
        var modifier = new LowLevelDocumentModifier(doc);
        modifier.AssignValueToReference(contentStream, newStream);
        await modifier.WriteModificationTrailer(target);
    }

    private async Task<MultiBufferStream> CopyOriginalFile()
    {
        var target = new MultiBufferStream();
        await target.WriteAsync(buffer.AsMemory());
        return target;
    }

    private static PdfStream StreamWithContent(PdfObject source, string newValue)
    {
        var builder = new DictionaryBuilder();
        if (source is PdfDictionary sourceDict) builder.CopyFrom(sourceDict);
        return builder.AsStream(newValue);
    }

    public async ValueTask PrettyPrint()
    {
        ContentStreamText = await ContentStreamPrettyPrinter.PrettyPrint(ContentStreamText);
    }

    public async ValueTask SavePage([FromServices]IOpenSaveFile osf)
    {
        var outputFile = osf.GetSaveFile(null, "pdf", "PDF Files (*.pdf)|*.pdf", "Select file to save page to");
        if (outputFile is null) return;
        var doc = new PdfDocument(await new PdfLowLevelReader().ReadFromAsync(buffer));
        var pageObj = await (await doc.PagesAsync()).GetPageAsync(page.Page);
        await using var output = await outputFile.CreateWrite();
        await new PageExtractor(doc, new PdfPage(pageObj.LowLevel), ContentStreamText).WriteAsync(output);
    }
}