using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melville.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Trees;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public static class TreeCreator
{
    public static PdfDictionary CreateTree<T>(
        this ILowLevelDocumentBuilder builder, int nodeSize, params (T, PdfObject)[] items)
        where T : PdfObject, IComparable<T> =>
        CreateTree(builder, nodeSize, (IEnumerable<(T, PdfObject)>)items);

    public static PdfDictionary CreateTree<T>(
        this ILowLevelDocumentBuilder builder, int nodeSize, IEnumerable<(T Key, PdfObject Item)> items)
        where T : PdfObject, IComparable<T> =>
        new TreeCreator<T>(builder, nodeSize).CreateTree(items);
}

public readonly struct TreeCreator<T> where T : PdfObject, IComparable<T>
{
    private readonly ILowLevelDocumentBuilder builder;
    private readonly int nodeSize;
    private readonly PdfName finalArrayName;

    public TreeCreator(ILowLevelDocumentBuilder builder, int nodeSize)
    {
        this.builder = builder;
        this.nodeSize = nodeSize;
        finalArrayName = PdfTreeElementNamer.FinalArrayName<T>();
    }

    public PdfDictionary CreateTree(IEnumerable<(T Key, PdfObject Item)> items)
    {
        var ordered = items
            .OrderBy(i => i.Key)
            .Chunks(nodeSize)
            .Select(CreateLeafNode)
            .ToList();
        return TreeFromLeaves(ordered);
    }

    private PdfDictionary CreateLeafNode(IList<(T Key, PdfObject Item)> ordered)
    {
        Debug.Assert(ordered.Count <= nodeSize);
        return new DictionaryBuilder()
            .WithItem(finalArrayName, new PdfArray(InterleveKeysAndValues(ordered)))
            .WithItem(KnownNames.Limits, new PdfArray(ordered.First().Key, ordered.Last().Key))
            .AsDictionary();
    }

    private static List<PdfObject> InterleveKeysAndValues(IList<(T Key, PdfObject Item)> ordered)
    {
        var finalList = new List<PdfObject>(ordered.Count * 2);
        foreach (var item in ordered)
        {
            finalList.Add(item.Key);
            finalList.Add(item.Item);
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
                new PdfArray(ReduceLeaves(leaves).Select(CreateIndirectReference).ToList()))
            .AsDictionary();
    }

    private IReadOnlyList<PdfDictionary> ReduceLeaves(IReadOnlyList<PdfDictionary> leaves) =>
        leaves.Count <= nodeSize ? leaves : ReduceLeaves(leaves.Chunks(nodeSize).Select(MiddleNode).ToList());

    private PdfDictionary MiddleNode(IList<PdfDictionary> nodes)
    {
        Debug.Assert(nodes.Count <= nodeSize);
        return
            new DictionaryBuilder()
                .WithItem(KnownNames.Kids, new PdfArray(nodes.Select(CreateIndirectReference).ToList()))
                .WithItem(KnownNames.Limits, new PdfArray(FirstKey(nodes.First()), LastKey(nodes.Last())))
                .AsDictionary();
    }

    private PdfIndirectObject CreateIndirectReference(PdfDictionary i) => builder.Add(i);

    public T FirstKey(PdfDictionary node) => (T)LimitsArray(node)[0];
    public T LastKey(PdfDictionary node) => (T)LimitsArray(node).Last();

    private IReadOnlyList<PdfObject> LimitsArray(PdfDictionary node) =>
        ((PdfArray)node.RawItems[KnownNames.Limits]).RawItems;
}