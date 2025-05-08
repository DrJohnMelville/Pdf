using System;
using CoreJ2K.Util;
using Melville.Parsing.Streams.Bases;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

internal class ReadPartialBytesStream: DefaultBaseStream, IImageSizeStream
{
    private int position;
    private ReadOnlyMemory<byte> source;

    public ReadPartialBytesStream(PortableImage image, int bytesPerQuad) : base(true, false, false)
    {
        Width = image.Width;
        Height = image.Height;
        ImageComponents = image.NumberOfComponents;
        RawImageCreator.Register();
        var data = image.As<RawImage>();
        source = CompressArray(data, bytesPerQuad);
    }

    private static ReadOnlyMemory<byte> CompressArray(RawImage image, int bytesPerQuad)
    {
        if (bytesPerQuad >= image.Components) return new ReadOnlyMemory<byte>(image.Bytes);
      
        var bytes = image.Bytes;
        if (bytes.Length % bytesPerQuad != 0) throw new ArgumentException("Invalid length for JPX image");
        int target = bytesPerQuad;
        for (int source = bytesPerQuad; source < bytes.Length; source+=bytesPerQuad)
        {
            bytes.AsSpan(source, bytesPerQuad).CopyTo(bytes.AsSpan(target));
            target += bytesPerQuad;
        }

        return new ReadOnlyMemory<byte>(bytes, 0, target);
    }

    public int Width { get; }
    public int Height { get; }
    public int ImageComponents { get; }
    public int BitsPerComponent => 8;

    public override int Read(Span<byte> buffer)
    {
        var retLen = Math.Min(source.Length - position, buffer.Length);
        source.Span.Slice(position, retLen).CopyTo(buffer);
        position += retLen;
        return retLen;
    }
}