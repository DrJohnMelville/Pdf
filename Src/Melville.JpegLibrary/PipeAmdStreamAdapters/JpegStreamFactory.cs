using System.Buffers;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.JpegLibrary.BlockOutputWriters;
using Melville.JpegLibrary.Decoder;

namespace Melville.JpegLibrary.PipeAmdStreamAdapters;

/// <summary>
/// Creates a stream that decodes a JPEG image into a stream of bytes representing color data
/// </summary>
public readonly partial struct JpegStreamFactory
{
    /// ColorTransform Value from the DctDecode
    /// parameter in the stream dictionary in PDF.  If this is 0 there is no color
    /// transformation, if 1 then 3 element images are converted from
    /// YCbCr -> RGB and 4 element values are transformed from YCrCbK -> CMYK.  This
    /// parameter can be overridden, either way, by an App14 block in the JPEG file.
    [FromConstructor] private readonly long colorTransformFromPdf;
    
    /// <summary>
    /// Create a JPegDecoder stream from a stream
    /// </summary>
    /// <param name="s">A readable stream containing a JPEG image file</param>
    /// <returns>A stream that will read pixel values from the JPEG image.   Top to bottom, left to right.</returns>
    public ValueTask<Stream> FromStream(Stream s) => FromPipe(PipeReader.Create(s), s.Length);

    /// <summary>
    /// Create a Jpeg stream from a pipereader.
    /// </summary>
    /// <param name="pipe">The pipe reader.</param>
    /// <param name="length">The length of the data</param>
    /// <returns>A stream representing the image</returns>
    public async ValueTask<Stream> FromPipe(PipeReader pipe, long length)
    {
        var seq = await pipe.ReadAtLeastAsync((int)length);
        return FromReadOnlySequence(seq.Buffer);
    }

    /// <summary>
    /// Create a JpegStream from a sequence of bytes
    /// </summary>
    /// <param name="input">The JPEG data as a sequence.</param>
    /// <returns>A stream that will read pixel values from the JPEG image.   Top to bottom, left to right.</returns>
    public Stream FromReadOnlySequence(ReadOnlySequence<byte> input)
    {
        var decoder = new JpegDecoder();
        decoder.SetInput(input);
        decoder.Identify();
        var bufferLength = decoder.Width * decoder.Height * decoder.NumberOfComponents;
        var output = ArrayPool<byte>.Shared.Rent(bufferLength);
        var outputWriter =
            new JpegBufferOutputWriter8Bit(decoder.Width, decoder.Height, decoder.NumberOfComponents, output);
        decoder.SetOutputWriter(outputWriter);
        decoder.Decode();

        return SelectDecoding(decoder, output, bufferLength);
    }

    private Stream SelectDecoding(JpegDecoder decoder, byte[] output, int bufferLength)
    {
        if (decoder.App14EncodingByte.HasValue)
            return (decoder.App14EncodingByte.Value, decoder.NumberOfComponents) switch
            {
                (2, 4) => new YCCKStream(output, bufferLength),
                (1, 3) => new YCrCbStream(output, bufferLength),
                _ => new StraightCopyStream(output, bufferLength)
            };

        if (ColorTransformExplicitlyProhibited() ||
            ColorTransformImplicitlyProhibited(decoder)||
            ImageIsNotSubsampled(decoder)) return new StraightCopyStream(output, bufferLength);
        
        return decoder.NumberOfComponents switch
        {
            3 => new YCrCbStream(output, bufferLength),
            4 => new YCCKStream(output, bufferLength),
            _ => new StraightCopyStream(output, bufferLength)
        };
    }

    private bool ColorTransformImplicitlyProhibited(JpegDecoder decoder) => 
        colorTransformFromPdf == -1 && decoder.NumberOfComponents != 3;

    private bool ColorTransformExplicitlyProhibited() => colorTransformFromPdf == 0;

    private bool ImageIsNotSubsampled(JpegDecoder decoder) =>
        decoder.NumberOfComponents == 1 || decoder.GetMaximumHorizontalSampling() == 1;
}