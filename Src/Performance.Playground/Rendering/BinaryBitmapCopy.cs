using System;
using BenchmarkDotNet.Attributes;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Performance.Playground.Rendering
{
    public class BinaryBitmapCopy
    {
        private const int Dimension = 40;
        private BinaryBitmap source = new BinaryBitmap(Dimension, Dimension);
        private BinaryBitmap FastDestination = new BinaryBitmap(Dimension, Dimension);

        public BinaryBitmapCopy()
        {
            var rand = new Random(1234); //we want unpredictable, but replicable data
            rand.NextBytes(source.AsByteSpan());
        }

        [Benchmark(Baseline = true)]
        public void FastAlgorithm()
        {
            for (int i = -20; i < 20; i++)
            {
                for (int j = -20; j < 20; j++)
                {
                    for (int k = 0; k < 5; k++)
                    {
                        FastDestination.PasteBitsFrom(i,j, source, (CombinationOperator)k);
                    }
                }
            }
        }
    }
}