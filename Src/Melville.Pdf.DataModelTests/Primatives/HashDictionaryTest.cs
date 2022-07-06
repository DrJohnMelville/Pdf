using System;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Xunit;

namespace Melville.Pdf.DataModelTests.Primatives;

public static class HashDictionaryHelper
{
    public static T GetOrCreate<T>(this HashDictionary<T> dict, string name) where T:class
    {
        Span<byte> item = stackalloc byte[name.Length];
        for (int i = 0; i < item.Length; i++)
        {
            item[i] = (byte)name[i];
        }
        return dict.GetOrCreate(item);
    }

}
public class HashDictionaryTest
{
    // FFV-1 colision courtesy of
    // https://softwareengineering.stackexchange.com/questions/49550/which-hashing-algorithm-is-best-for-uniqueness-and-speed
    private const string ColidingName2 = "quists";
    private const string ColidingName1 = "creamwove";
    private readonly NameDictionay sut = new NameDictionay();
    [Fact]
    public void CreateName()
    {
        var name = sut.GetOrCreate("xrt23");
        Assert.Equal("/xrt23", name.ToString());
    }

    [Fact]
    public void Unique()
    {
        Assert.True(ReferenceEquals(sut.GetOrCreate("ikhkl"), sut.GetOrCreate("ikhkl")));
    }

    [Fact]
    public void TestColision()
    {
        var o1 = sut.GetOrCreate(ColidingName1);
        var o2 = sut.GetOrCreate(ColidingName2);
        Assert.NotEqual(o1, o2);
        Assert.True(ReferenceEquals(o1, sut.GetOrCreate(ColidingName1)));
        Assert.True(ReferenceEquals(o2, sut.GetOrCreate(ColidingName2)));
    }
}