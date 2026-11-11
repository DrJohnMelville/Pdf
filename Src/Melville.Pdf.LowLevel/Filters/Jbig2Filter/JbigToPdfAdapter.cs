using Melville.JBig2;
using Melville.JBig2.BinaryBitmaps;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter;

internal class JbigToPdfAdapter: ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectObject parameters, object? context) => 
        throw new NotSupportedException();

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectObject parameters, object? context)
    {
        var reader = new JbigExplicitPageReader();
        reader.RequestPage(1);
        if (parameters.TryGet(out PdfDictionary? dict)&&
            dict.TryGetValue(KnownNames.JBIG2Globals, out var globalsTask) &&
            (await globalsTask.CA()).TryGet(out PdfStream? globals))
        {
            await using var globalStream = await globals.StreamContentAsync().CA();
            await reader.ProcessSequentialSegmentsAsync(globalStream, 1).CA();
        }

        await reader.ProcessSequentialSegmentsAsync(input, 1).CA();
        input.Dispose();
        var page = reader.GetPage(1);
        var (ary, _) = page.ColumnLocation(0);

        // in the JBIG spec 0 represents a white (background) pixel and 1 represents black,  Since this is the opposite of
        // how DeviceGray color palate works -- we will invert all the bits
        InvertSpan(ary.AsSpan(0, page.BufferLength()));

        return new MemoryStream(ary, 0, page.BufferLength(), false, true);
    }

    void InvertSpan(Span<byte> span)
    {
        var remainder = span.Length % Vector<byte>.Count;
        if (remainder is > 0) InvertBytes(span[..remainder]);
        InvertBytesFast(span[remainder..]);
    }

    private static void InvertBytesFast(Span<byte> span)
    {
        var bigSpan = MemoryMarshal.Cast<byte, Vector<byte>>(span);
        for (int i = 0; i < bigSpan.Length; i++)
        {
            bigSpan[i] = ~bigSpan[i];
        }
    }

    private static void InvertBytes(Span<byte> asSpan)
    {
        for (int i = 0; i < asSpan.Length; i++)
        {
            asSpan[i] = (byte)~asSpan[i];
        }
    }

}