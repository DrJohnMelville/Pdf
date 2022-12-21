using Melville.JBig2.JBigSorters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig;

public abstract class JBigBitStream: DisplayImageTest
{
    private readonly int page;
    private readonly int width;
    private readonly int height;

    protected JBigBitStream(string name, int page, int width, int height) : base(name)
    {
        this.page = page;
        this.width = width;
        this.height = height;
    }
    private PdfStream? image;
    protected override async ValueTask AddContentToDocumentAsync(PdfDocumentCreator docCreator)
    {
        MemoryStream global = new();
        MemoryStream specific = new();
        
        var sorter = new JBigSorter(SourceBits(), global, specific,page); 
        sorter.Sort();
        global.Seek(0, SeekOrigin.Begin);
        specific.Seek(0, SeekOrigin.Begin);
        
        image = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceGray)
            .WithItem(KnownNames.Width, new PdfInteger(width))
            .WithItem(KnownNames.Height, new PdfInteger(height))
            .WithItem(KnownNames.BitsPerComponent, new PdfInteger(1))
            .WithItem(KnownNames.DecodeParms, new PdfArray(
                new DictionaryBuilder()
                    .WithItem(KnownNames.JBIG2Globals,
                        docCreator.LowLevelCreator.Add(
                            new DictionaryBuilder().AsStream(global, StreamFormat.DiskRepresentation)))
                    .AsDictionary()
            ))
            .WithFilter(FilterName.JBIG2Decode)
            .AsStream(specific, StreamFormat.DiskRepresentation);
        await base.AddContentToDocumentAsync(docCreator);
    }

    protected abstract byte[] SourceBits();

    protected override PdfStream CreateImage() => image!;
}