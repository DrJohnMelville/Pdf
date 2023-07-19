using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melville.INPC;
using Melville.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Trees;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public static class PdfTreeElementNamer
{
    public static PdfDirectValue FinalArrayName(bool isNumberTree) =>
        isNumberTree? KnownNames.NumsTName : KnownNames.NamesTName;

}

internal static class TreeCreator
{
    public static PdfValueDictionary CreateNumberTree(
        this IPdfObjectCreatorRegistry builder, int nodeSize, params (PdfDirectValue, PdfIndirectValue)[] items) =>
        CreateNumberTree(builder, nodeSize, (IEnumerable<(PdfDirectValue, PdfIndirectValue)>)items);

    public static PdfValueDictionary CreateNumberTree(
        this IPdfObjectCreatorRegistry builder, int nodeSize,
        IEnumerable<(PdfDirectValue Key, PdfIndirectValue Item)> items) =>
        new TreeCreatorImpl(builder, nodeSize, KnownNames.NumsTName).CreateTree(items);

    public static PdfValueDictionary CreateNameTree(
        this IPdfObjectCreatorRegistry builder, int nodeSize, params (PdfDirectValue, PdfIndirectValue)[] items) =>
        CreateNameTree(builder, nodeSize, (IEnumerable<(PdfDirectValue, PdfIndirectValue)>)items);

    public static PdfValueDictionary CreateNameTree(
        this IPdfObjectCreatorRegistry builder, int nodeSize,
        IEnumerable<(PdfDirectValue Key, PdfIndirectValue Item)> items) =>
        new TreeCreatorImpl(builder, nodeSize, KnownNames.NamesTName).CreateTree(items);
}

public readonly partial struct TreeCreatorImpl
{

    [FromConstructor] private readonly IPdfObjectCreatorRegistry builder;
    [FromConstructor] private readonly int nodeSize;
    [FromConstructor] private readonly PdfDirectValue finalArrayName;

    public PdfValueDictionary CreateTree(IEnumerable<(PdfDirectValue Key, PdfIndirectValue Item)> items)
    {
        var ordered = items
            .OrderBy(i => i.Key)
            .Chunks(nodeSize)
            .Select(CreateLeafNode)
            .ToList();
        return TreeFromLeaves(ordered);
    }

    private PdfValueDictionary CreateLeafNode(IList<(PdfDirectValue Key, PdfIndirectValue Item)> ordered)
    {
        Debug.Assert(ordered.Count <= nodeSize && ordered.Count > 0);
        return new ValueDictionaryBuilder()
            .WithItem(finalArrayName, new PdfValueArray(InterleveKeysAndValues(ordered)))
            .WithItem(KnownNames.LimitsTName, new PdfValueArray(ordered.First().Key, ordered.Last().Key))
            .AsDictionary();
    }

    private static PdfIndirectValue[] InterleveKeysAndValues(IList<(PdfDirectValue Key, PdfIndirectValue Item)> ordered)
    {
        var finalList = new PdfIndirectValue[ordered.Count * 2];
        int position = 0;
        foreach (var item in ordered)
        {
            finalList[position++] = item.Key;
            finalList[position++] = item.Item;
        }
        return finalList;
    }

    private PdfValueDictionary TreeFromLeaves(IReadOnlyList<PdfValueDictionary> leaves) =>
        leaves.Count == 1 ? TrivialTree(leaves) : TreeFromChunks(leaves);

    private PdfValueDictionary TrivialTree(IReadOnlyList<PdfValueDictionary> leaves) => 
        new ValueDictionaryBuilder().WithItem(finalArrayName, leaves[0].RawItems[finalArrayName]).AsDictionary();

    private PdfValueDictionary TreeFromChunks(IReadOnlyList<PdfValueDictionary> leaves)
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.KidsTName,
                new PdfValueArray(ReduceLeaves(leaves).Select(CreateIndirectReference).ToArray()))
            .AsDictionary();
    }

    private IReadOnlyList<PdfValueDictionary> ReduceLeaves(IReadOnlyList<PdfValueDictionary> leaves) =>
        leaves.Count <= nodeSize ? leaves : ReduceLeaves(leaves.Chunks(nodeSize).Select(MiddleNode).ToList());

    private PdfValueDictionary MiddleNode(IList<PdfValueDictionary> nodes)
    {
        Debug.Assert(nodes.Count <= nodeSize);
        return
            new ValueDictionaryBuilder()
                .WithItem(KnownNames.KidsTName, new PdfValueArray(nodes.Select(CreateIndirectReference).ToArray()))
                .WithItem(KnownNames.LimitsTName, new PdfValueArray(FirstKey(nodes.First()), LastKey(nodes.Last())))
                .AsDictionary();
    }

    private PdfIndirectValue CreateIndirectReference(PdfValueDictionary i) => builder.Add(i);

    public PdfIndirectValue FirstKey(PdfValueDictionary node) => LimitsArray(node)[0];
    public PdfIndirectValue LastKey(PdfValueDictionary node) => LimitsArray(node).Last();

    private IReadOnlyList<PdfIndirectValue> LimitsArray(PdfValueDictionary node)
    {
        if (!(node.RawItems.TryGetValue(KnownNames.LimitsTName, out var indir) &&
            indir.TryGetEmbeddedDirectValue(out var dirvalue) &&
            dirvalue.TryGet(out PdfValueArray? limits)))
            throw new InvalidOperationException("Cannot find item");
        return limits.RawItems;
    }
}