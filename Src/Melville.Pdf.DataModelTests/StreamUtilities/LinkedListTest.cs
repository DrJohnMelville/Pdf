using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Melville.Parsing.LinkedLists;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public class LinkedListTest: IDisposable
{
    private readonly LinkedList sut = new MultiBufferStreamList().With(16);
    public void Dispose()
    {
        sut.Dispose();
    }

    [Test]
    public void InitialPosition()
    {
        sut.StartPosition.GlobalPosition.Should().Be(0);
        sut.EndPosition.GlobalPosition.Should().Be(0);
        sut.EndPosition.Node.LocalLength.Should().Be(16);
        sut.StartPosition.Node.Should().BeSameAs(sut.EndPosition.Node);
    }

    [Test]
    public void Write16Bytes()
    {
        WriteNBytes(16);
        sut.StartPosition.GlobalPosition.Should().Be(0);
        sut.EndPosition.GlobalPosition.Should().Be(16);
        sut.EndPosition.Node.LocalLength.Should().Be(16);
        sut.StartPosition.Node.Should().BeSameAs(sut.EndPosition.Node);
    }

    private void WriteNBytes(int n) => sut.Write(sut.StartPosition, new byte[n]);

    [Test]
    public void Write17Bytes()
    {
        WriteNBytes(17);
        sut.StartPosition.GlobalPosition.Should().Be(0);
        sut.EndPosition.GlobalPosition.Should().Be(17);
        sut.EndPosition.Node.LocalLength.Should().Be(16);
        sut.StartPosition.Node.Next.Should().BeSameAs(sut.EndPosition.Node);
    }

    [Test]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(9)]
    [Arguments(14)]
    [Arguments(15)]
    [Arguments(16)]
    [Arguments(17)]
    [Arguments(25)]
    [Arguments(2500)]
    public void PositionAt(int pos)
    {
        WriteNBytes(pos);
        sut.PositionAt(pos).GlobalPosition.Should().Be(pos);
    }

}