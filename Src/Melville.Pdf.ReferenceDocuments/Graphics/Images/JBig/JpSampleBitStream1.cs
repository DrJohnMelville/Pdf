using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.ReferenceDocuments.Utility;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig;

public class JBigSampleBitStream1 : JBigSampleBitStream
{
    public JBigSampleBitStream1() : base(1,64,56)
    {
    }
}
public class JBigSampleBitStream2 : JBigSampleBitStream
{
    public JBigSampleBitStream2() : base(2,64,56)
    {
    }
}

public abstract class JBigSampleBitStream: DisplayImageTest
{
    private readonly int page;
    private readonly int width;
    private readonly int height;

    protected JBigSampleBitStream(int page, int width, int height) : base(@"Page {page} of the JBIG sample bitstream")
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
        
        var sorter = new JBigSorter(SourceBits, global, specific,page); 
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

    protected override PdfStream CreateImage()
    {
        return image!;
    }
    
     private static byte[] SourceBits = @"
                                         97 4A 42 32 0D 0A 1A 0A 01 00 00 00 03 00 00 00
                                         00 00 01 00 00 00 00 18 00 01 00 00 00 01 00 00
                                         00 01 E9 CB F4 00 26 AF 04 BF F0 78 2F E0 00 40
                                         00 00 00 01 30 00 01 00 00 00 13 00 00 00 40 00
                                         00 00 38 00 00 00 00 00 00 00 00 01 00 00 00 00
                                         00 02 00 01 01 00 00 00 1C 00 01 00 00 00 02 00
                                         00 00 02 E5 CD F8 00 79 E0 84 10 81 F0 82 10 86
                                         10 79 F0 00 80 00 00 00 03 07 42 00 02 01 00 00
                                         00 31 00 00 00 25 00 00 00 08 00 00 00 04 00 00
                                         00 01 00 0C 09 00 10 00 00 00 05 01 10 00 00 00
                                         00 00 00 00 00 00 00 00 00 00 00 00 0C 40 07 08
                                         70 41 D0 00 00 00 04 27 00 01 00 00 00 2C 00 00
                                         00 36 00 00 00 2C 00 00 00 04 00 00 00 0B 00 01
                                         26 A0 71 CE A7 FF FF FF FF FF FF FF FF FF FF FF
                                         FF FF FF FF FF FF FF FF F8 F0 00 00 00 05 10 01
                                         01 00 00 00 2D 01 04 04 00 00 00 0F 20 D1 84 61
                                         18 45 F2 F9 7C 8F 11 C3 9E 45 F2 F9 7D 42 85 0A
                                         AA 84 62 2F EE EC 44 62 22 35 2A 0A 83 B9 DC EE
                                         77 80 00 00 00 06 17 20 05 01 00 00 00 57 00 00
                                         00 20 00 00 00 24 00 00 00 10 00 00 00 0F 00 01
                                         00 00 00 08 00 00 00 09 00 00 00 00 00 00 00 00
                                         04 00 00 00 AA AA AA AA 80 08 00 80 36 D5 55 6B
                                         5A D4 00 40 04 2E E9 52 D2 D2 D2 8A A5 4A 00 20
                                         02 23 E0 95 24 B4 92 8A 4A 92 54 92 D2 4A 29 2A
                                         49 40 04 00 40 00 00 00 07 31 00 01 00 00 00 00
                                         00 00 00 08 30 00 02 00 00 00 13 00 00 00 40 00
                                         00 00 38 00 00 00 00 00 00 00 00 01 00 00 00 00
                                         00 09 00 01 02 00 00 00 1B 08 00 02 FF 00 00 00
                                         02 00 00 00 02 4F E7 8C 20 0E 1D C7 CF 01 11 C4
                                         B2 6F FF AC 00 00 00 0A 07 40 00 09 02 00 00 00
                                         1F 00 00 00 25 00 00 00 08 00 00 00 04 00 00 00
                                         01 00 0C 08 00 00 00 05 8D 6E 5A 12 40 85 FF AC
                                         00 00 00 0B 27 00 02 00 00 00 23 00 00 00 36 00
                                         00 00 2C 00 00 00 04 00 00 00 0B 00 08 03 FF FD
                                         FF 02 FE FE FE 04 EE ED 87 FB CB 2B FF AC 00 00
                                         00 0C 10 01 02 00 00 00 1C 06 04 04 00 00 00 0F
                                         90 71 6B 6D 99 A7 AA 49 7D F2 E5 48 1F DC 68 BC
                                         6E 40 BB FF AC 00 00 00 0D 17 20 0C 02 00 00 00
                                         3E 00 00 00 20 00 00 00 24 00 00 00 10 00 00 00
                                         0F 00 02 00 00 00 08 00 00 00 09 00 00 00 00 00
                                         00 00 00 04 00 00 00 87 CB 82 1E 66 A4 14 EB 3C
                                         4A 15 FA CC D6 F3 B1 6F 4C ED BF A7 BF FF AC 00
                                         00 00 0E 31 00 02 00 00 00 00 00 00 00 0F 30 00
                                         03 00 00 00 13 00 00 00 25 00 00 00 08 00 00 00
                                         00 00 00 00 00 01 00 00 00 00 00 10 00 01 00 00
                                         00 00 16 08 00 02 FF 00 00 00 01 00 00 00 01 4F
                                         E7 8D 68 1B 14 2F 3F FF AC 00 00 00 11 00 21 10
                                         03 00 00 00 20 08 02 02 FF FF FF FF FF 00 00 00
                                         03 00 00 00 02 4F E9 D7 D5 90 C3 B5 26 A7 FB 6D
                                         14 98 3F FF AC 00 00 00 12 07 20 11 03 00 00 00
                                         25 00 00 00 25 00 00 00 08 00 00 00 00 00 00 00
                                         00 00 8C 12 00 00 00 04 A9 5C 8B F4 C3 7D 96 6A
                                         28 E5 76 8F FF AC 00 00 00 13 31 00 03 00 00 00
                                         00 00 00 00 14 33 00 00 00 00 00 00".BitsFromHex();
}