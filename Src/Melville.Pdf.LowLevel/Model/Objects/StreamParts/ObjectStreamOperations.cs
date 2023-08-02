using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects.StreamParts;

internal static class ObjectStreamOperations
{
    public static async ValueTask<IList<ObjectLocation>> GetIncludedObjectNumbersAsync(
        this PdfStream stream)
    {
        await using var decoded = await stream.StreamContentAsync().CA();
        return await GetIncludedObjectNumbersAsync(stream, 
            new ByteSource(PipeReader.Create(decoded))).CA();
    }


    public static async ValueTask<IList<ObjectLocation>> GetIncludedObjectNumbersAsync(
        PdfStream stream, IByteSource reader) =>
        await reader.GetIncludedObjectNumbersAsync(
            (await stream.GetAsync<int>(KnownNames.NTName).CA()),
            (await stream.GetAsync<int>(KnownNames.FirstTName).CA())).CA();

    private static async ValueTask<ObjectLocation[]> GetIncludedObjectNumbersAsync(
        this IByteSource reader, int count, int first)
    {
        var source = await reader.ReadAsync().CA();
        while (source.Buffer.Length < first)
        {
            reader.MarkSequenceAsExamined();
            source = await reader.ReadAsync().CA();
        }

        var ret = FillInts(new SequenceReader<byte>(source.Buffer), count, first);
        reader.AdvanceTo(source.Buffer.GetPosition(first));
        return ret;
    }

    private static ObjectLocation[] FillInts(SequenceReader<byte> seqReader, int count, int first)
    {
        #warning try to get rid of this allocation.
        var ret = new ObjectLocation[count];
        for (int i = 0; i < count; i++)
        {
            WholeNumberParser.TryParsePositiveWholeNumber(ref seqReader, out int number, out _);
            WholeNumberParser.TryParsePositiveWholeNumber(ref seqReader, out int offset, out _);
            ret[i] = new ObjectLocation(number, offset + first);
        }

        return ret;
    }
}