using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Performance.Playground.ObjectModel;

public class DictionaryVsList
{
    [Params(1, 2, 5, 10)] public int N { get; set; }

    private IDictionary<PdfName, PdfObject> dict;

    [Params(false, true)] public bool New { get; set; }

    private IReadOnlyDictionary<PdfName, PdfObject> sut;

    [GlobalSetup]
    public void CreateDictionary()
    {
        if (New)
            CreateNewDictionary();
        else
            CreateOldDictionary();
    }

    private void CreateNewDictionary()
    {
        CreateOldDictionary();
        sut = new SmallReadOnlyDictionary<PdfName, PdfObject>(sut.ToArray());
    }

    private void CreateOldDictionary()
    {
        var dict = new Dictionary<PdfName, PdfObject>();
        for (int i = 0; i < N; i++)
        {
            dict.Add(names[i], names[i]);
        }

        sut = dict;
    }

    private static readonly PdfName[] names =
    {
        KnownNames.All,
        KnownNames.K,
        KnownNames.Kids,
        KnownNames.RGB,
        KnownNames.RBGroups,
        KnownNames.Catalog,
        KnownNames.A85,
        KnownNames.And,
        KnownNames.N,
        KnownNames.None,
        KnownNames.Background
    };

    [Benchmark]
    public void Access()
    {
        foreach (var key in names)
        {
            sut.TryGetValue(key, out _);
        }
    }
}