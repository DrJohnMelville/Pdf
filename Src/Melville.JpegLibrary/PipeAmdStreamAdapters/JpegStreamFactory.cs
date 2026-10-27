using System.Buffers;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.JpegLibrary.BlockOutputWriters;
using Melville.JpegLibrary.Decoder;

using Melville.Parsing.CountingReaders;
using Melville.Parsing.IStreamAdaptors;
using Melville.Parsing.LinkedLists;
using Melville.Parsing.MultiplexSources;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.StreamInterfaces.Streams;

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
    public async ValueTask<Stream> FromStreamAsync(Stream s)
    {
        using var buffer = new MultiMemoryBuffer();
        using(var writer = buffer.CreateWriter())
        {
            await s.AsStreamReader().CopyToAsync(writer).CA();
        }
        s.Dispose();
        // Melville.Jpeg is really designed around ReadOnlySequence so there is no
        // need for a unitary buffer
        return FromReadOnlySequence(buffer.AsSequence());
    }

    private Stream FromReadOnlySequence(ReadOnlySequence<byte> input)
    {
        var decoder = new JpegDecoder();
        decoder.SetInput(input);
        decoder.Identify();
        var bufferLength = decoder.Width * decoder.Height * decoder.NumberOfComponents;
        using var output = new PooledBuffer<byte>(bufferLength);
        var outputWriter =
            new JpegBufferOutputWriter8Bit(
                decoder.Width, decoder.Height, decoder.NumberOfComponents, output.AsMemory());
        decoder.SetOutputWriter(outputWriter);
        decoder.Decode();

        DoDecode(decoder, output, bufferLength);
        return new StraightCopyStream(output, bufferLength);

    }
    private void DoDecode(JpegDecoder decoder, byte[] output, int bufferLength)
    {
        if (decoder.App14EncodingByte.HasValue)
        {
            switch (decoder.App14EncodingByte.Value, decoder.NumberOfComponents) 
            {
                case (2, 4) :
                    YCCKConversion.ConvertArray(output, bufferLength);
                    break;
                case (1, 3) : 
                    ConvertYCrCb.Convert(output, bufferLength);
                    break;
            };
            return;
        }

        if (ColorTransformExplicitlyProhibited() ||
            ColorTransformImplicitlyProhibited(decoder)) 
            return ;

        switch (decoder.NumberOfComponents)
        {
            case 3:
                ConvertYCrCb.Convert(output, bufferLength);
                break;
            case 4:
                YCCKConversion.ConvertArray(output, bufferLength);
                break;
        };
    }

    private bool ColorTransformImplicitlyProhibited(JpegDecoder decoder) => 
        colorTransformFromPdf == -1 && decoder.NumberOfComponents != 3;

    private bool ColorTransformExplicitlyProhibited() => 
        colorTransformFromPdf == 0;
}