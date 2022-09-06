using System;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.Streams;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.ComparingReader.Viewers.LowLevel;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
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
        var doc = await new PdfLowLevelReader().ReadFrom(buffer);
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
}