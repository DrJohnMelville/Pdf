﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal readonly struct MaskBitmap
{
    public byte[] Mask { get; }
    public int Width { get; }
    public int Height { get; }

    private MaskBitmap(byte[] mask, int width, int height)
    {
        Mask = mask;
        Width = width;
        Height = height;
    }
    
    public ReadOnlySpan<byte> PixelAt(int row, int col) =>
        PixelAt(PixelOffset(row, col));
    public ReadOnlySpan<byte> PixelAt(int index) => Mask.AsSpan(index, 4);

    public int PixelOffset(int row, int col)
    {
        Debug.Assert(row >= 0);
        Debug.Assert(row < Height);
        Debug.Assert(col >= 0);
        Debug.Assert(col < Width);
        return 4 * (col + (row * Width));
    }

    public static async ValueTask<MaskBitmap> CreateAsync(PdfStream stream, IHasPageAttributes page)
    {
        var wrapped = await stream.WrapForRenderingAsync(page, DeviceColor.Black).CA();
        var buffer = await wrapped.AsByteArrayAsync().CA();
        return new MaskBitmap(buffer, wrapped.Width, wrapped.Height);
    }

    public bool IsSameSizeAs(IPdfBitmap other) =>
        Height == other.Height && Width == other.Width;
}