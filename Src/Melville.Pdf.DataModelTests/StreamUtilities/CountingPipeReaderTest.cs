﻿using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.CountingReaders;
using Xunit;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public class CountingPipeReaderTest
{
    private readonly ByteSource sut;

    public CountingPipeReaderTest()
    {
        var src = new MemoryStream(new byte[30]);
        sut = new(PipeReader.Create(src));
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
}