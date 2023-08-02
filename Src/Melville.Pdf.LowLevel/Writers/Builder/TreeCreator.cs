using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melville.INPC;
using Melville.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public static class PdfTreeElementNamer
{
    public static PdfDirectObject FinalArrayName(bool isNumberTree) =>
        isNumberTree? KnownNames.Nums : KnownNames.Names;

}

internal static class TreeCreator
{
    public static PdfDictionary CreateNumberTree(
        this IPdfObjectCreatorRegistry builder, int nodeSize, params (PdfDirectObject, PdfIndirectObject)[] items) =>
        CreateNumberTree(builder, nodeSize, (IEnumerable<(PdfDirectObject, PdfIndirectObject)>)items);

    public static PdfDictionary CreateNumberTree(
        this IPdfObjectCreatorRegistry builder, int nodeSize,
        IEnumerable<(PdfDirectObject Key, PdfIndirectObject Item)> items) =>
        new TreeCreatorImpl(builder, nodeSize, KnownNames.Nums).CreateTree(items);

    public static PdfDictionary CreateNameTree(
        this IPdfObjectCreatorRegistry builder, int nodeSize, params (PdfDirectObject, PdfIndirectObject)[] items) =>
        CreateNameTree(builder, nodeSize, (IEnumerable<(PdfDirectObject, PdfIndirectObject)>)items);

    public static PdfDictionary CreateNameTree(
        this IPdfObjectCreatorRegistry builder, int nodeSize,
        IEnumerable<(PdfDirectObject Key, PdfIndirectObject Item)> items) =>
        new TreeCreatorImpl(builder, nodeSize, KnownNames.Names).CreateTree(items);
}

public readonly partial struct TreeCreatorImpl
{

    [FromConstructor] private readonly IPdfObjectCreatorRegistry builder;
    [FromConstructor] private readonly int nodeSize;
    [FromConstructor] private readonly PdfDirectObject finalArrayName;

    public PdfDictionary CreateTree(IEnumerable<(PdfDirectObject Key, PdfIndirectObject Item)> items)
    {
        var ordered = items
            .OrderBy(i => i.Key)
            .Chunks(nodeSize)
            .Select(CreateLeafNode)
            .ToList();
        return TreeFromLeaves(ordered);
    }

    private PdfDictionary CreateLeafNode(IList<(PdfDirectObject Key, PdfIndirectObject Item)> ordered)
    {
        Debug.Assert(ordered.Count <= nodeSize && ordered.Count > 0);
        return new DictionaryBuilder()
            .WithItem(finalArrayName, new PdfArray(InterleveKeysAndValues(ordered)))
            .WithItem(KnownNames.Limits, new PdfArray(ordered.First().Key, ordered.Last().Key))
            .AsDictionary();
    }

    private static PdfIndirectObject[] InterleveKeysAndValues(IList<(PdfDirectObject Key, PdfIndirectObject Item)> ordered)
    {
        var finalList = new PdfIndirectObject[ordered.Count * 2];
        int position = 0;
        foreach (var item in ordered)
        {
            finalList[position++] = item.Key;
            finalList[position++] = item.Item;
        }
        return finalList;
    }

    private PdfDictionary TreeFromLeaves(IReadOnlyList<PdfDictionary> leaves) =>
        leaves.Count == 1 ? TrivialTree(leaves) : TreeFromChunks(leaves);

    private PdfDictionary TrivialTree(IReadOnlyList<PdfDictionary> leaves) => 
        new DictionaryBuilder().WithItem(finalArrayName, leaves[0].RawItems[finalArrayName]).AsDictionary();

    private PdfDictionary TreeFromChunks(IReadOnlyList<PdfDictionary> leaves)
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Kids,
                new PdfArray(ReduceLeaves(leaves).Select(CreateIndirectReference).ToArray()))
            .AsDictionary();
    }

    private IReadOnlyList<PdfDictionary> ReduceLeaves(IReadOnlyList<PdfDictionary> leaves) =>
        leaves.Count <= nodeSize ? leaves : ReduceLeaves(leaves.Chunks(nodeSize).Select(MiddleNode).ToList());

    private PdfDictionary MiddleNode(IList<PdfDictionary> nodes)
    {
        Debug.Assert(nodes.Count <= nodeSize);
        return
            new DictionaryBuilder()
                .WithItem(KnownNames.Kids, new PdfArray(nodes.Select(CreateIndirectReference).ToArray()))
                .WithItem(KnownNames.Limits, new PdfArray(FirstKey(nodes.First()), LastKey(nodes.Last())))
                .AsDictionary();
    }

    private PdfIndirectObject CreateIndirectReference(PdfDictionary i) => builder.Add(i);

    public PdfIndirectObject FirstKey(PdfDictionary node) => LimitsArray(node)[0];
    public PdfIndirectObject LastKey(PdfDictionary node) => LimitsArray(node).Last();

    private IReadOnlyList<PdfIndirectObject> LimitsArray(PdfDictionary node)
    {
        if (!(node.RawItems.TryGetValue(KnownNames.Limits, out var indir) &&
            indir.TryGetEmbeddedDirectValue(out var dirvalue) &&
            dirvalue.TryGet(out PdfArray? limits)))
            throw new InvalidOperationException("Cannot find item");
        return limits.RawItems;
    }
}