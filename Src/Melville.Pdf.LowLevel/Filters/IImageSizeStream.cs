using System.IO;

namespace Melville.Pdf.LowLevel.Filters;

internal interface IImageSizeStream
{
    int Width { get; }
    int Height { get; }
    int ImageComponents { get; }
    int BitsPerComponent { get; }
}