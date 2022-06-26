using System.IO;
using System.Threading.Tasks;
using CSJ2K;
using CSJ2K.j2k.image;
using CSJ2K.j2k.util;
using CSJ2K.Util;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

public class JpxToPdfAdapter: ICodecDefinition
{
    public JpxToPdfAdapter()
    {
        RawImageCreator.Register();
    }

    public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters)
    {
        throw new System.NotSupportedException();
    }

    public ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters)
    {
        var independentImage = J2kImage.FromStream(input);
        var image = independentImage.As<MemoryStream>();
        return new(image);
    }
}

public class RawImage : ImageBase<MemoryStream>
{
    private readonly byte[] bytes;
    private readonly int length;

    public RawImage(int width, int height, byte[] bytes) : base(width, height, bytes)
    {
        this.bytes = bytes;
        var pixelcount = (this.bytes.Length / 4);
        length = pixelcount * 3;
        Collapse(pixelcount);
    }

    private unsafe void Collapse(int pixelcount)
    {
        fixed (byte* bufPtr = bytes)
        {
            var read = bufPtr;
            var write = bufPtr;
            for (int i = 0; i < pixelcount; i++)
            {
                *(write+2) = *read++; //R
                *(write+1) = *read++; //G
                *write = *read++; //B
                write += 3;
                read++; //A
            }
        }
    }

    protected override object GetImageObject() => new MemoryStream(bytes, 0, length);
}

public class RawImageCreator : IImageCreator
{
    public static void Register() => ImageFactory.Register(new RawImageCreator());
    
    public bool IsDefault => true;

    public IImage Create(int width, int height, byte[] bytes) =>
        new RawImage(width, height, bytes);

    public BlkImgDataSrc ToPortableImageSource(object imageObject)
    {
        throw new System.NotSupportedException();
    }
}