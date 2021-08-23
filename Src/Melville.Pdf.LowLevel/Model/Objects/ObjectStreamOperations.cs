using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public static class ObjectStreamOperations
    {
        public static async ValueTask<IList<int>> GetIncludedObjectNumbers(this PdfStream stream)
        {
            return await PipeReader.Create(await stream.GetDecodedStream()).GetIncludedObjectNumbers(
                (await stream.GetAsync<PdfNumber>(KnownNames.N)).IntValue,
                (await stream.GetAsync<PdfNumber>(KnownNames.First)).IntValue);
        }

        public static async ValueTask<IList<int>> GetIncludedObjectNumbers(
            this PipeReader reader, long count, long first)
        {
            var ret = new int[count];
            var source = await reader.ReadAsync();
            while (source.Buffer.Length < first)
            {
                reader.AdvanceTo(source.Buffer.Start, source.Buffer.End);
                source = await reader.ReadAsync();
            }

            FillInts(new SequenceReader<byte>(source.Buffer), ret);
            return ret;
        }

        private static void FillInts(SequenceReader<byte> seqReader, int[] ret)
        {
            for (int i = 0; i < ret.Length; i++)
            {
                WholeNumberParser.TryParsePositiveWholeNumber(ref seqReader, out ret[i], out _);
                WholeNumberParser.TryParsePositiveWholeNumber(ref seqReader, out long _, out _);
            }
        }
    }
}