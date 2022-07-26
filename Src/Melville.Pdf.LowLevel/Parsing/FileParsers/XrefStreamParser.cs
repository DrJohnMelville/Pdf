using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Melville.Linq;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

public readonly struct XrefStreamParserFactory
{
    private static readonly PdfArray DefaultIndex = new PdfArray(0, int.MaxValue);
    private readonly PdfStream xrefStream;
    private readonly IIndirectObjectRegistry registry;

    public XrefStreamParserFactory(PdfStream xrefStream, IIndirectObjectRegistry registry)
    {
        this.xrefStream = xrefStream;
        this.registry = registry;
    }

    public async ValueTask<XrefStreamParser> Create()
    {
        var w = await xrefStream.GetAsync<PdfArray>(KnownNames.W).CA();
        return new XrefStreamParser(
            await w.IntAtAsync(0).CA(),
            await w.IntAtAsync(1).CA(),
            await w.IntAtAsync(2).CA(),
            await xrefStream.GetOrDefaultAsync(KnownNames.Index, DefaultIndex).CA(),
            registry
        );
    }
}

public readonly struct XrefStreamParser
{
    private readonly int col0Size;
    private readonly int col1Size;
    private readonly int col2Size;
    private readonly int itemSize;
    private readonly IIndirectObjectRegistry registry;
    private readonly PdfArray index;

    public XrefStreamParser(int col0Size, int col1Size, int col2Size, PdfArray index,
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

    private static void VerifyIndexHasEvenNumberOfElements(PdfArray index)
    {
        if (index.Count % 2 != 0)
            throw new PdfParseException("Index of an xrefstream must have even number of elements");
    }

    public async ValueTask Parse(PipeReader source)
    {
        for (int i = 0; i < index.Count; i+=2)
        {
            var baseNum = await index.IntAtAsync(i).CA();
            var length = await index.IntAtAsync(i+1).CA();
            await ParseSection(source, baseNum, length).CA();
        }
    }

    private async ValueTask ParseSection(PipeReader source, int baseNum, int length)
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
                registry.RegisterDeletedBlock(index, c2, c1);
                break;
            case 1:
                registry.RegisterIndirectBlock(index, c2, c1);
                break;
            case 2:
                registry.RegisterObjectStreamBlock(index, c1, c2);
                break;
            default:
                registry.RegistedNullObject(index, c2, c1);
                break;
        }
    }



}