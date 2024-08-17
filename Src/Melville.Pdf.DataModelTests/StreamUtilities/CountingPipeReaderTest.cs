using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Xunit;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public class CountingPipeReaderTest: IDisposable
{
    private readonly IByteSource sut;

    public CountingPipeReaderTest()
    {
        using var multiplexSource = MultiplexSourceFactory.Create(new byte[30]);
        sut = multiplexSource.ReadPipeFrom(0);
    }

    [Fact]
    public void StartsAtZero()
    {
        Assert.Equal(0, sut.Position);
    }
    [Fact]
    public async Task OrdinaryReadIncrementsAsync()
    {
        var res = await sut.ReadAsync(); 
        Assert.Equal(0, sut.Position);
        sut.AdvanceTo(res.Buffer.GetPosition(2));
        Assert.Equal(2, sut.Position);
        res = await sut.ReadAsync(); 
        Assert.Equal(2, sut.Position);
        sut.AdvanceTo(res.Buffer.GetPosition(2));
        Assert.Equal(4, sut.Position);
    }
    [Fact]
    public async Task SyncOrdinaryReadIncrementsAsync()
    {
        sut.AdvanceTo((await sut.ReadAsync()).Buffer.Start);
        Assert.True(sut.TryRead(out var res));
        Assert.Equal(0, sut.Position);
        sut.AdvanceTo(res.Buffer.GetPosition(2));
        Assert.Equal(2, sut.Position);
        Assert.True(sut.TryRead(out res));
        Assert.Equal(2, sut.Position);
        sut.AdvanceTo(res.Buffer.GetPosition(3));
        Assert.Equal(5, sut.Position);
    }

    public void Dispose()
    {
        sut.Dispose();
    }
}