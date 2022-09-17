using System.Buffers;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Xunit;

namespace Performance.Playground.Rendering;
#pragma warning disable xUnit1013

public class BitmapWriting
{
    private static byte[] source = Enumerable.Repeat((byte)12, 30000).ToArray();
    private static byte[] dest = Enumerable.Repeat((byte)12, 40000).ToArray();
        
    [Benchmark(Baseline = true)]
    public void Generic() => TestWriter(new 
        NBitByteWriter(new ComponentWriter(
            new ClosedInterval(0, 255), new ClosedInterval[]
            {
                new (0,1), new (0,1), new (0,1)
            }, DeviceRgb.Instance), 8));

    [Benchmark()]
    public void Specialized() => TestWriter(FastBitmapWriterRGB8.Instance);

    private static unsafe void TestWriter(IByteWriter writer)
    {
        fixed (byte* destPtr = dest)
        {
            var capSrc = new SequenceReader<byte>(new ReadOnlySequence<byte>(source));
            var capDest = destPtr;
            writer.WriteBytes(ref capSrc, ref capDest, destPtr + 40000);
        }
    }

    [Fact]
    public unsafe void StencilWriterTest()
    {
        var source = new SequenceReader<byte>(new ReadOnlySequence<byte>(new byte[] { 0b1010_1100 }));
        var dest = new byte[32];
        var final = new byte[]
        {
            3, 2, 1, 255,
            0, 0, 0, 0,
            3, 2, 1, 255,
            0, 0, 0, 0,
            3, 2, 1, 255,
            3, 2, 1, 255,
            0, 0, 0, 0,
            0, 0, 0, 0
        };
        fixed (byte* destptr = dest)
        {
            var captureDest = destptr;
            new StencilWriter(new double[] {1.0,0.0}, new DeviceColor(1,2,3,255))
                .WriteBytes(ref source, ref captureDest, destptr+32);
        }

        Assert.Equal(final, dest);
        
    }
}