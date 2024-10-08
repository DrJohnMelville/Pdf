﻿using System;
using System.IO;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Xunit;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public class OffsetMultiplexedSourceTest: IDisposable
{
    private readonly IMultiplexSource sut  = MultiplexSourceFactory.Create(
        [0,1,2,3,4,5,6,7,8,9,10]);

    private void VerifyRead(Stream reader, params byte[] data)
    {
        var acutal = new byte[data.Length];
        Assert.Equal(data.Length, reader.Read(acutal, 0, data.Length));
        Assert.Equal(data, acutal);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1,0)]
    [InlineData(1,1)]
    [InlineData(5, 0)]
    [InlineData(5, 2)]
    public void OffsetRead(int offset, int pos)
    {
        using var sut2 = sut.OffsetFrom((uint)offset);
        var sum = offset + pos;
        using var readFrom = sut2.ReadFrom(pos); 
        VerifyRead(readFrom, [(byte)sum, (byte)(sum+1), (byte)(sum+2)]);
    }

    public void Dispose() => sut.Dispose();
}

