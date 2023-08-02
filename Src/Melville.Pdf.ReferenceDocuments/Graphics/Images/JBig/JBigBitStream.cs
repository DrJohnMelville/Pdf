using Melville.JBig2.JBigSorters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

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
    private PdfValueStream? image;
    protected override async ValueTask AddContentToDocumentAsync(PdfDocumentCreator docCreator)
    {
        MemoryStream global = new();
        MemoryStream specific = new();
        
        var sorter = new JBigSorter(SourceBits(), global, specific,page); 
        sorter.Sort();
        global.Seek(0, SeekOrigin.Begin);
        specific.Seek(0, SeekOrigin.Begin);
        
        image = new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceGrayTName)
            .WithItem(KnownNames.WidthTName, width)
            .WithItem(KnownNames.HeightTName, height)
            .WithItem(KnownNames.BitsPerComponentTName, 1)
            .WithItem(KnownNames.DecodeParmsTName, new PdfValueArray(
                new ValueDictionaryBuilder()
                    .WithItem(KnownNames.JBIG2GlobalsTName,
                        docCreator.LowLevelCreator.Add(
                            new ValueDictionaryBuilder().AsStream(global, StreamFormat.DiskRepresentation)))
                    .AsDictionary()
            ))
            .WithFilter(FilterName.JBIG2Decode)
            .AsStream(specific, StreamFormat.DiskRepresentation);
        await base.AddContentToDocumentAsync(docCreator);
    }

    protected abstract byte[] SourceBits();

    protected override PdfValueStream CreateImage() => image!;
}