using System;
using System.Buffers;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt.CFFOutlines;

public class DictScannerTest
{
    private static DictScanner CreateScanner(string bytes, int operatorDesired, Span<DictValue> operands)
    {
        return new DictScanner(
            new SequenceReader<byte>(new ReadOnlySequence<byte>(bytes.BitsFromHex())),
            operatorDesired, operands);
    }
    [Fact]
    public void ScanEmptyDict()
    {
        CreateScanner("", 2, stackalloc DictValue[1])
            .TryFindEntry().Should().BeFalse();
    }

    [Theory]
    [InlineData("2002", -107)]
    [InlineData("8B02", 0)]
    [InlineData("efefefefefefefef 8B02", 0)]// skip extra arguments
    [InlineData("ef02", 100)]
    [InlineData("2702", -100)]
    [InlineData("FA7C 02", 1_000)]
    [InlineData("FE7C 02", -1_000)]
    [InlineData("1C2710 02", 10_000)]
    [InlineData("1CD8F0 02", -10_000)]
    [InlineData("1D000186a0 02", 100_000)]
    [InlineData("1DFFFE7960 02", -100_000)]
    public void FindInteger(string bytes, int value)
    {
        Span<DictValue> operands = stackalloc DictValue[1];
        var scanner = CreateScanner(bytes, 2, operands);
        scanner.TryFindEntry().Should().BeTrue();
        operands[0].IntValue.Should().Be(value);
    }

    [Theory]
    [InlineData("1EE2A25F 02", -2.25)]
    [InlineData("1E0A140541c3FF 02", 0.140541E-3)]
    public void FindFloat(string bytes, float value)
    {
        Span<DictValue> operands = stackalloc DictValue[1];
        var scanner = CreateScanner(bytes, 2, operands);
        scanner.TryFindEntry().Should().BeTrue();
        operands[0].FloatValue.Should().Be(value);
    }

}