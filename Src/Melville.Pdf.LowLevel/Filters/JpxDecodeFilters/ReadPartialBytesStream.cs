using CoreJ2K.Util;
using Melville.Parsing.SequenceReaders;
using Melville.Parsing.Streams.Bases;
using Melville.StreamInterfaces;
using System;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

internal class ReadPartialBytesStream: DefaultBaseStream, IImageSizeStream, ISequenceSource
{
    private ReadOnlyMemory<byte> source;

    static ReadPartialBytesStream() => ImageFactory.Register(RawImageCreator.Instance);

    public ReadPartialBytesStream(InterleavedImage image, int bytesPerQuad) : base(true, false, false)
    {
        Width = image.Width;
        Height = image.Height;
        ImageComponents = image.NumberOfComponents;
        var data = image.As<RawImage>();
        source = CompressArray(data, bytesPerQuad);
    }

    private static ReadOnlyMemory<byte> CompressArray(RawImage image, int bytesPerQuad)
    {
        if (bytesPerQuad >= image.Components) return new ReadOnlyMemory<byte>(image.Bytes);
      
        var bytes = image.Bytes.AsSpan();
        if (bytes.Length % bytesPerQuad != 0) throw new ArgumentException("Invalid length for JPX image");

        var sourceSpan = bytes[4..];
        var targetSpan = bytes[bytesPerQuad..];
        
        while (!sourceSpan.IsEmpty)
        {
            sourceSpan[..bytesPerQuad].CopyTo(targetSpan);
            sourceSpan = sourceSpan[4..];
            targetSpan = targetSpan[bytesPerQuad..];
        }
        return new ReadOnlyMemory<byte>(image.Bytes, 0, bytes.Length - targetSpan.Length);
    }

    public int Width { get; }
    public int Height { get; }
    public int ImageComponents { get; }
    public int BitsPerComponent => 8;

    public override int Read(Span<byte> buffer)
    {
        source.Span.TakeMax(buffer.Length, out var retLen).CopyTo(buffer);
        source = source[retLen..];
        return retLen;
    }

    public ReadOnlySequence<byte> AsSequence() => new ReadOnlySequence<byte>(source);
}