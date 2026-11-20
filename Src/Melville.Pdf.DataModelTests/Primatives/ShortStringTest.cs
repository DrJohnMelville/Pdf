using System;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.ShortStrings;

namespace Melville.Pdf.DataModelTests.Primatives;

internal readonly struct TestTarget : IShortStringTarget<IShortString>
{
    public IShortString Create(ShortString<NinePackedBytes> data) => data;
    public IShortString Create(ShortString<EighteenPackedBytes> data) => data;
    public IShortString Create(ShortString<ArbitraryBytes> data) => data;
}

public class ShortStringTest
{
    [Test]
    [Arguments("A")]
    [Arguments("A\xA0")]
    [Arguments("AB")]
    [Arguments("123456789")]
    [Arguments("1234567890")]
    [Arguments("123456789012345678")]
    [Arguments("1234567890123456789")]
    public void TestShortString(string data)
    {
        var dataAsArray = System.Text.Encoding.UTF8.GetBytes(data);
        var sut = new ReadOnlySpan<byte>(dataAsArray).WrapWith(new TestTarget());

        Assert.Equal(dataAsArray.Length, sut.Length());

        Assert.True(sut.SameAs(dataAsArray));

        Assert.Equal(data, sut.ValueAsString());

        Assert.Equal(FnvHash.FnvHashAsInt(dataAsArray), sut.ComputeHashCode());
    }

    [Test]
    [Arguments("A")]
    [Arguments("AB")]
    [Arguments("123456789")]
    public void PackedByteTest(string data)
    {
        var dataAsArray = data.AsExtendedAsciiBytes();
        var sut = new NinePackedBytes(dataAsArray);
        
        Assert.Equal(data.Length, sut.Length());
        
        Assert.True(sut.SameAs(dataAsArray));

        Span<byte> filled = stackalloc byte[data.Length];
        sut.Fill(filled);
        Assert.True(dataAsArray.AsSpan().SequenceEqual(filled));

        var hash = new FnvComputer();
        sut.AddToHash(ref hash);
        Assert.Equal(FnvHash.FnvHashAsInt(data), hash.HashAsInt());
    }

    [Test]
    [Arguments(1)]
    [Arguments(2)]
    [Arguments(8)]
    [Arguments(9)]
    public void NotSameAs(int len)
    {
        Span<byte> data = stackalloc byte[len];
        for (int i = 0; i < len; i++)
        {
            data[i] = 65;
        }

        var sut = new NinePackedBytes(data);
        Assert.True(sut.SameAs(data));
        Assert.False(sut.SameAs(data[..^1]));
        for (int i = 0; i < len; i++)
        {
            data[i] = 66;
            Assert.False(sut.SameAs(data));
            data[i] = 65;
        }
    }
}