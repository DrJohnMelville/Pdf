using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Linq;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal readonly partial struct XrefStreamParserFactory
{
    private static readonly int[] DefaultIndex = { 0, int.MaxValue};
    [FromConstructor] private readonly PdfValueStream xrefStream;
    [FromConstructor] private readonly IIndirectObjectRegistry registry;

    public async ValueTask<XrefStreamParser> CreateAsync()
    {
        var w = await xrefStream.GetAsync<PdfValueArray>(KnownNames.WTName).CA();
        return new XrefStreamParser(
            (await w[0].CA()).Get<int>(),
            (await w[1].CA()).Get<int>(),
            (await w[2].CA()).Get<int>(),
            await ReadIndexArrayAsync().CA(),
            registry
        );
    }

    private async ValueTask<int[]> ReadIndexArrayAsync()
    {
        var readArray = await xrefStream.GetOrDefaultAsync(KnownNames.IndexTName, (PdfValueArray?)null).CA();
        return readArray != null ? await readArray.CastAsync<int>().CA() : DefaultIndex;
    }
}

internal readonly struct XrefStreamParser
{
    private readonly int col0Size;
    private readonly int col1Size;
    private readonly int col2Size;
    private readonly int itemSize;
    private readonly IIndirectObjectRegistry registry;
    private readonly int[] index;

    public XrefStreamParser(int col0Size, int col1Size, int col2Size, int[] index,
        IIndirectObjectRegistry registry)
    {
        this.col0Size = col0Size;
        this.col1Size = col1Size;
        this.col2Size = col2Size;
        itemSize = col0Size + col1Size + col2Size;
        this.registry = registry;
        this.index = index;
        VerifyIndexHasEvenNumberOfElements(index);
    }

    private static void VerifyIndexHasEvenNumberOfElements(int[] index)
    {
        if (index.Length % 2 != 0)
            throw new PdfParseException("Index of an xrefstream must have even number of elements");
    }

    public async ValueTask ParseAsync(PipeReader source)
    {
        for (int i = 0; i < index.Length; i+=2)
        {
            var baseNum = index[i];
            var length = index[i+1];
            await ParseSectionAsync(source, baseNum, length).CA();
        }
    }

    private async ValueTask ParseSectionAsync(PipeReader source, int baseNum, int length)
    {
        for (int i = 0; i < length; i++)
        {
            var readResult = await source.ReadAtLeastAsync(itemSize).CA();
            if (!HasEnoughDataForOneRow(readResult.Buffer)) return;
            ReadOne(new SequenceReader<byte>(readResult.Buffer), baseNum + i);
            source.AdvanceTo(readResult.Buffer.GetPosition(itemSize));
        }
    }

    private void ReadOne(SequenceReader<byte> input, int index)
    {
        var c0 = input.ReadBigEndianUint(col0Size);
        var c1 = input.ReadBigEndianUint(col1Size);
        var c2 = input.ReadBigEndianUint(col2Size);
        ProcessLine(index, c0, c1, c2);
    }

    private bool HasEnoughDataForOneRow(in ReadOnlySequence<byte> input) => input.Length >= itemSize;

    private void ProcessLine(int index, ulong c0, ulong c1, ulong c2)
    {
        switch (c0)
        {
            case 0: 
                registry.RegisterDeletedBlock(index, (int)c2, (int)c1);
                break;
            case 1:
                registry.RegisterIndirectBlock(index, (int)c2, (long)c1);
                break;
            case 2:
                registry.RegisterObjectStreamBlock(index, (int)c1, (int)c2);
                break;
            default:
                throw new PdfParseException("Unrecognized xref stream entry type");
        }
    }



}