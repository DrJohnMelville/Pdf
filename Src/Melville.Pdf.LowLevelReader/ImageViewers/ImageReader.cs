using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Melville.FileSystem;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Filters.ExternalFilters;
using Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.LowLevelReader.ImageViewers;

public static class ImageReader
{
    public static async ValueTask<ImageSource> ReadJpeg(IFile file)
    {
        var str = await Decoder(file.Extension().ToUpper()).DecodeOnReadStream(await file.OpenRead(), PdfTokenValues.Null);
        var size = str as IImageSizeStream ?? throw new InvalidOperationException("need image size");
        var image = await new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.Height, size.Height)
            .WithItem(KnownNames.Width, size.Width)
            .WithItem(KnownNames.ColorSpace, size.ImageComponents == 1 ?KnownNames.DefaultGray : KnownNames.DeviceRGB)
            .WithItem(KnownNames.BitsPerComponent, size.BitsPerComponent)
            .AsStream(str)
            .WrapForRenderingAsync(new PdfPage(PdfDictionary.Empty), DeviceColor.Black);
        var ret = await image.ToWpfBitmap();
        ret.Freeze();
        return ret;
    }

    private static ICodecDefinition Decoder(string ext) => ext switch
    {
        "JPG" => new DctDecoder(),
        "JP2" or "JPX" => new JpxToPdfAdapter(),
        _=> throw new InvalidOperationException("Unsupported file format") 
    };
}