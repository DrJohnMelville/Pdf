using FluentAssertions;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Melville.Pdf.DataModelTests.Standard.S8_9Images;

public class FastBitmapWriterTest
{
    [Test]
    public void SingleKernelTransform()
    {
        var source = new byte[] {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23};
        var output = new byte[32];

        var parser = new SpanParsingBuffer(source);
        var colors = output.AsSpan();
        VectorizedBitmapWriter256.WriteBytes(ref parser, ref colors);
        parser.CurrentLength.Should().Be(0);
        colors.IsEmpty.Should().BeTrue();
        output.Should().BeEquivalentTo(
        [2, 1, 0, 0xFF,
        5, 4, 3, 0xFF,
        8, 7, 6, 0xFF,
        11, 10, 9, 0xFF,
        14, 13, 12, 0xFF,
        17, 16, 15, 0xFF,
        20, 19, 18, 0xFF,
        23, 22, 21, 0xFF
        ]);
    }

    [Test]
    public void TwoKernelTransform()
    {
        var source = new byte[] {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23,
        };
        var output = new byte[64];

        var parser = new SpanParsingBuffer(source);
        var colors = output.AsSpan();
        FastBitmapWriterRGB8.Instance.WriteBytes(ref parser, ref colors);
        parser.CurrentLength.Should().Be(0);
        colors.IsEmpty.Should().BeTrue();
        output.Should().BeEquivalentTo(
        [2, 1, 0, 0xFF,
        5, 4, 3, 0xFF,
        8, 7, 6, 0xFF,
        11, 10, 9, 0xFF,
        14, 13, 12, 0xFF,
        17, 16, 15, 0xFF,
        20, 19, 18, 0xFF,
        23, 22, 21, 0xFF,
        2, 1, 0, 0xFF,
        5, 4, 3, 0xFF,
        8, 7, 6, 0xFF,
        11, 10, 9, 0xFF,
        14, 13, 12, 0xFF,
        17, 16, 15, 0xFF,
        20, 19, 18, 0xFF,
        23, 22, 21, 0xFF

        ]);
    }
}
