using System.Buffers;
using System.IO.Pipelines;
using Melville.JpegLibrary.BlockOutputWriters;
using Melville.JpegLibrary.Decoder;

namespace Melville.JpegLibrary.PipeAmdStreamAdapters;

public static class JpegStreamFactory
{
    public static ValueTask<Stream> FromStream(Stream s) => FromPipe(PipeReader.Create(s), s.Length);

    private static async ValueTask<Stream> FromPipe(PipeReader pipe, long length)
    {
        var seq = await pipe.ReadAtLeastAsync((int)length);
        return FromReadOnlySequence(seq.Buffer);
    }

    private static Stream FromReadOnlySequence(ReadOnlySequence<byte> input)
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

    private static Stream SelectDecoding(JpegDecoder decoder, byte[] output, int bufferLength)
    {
        if (decoder.App14EncodingByte.HasValue)
            return (decoder.App14EncodingByte.Value, decoder.NumberOfComponents) switch
            {
                (2, 4) => new YCCKStream(output, bufferLength),
                (1, 3) => new YCrCbStream(output, bufferLength),
                _ => new StraightCopyStream(output, bufferLength)
            };

        if (ImageIsNotSubsampled(decoder)) return new StraightCopyStream(output, bufferLength);

        return decoder.NumberOfComponents switch
        {
            3 => new YCrCbStream(output, bufferLength),
            4 => new YCCKStream(output, bufferLength),
            _ => new StraightCopyStream(output, bufferLength)
        };
    }

    private static bool ImageIsNotSubsampled(JpegDecoder decoder) =>
        decoder.NumberOfComponents == 1 || decoder.GetMaximumHorizontalSampling() == 1;
}