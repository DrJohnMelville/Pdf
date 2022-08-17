using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.Jpeg;
using Melville.Pdf.LowLevel.Model.Objects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Melville.Pdf.LowLevel.Filters.ExternalFilters;

public class DctDecoder : ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters)
    {
        throw new NotSupportedException();
    }

    public async ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters)
    {
        return await new JpegStreamFactory(input).Construct().CA();
//        return await ReadInFormat<Rgb24>(input).CA();
    }

    private static async Task<Stream> ReadInFormat<T>(Stream input) where T : unmanaged, IPixel<T>
    {
        var img = await Image.LoadAsync<T>(input).CA();
        return new ImageReadStream<T>(img);
    }
}