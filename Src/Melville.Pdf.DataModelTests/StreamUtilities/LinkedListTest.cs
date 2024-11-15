using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Melville.Parsing.LinkedLists;
using Xunit;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public class LinkedListTest: IDisposable
{
    private readonly LinkedList sut = new MultiBufferStreamList().With(16);
    public void Dispose()
    {
        sut.Dispose();
    }

    [Fact]
    public void InitialPosition()
    {
        sut.StartPosition.GlobalPosition.Should().Be(0);
        sut.EndPosition.GlobalPosition.Should().Be(0);
        sut.EndPosition.Node.LocalLength.Should().Be(16);
        sut.StartPosition.Node.Should().BeSameAs(sut.EndPosition.Node);
    }

    [Fact]
    public void Write16Bytes()
    {
        WriteNBytes(16);
        sut.StartPosition.GlobalPosition.Should().Be(0);
        sut.EndPosition.GlobalPosition.Should().Be(16);
        sut.EndPosition.Node.LocalLength.Should().Be(16);
        sut.StartPosition.Node.Should().BeSameAs(sut.EndPosition.Node);
    }

    private void WriteNBytes(int n) => sut.Write(sut.StartPosition, new byte[n]);

    [Fact]
    public void Write17Bytes()
    {
        WriteNBytes(17);
        sut.StartPosition.GlobalPosition.Should().Be(0);
        sut.EndPosition.GlobalPosition.Should().Be(17);
        sut.EndPosition.Node.LocalLength.Should().Be(16);
        sut.StartPosition.Node.Next.Should().BeSameAs(sut.EndPosition.Node);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(9)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(25)]
    [InlineData(2500)]
    public void PositionAt(int pos)
    {
        WriteNBytes(pos);
        sut.PositionAt(pos).GlobalPosition.Should().Be(pos);
    }

}