using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects;

internal interface IHasInternalIndirectObjects
{
    ValueTask<IEnumerable<ObjectLocation>> GetInternalObjectNumbersAsync();
}

internal static class ObjectStreamOperations
{
    public static async ValueTask<IList<ObjectLocation>> GetIncludedObjectNumbersAsync(this PdfStream stream)
    {
        await using var decoded = await stream.StreamContentAsync().CA();
        return await GetIncludedObjectNumbersAsync(stream, 
            new ByteSourceWithGlobalPosition(PipeReader.Create(decoded), 0)).CA();
    }

    public static async ValueTask<IList<ObjectLocation>> GetIncludedObjectNumbersAsync(
        PdfStream stream, IByteSourceWithGlobalPosition reader) =>
        await reader.GetIncludedObjectNumbersAsync(
            (await stream.GetAsync<PdfNumber>(KnownNames.N).CA()).IntValue,
            (await stream.GetAsync<PdfNumber>(KnownNames.First).CA()).IntValue).CA();

    private static async ValueTask<ObjectLocation[]> GetIncludedObjectNumbersAsync(
        this IByteSourceWithGlobalPosition reader, long count, long first)
    {
        var source = await reader.ReadAsync().CA();
        while (source.Buffer.Length < first)
        {
            reader.AdvanceTo(source.Buffer.Start, source.Buffer.End);
            source = await reader.ReadAsync().CA();
        }

        var ret = FillInts(new SequenceReader<byte>(source.Buffer), count);
        SetNextOffsets(ret);
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

    private static void SetNextOffsets(ObjectLocation[] ret)
    {
        if (ret.Length < 1) return; // some files have empty objectstreams
        for (int i = 1; i < ret.Length; i++)
        {
            ret[i - 1].NextOffset = ret[i].Offset;
        }

        ret[^1].NextOffset = int.MaxValue;
    }
}
internal struct ObjectLocation
{
    public int ObjectNumber;
    public int Offset;
    public int NextOffset;
}