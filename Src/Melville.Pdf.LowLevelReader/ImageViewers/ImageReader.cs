using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Wpf.Rendering;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Melville.Pdf.LowLevelReader.ImageViewers;

public static class ImageReader
{
    public static ValueTask<ImageSource> ReadJpeg(Stream s)
    {
        var dec = new JpegBitmapDecoder(s, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.None);
        return new(dec.Frames[0]);
    }

}