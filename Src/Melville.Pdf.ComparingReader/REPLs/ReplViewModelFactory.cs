using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
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

    public async ValueTask<ReplViewModel> CreateAsync(CrossReference? crossReference)
    {
        var srcReader = renderer.GetCurrentTargetReader();
        var buffer = new byte[srcReader.Length];
        await buffer.FillBufferAsync(0, (int)srcReader.Length, srcReader);
        var doc = await PdfDocument.ReadAsync(new MemoryStream(buffer));

        if (crossReference.HasValue && doc.LowLevel.Objects.TryGetValue(
                (crossReference.Value.Object, crossReference.Value.Generation), out var indir) &&
            (await indir.LoadValueAsync()).TryGet(out PdfValueStream stream))
        {
            var text = await ReadContentStringAsync(stream);
            return new ReplViewModel(text, renderer, buffer, indir, pageSel);
        }
        return await CreateFromCurrentPageAsync(doc, buffer);
    }

    private static async Task<string> ReadContentStringAsync(PdfValueStream stream)
    {
        await using var source = await stream.StreamContentAsync();
        return await ReadContentStringAsync(source);
    }

    private static async Task<string> ReadContentStringAsync(Stream source)
    {
        var sb = new StringBuilder();
        var strBuffer = ArrayPool<byte>.Shared.Rent(4096);
        int len = 0;
        do
        {
            len = await source.ReadAsync(strBuffer.AsMemory());
            sb.Append(ExtendedAsciiEncoding.ExtendedAsciiString(strBuffer.AsSpan(0, len)));
        } while (len > 0);

        ArrayPool<byte>.Shared.Return(strBuffer);
        var text = sb.ToString();
        return text;
    }

    private async Task<ReplViewModel> CreateFromCurrentPageAsync(PdfDocument doc, byte[] buffer)
    {
        var page = await (await doc.PagesAsync()).GetPageAsync(pageSel.Page - 1);
        var content = page.LowLevel.RawItems[KnownNames.ContentsTName];
        var replContent = await ReadContentStringAsync(await page.GetContentBytesAsync());
        return new(replContent, renderer, buffer, content, pageSel);
    }
}