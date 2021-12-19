using System.Buffers;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;

namespace Performance.Playground.Rendering
{
    public class BitmapWriting
    {
        private static byte[] source = Enumerable.Repeat((byte)12, 30000).ToArray();
        private static byte[] dest = Enumerable.Repeat((byte)12, 40000).ToArray();
        
        [Benchmark(Baseline = true)]
        public void Generic() => TestWriter(new NBitByteWriter(DeviceRgb.Instance, 8));

        [Benchmark()]
        public void Specialized() => TestWriter(new FastBitmapWriterRGB8());

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
}

