using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Writers;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public interface IHasInternalIndirectObjects
    {
        ValueTask<IEnumerable<ObjectLocation>> GetInternalObjectNumbersAsync();
    }

    public static class ObjectStreamOperations
    {
        public static async ValueTask<IList<ObjectLocation>> GetIncludedObjectNumbers(this PdfStream stream)
        {
            await using var decoded = await stream.GetDecodedStreamAsync();
            return await GetIncludedObjectNumbers(stream, PipeReader.Create(decoded));
        }

        public static async Task<IList<ObjectLocation>> GetIncludedObjectNumbers(
            PdfStream stream, PipeReader reader) =>
            await reader.GetIncludedObjectNumbers(
                (await stream.GetAsync<PdfNumber>(KnownNames.N)).IntValue,
                (await stream.GetAsync<PdfNumber>(KnownNames.First)).IntValue);

        public static async ValueTask<ObjectLocation[]> GetIncludedObjectNumbers(
            this PipeReader reader, long count, long first)
        {
            var source = await reader.ReadAsync();
            while (source.Buffer.Length < first)
            {
                reader.AdvanceTo(source.Buffer.Start, source.Buffer.End);
                source = await reader.ReadAsync();
            }

            var ret = FillInts(new SequenceReader<byte>(source.Buffer), count);
            reader.AdvanceTo(source.Buffer.GetPosition(first));
            return ret;
        }

        private static ObjectLocation[] FillInts(SequenceReader<byte> seqReader, long count)
        {
            var ret = new ObjectLocation[count];
            for (int i = 0; i < count; i++)
            {
                WholeNumberParser.TryParsePositiveWholeNumber(ref seqReader, out ret[i].ObjectNumber, out _);
                WholeNumberParser.TryParsePositiveWholeNumber(ref seqReader, out ret[i].Offset, out _);
            }

            return ret;
        }
    }
    public struct ObjectLocation
    {
        public int ObjectNumber;
        public int Offset;
    }
}