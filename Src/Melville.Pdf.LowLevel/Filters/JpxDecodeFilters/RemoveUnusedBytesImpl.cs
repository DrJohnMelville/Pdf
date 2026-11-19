using CoreJ2K.Util;
using Melville.Parsing.SequenceReaders;
using Melville.Parsing.Streams.Bases;
using Melville.StreamInterfaces;
using System;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

internal static class RemoveUnusedBytesImpl
{

    static RemoveUnusedBytesImpl() => ImageFactory.Register(RawImageCreator.Instance);

#warning this would be a good target for a SIMD optimization.
    public static ReadOnlyMemory<byte> GetUsedBytes(this InterleavedImage image)
    {
        var bytesPerQuad = image.NumberOfComponents;
        var data = image.As<RawImage>();

        if (bytesPerQuad >= data.Components) return new ReadOnlyMemory<byte>(data.Bytes);
      
        var bytes = data.Bytes.AsSpan();
        if (bytes.Length % bytesPerQuad != 0) throw new ArgumentException("Invalid length for JPX image");
        
        //start on the second pixel because the first one is in place.
        var sourceSpan = bytes[4..];
        var targetSpan = bytes[bytesPerQuad..];
        
        while (!sourceSpan.IsEmpty)
        {
            sourceSpan[..bytesPerQuad].CopyTo(targetSpan);
            sourceSpan = sourceSpan[4..];
            targetSpan = targetSpan[bytesPerQuad..];
        }
        return new ReadOnlyMemory<byte>(data.Bytes, 0, bytes.Length - targetSpan.Length);
    }
}