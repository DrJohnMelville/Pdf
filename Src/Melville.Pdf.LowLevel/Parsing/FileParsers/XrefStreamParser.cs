using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;

using Melville.Parsing.CountingReaders;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal readonly partial struct XrefStreamParserFactory
{
    private static readonly int[] DefaultIndex = { 0, int.MaxValue};
    [FromConstructor] private readonly PdfStream xrefStream;
    [FromConstructor] private readonly IIndirectObjectRegistry registry;

    public async ValueTask<XrefStreamParser> CreateAsync()
    {
        var w = await xrefStream.GetAsync<PdfArray>(KnownNames.W).CA();
        return new XrefStreamParser(
            (await w[0].CA()).Get<int>(),
            (await w[1].CA()).Get<int>(),
            (await w[2].CA()).Get<int>(),
            await ReadIndexArrayAsync().CA(),
            registry
        );
    }

    private async ValueTask<IReadOnlyList<int>> ReadIndexArrayAsync()
    {
        var readArray = await xrefStream.GetOrDefaultAsync(KnownNames.Index, (PdfArray?)null).CA();
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
    private readonly IReadOnlyList<int> index;

    public XrefStreamParser(int col0Size, int col1Size, int col2Size, IReadOnlyList<int> index,
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

    private static void VerifyIndexHasEvenNumberOfElements(IReadOnlyList<int> index)
    {
        if (index.Count % 2 != 0)
            throw new PdfParseException("Index of an xrefstream must have even number of elements");
    }

    public async ValueTask ParseAsync(ParsingBuffer source)
    {
        for (int i = 0; i < index.Count; i+=2)
        {
            var baseNum = index[i];
            var length = index[i+1];
            await ParseSectionAsync(source, baseNum, length).CA();
        }
    }

    private async ValueTask ParseSectionAsync(ParsingBuffer source, int baseNum, int length)
    {
        var pos2 = col0Size + col1Size;
        for (int i = 0; i < length; i++)
        {
            ;
            if (!await source.TryEnsureBytesAsync(itemSize).CA()) return;
            var buffer = source.Peek(itemSize);
            ProcessLine(baseNum + i,
                IntFromSpan(buffer[..col0Size]),
                IntFromSpan(buffer[col0Size..pos2]),
                IntFromSpan(buffer[pos2..])
                );
            source.Advance(itemSize);
        }
    }

    private static ulong IntFromSpan(ReadOnlySpan<byte> bytes)
    {
        ulong ret = 0;
        foreach (var b in bytes )
        {
            ret = (ret << 8) | b;
        }
        return ret;
    }


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