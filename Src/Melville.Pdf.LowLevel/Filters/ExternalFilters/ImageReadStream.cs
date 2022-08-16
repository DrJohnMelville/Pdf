using System;
using System.Runtime.InteropServices;
using Melville.CSJ2K.j2k.io;
using Melville.Parsing.Streams.Bases;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Melville.Pdf.LowLevel.Filters.ExternalFilters;

public class ImageReadStream<TPixel> : DefaultBaseStream, IImageSizeStream 
    where TPixel : unmanaged, IPixel<TPixel>
{
    private Image<TPixel> source;
    private int currentRow;
    private int currentByte;


    public int Width => source.Width;
    public int Height => source.Height;
    public int ImageComponents => 3;
    public int BitsPerComponent => 8;

    public ImageReadStream(Image<TPixel> source) : base(true, false, false)
    {
        this.source = source;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) source.Dispose();
        base.Dispose(disposing);
    }

    public override int Read(Span<byte> buffer)
    {
        var totalLen = 0;
        while (buffer.Length > 0 && currentRow < source.Height)
        {
            var localLen = TryReadFromRow(buffer);
            totalLen += localLen;
            buffer = buffer.Slice(localLen);
        }
        
        return totalLen;
    }

    private int TryReadFromRow(Span<byte> buffer)
    {
        var byteSpan = RemainingBytesInCurrentRow();
        var bytesToCopy = ClaimCopyableBytes(byteSpan.Length, buffer.Length);
        byteSpan.Slice(0,bytesToCopy).CopyTo(buffer);
        return bytesToCopy;
    }

    private Span<byte> RemainingBytesInCurrentRow() => 
        MemoryMarshal.AsBytes(source.DangerousGetPixelRowMemory(currentRow).Span)[currentByte..];

    private int ClaimCopyableBytes(int sourceLength, int destLength)
    {
        if (sourceLength <= destLength)
        {
            currentByte = 0;
            currentRow++;
            return sourceLength;
        }
        else
        {
            currentByte += destLength;
            return destLength;
        }
    }
}