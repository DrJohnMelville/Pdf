using System.Buffers;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Runtime.Intrinsics.X86;
using Melville.JpegLibrary.BlockOutputWriters;
using Melville.JpegLibrary.Decoder;

namespace Melville.JpegLibrary.PipeAmdStreamAdapters;

public static class JpegStreamFactory2
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
        #warning rent this array
        var bufferLength = decoder.Width * decoder.Height * decoder.NumberOfComponents;
        var output = ArrayPool<byte>.Shared.Rent(bufferLength);
        var outputWriter =
            new JpegBufferOutputWriter8Bit(decoder.Width, decoder.Height, decoder.NumberOfComponents, output);
        decoder.SetOutputWriter(outputWriter);
        decoder.Decode();
        return decoder.NumberOfComponents == 3 ? 
            new YCrCbStream(output, bufferLength):
            new StraightCopyStream(output, bufferLength);
    }
}