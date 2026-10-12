using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using CoreJ2K.Util;
using Melville.Parsing.Streams.Bases;
using Melville.StreamInterfaces;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

internal class ReadPartialBytesStream: DefaultBaseStream, IImageSizeStream
{
    private int position;
    private ReadOnlyMemory<byte> source;

    static ReadPartialBytesStream()
    {
        (typeof(ImageFactory).GetField("_creators", BindingFlags.NonPublic | BindingFlags.Static)?.
            GetValue(null) as List<IImageCreator>)?.Add(RawImageCreator.Instance);
    }

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
        source.Span.Slice(position).TakeMax(buffer.Length, out var retLen).CopyTo(buffer);
        position += retLen;
        return retLen;
    }
}