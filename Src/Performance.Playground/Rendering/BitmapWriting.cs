using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Melville.Pdf.ImageExtractor;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.SkiaSharp;
using Melville.StreamInterfaces.ParsingBuffers;
using SkiaSharp;
using Xunit;

namespace Performance.Playground.Rendering;
#pragma warning disable xUnit1013

public class ImageExtraction
{
    [Benchmark()]
    public async Task ExtractImagesAsync()
    {
        int index = 1;
        var doc = await new PdfReader().ReadFromFileAsync(@"C:\Users\jmelv\Documents\PhotoDoc website Backup\backup_2019-05-31-1240_Digital_Forensic_Photography_262d8efaca37-uploads\uploads\2014\04\AAFS-slides.pdf");
        await foreach (var image in doc.CollapsedImagesFromAsync())
        {
            var skBitmap = await image.ToSkBitmapAsync();
            await using var output = File.Create(@"C:\Users\jmelv\Documents\Scratch\Output" + $"\\{index++}.jpg");
            skBitmap.Encode(SKEncodedImageFormat.Jpeg, 90).SaveTo(output);
        }

    }
}

public class BitmapWriting
{
    private static byte[] source = Enumerable.Repeat((byte)12, 30000).ToArray();
    private static byte[] dest = Enumerable.Repeat((byte)12, 40000).ToArray();
        
    [Benchmark(Baseline = true)]
    public void Direct() {

        SpanParsingBuffer input = new(source);
        var next = dest.AsSpan();
        FastBitmapWriterRGB8.Instance.WriteBytes(ref input, ref next);
    }

    [Benchmark]
    public void Old() => TestWriter(FastBitmapWriterRGB82.Instance);
    [Benchmark]
    public void Briged() => TestWriter(FastBitmapWriterRGB8.Instance);

    private static unsafe void TestWriter(IByteWriter writer)
    {
        fixed (byte* destPtr = dest)
        {
            var capSrc = new SequenceReader<byte>(new ReadOnlySequence<byte>(source));
            var capDest = destPtr;
            writer.WriteBytes(ref capSrc, ref capDest, destPtr + 40000);
        }
    }
}