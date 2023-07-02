using System;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_4Text;

public class TextObjectOperationsTest : WriterTest
{
    [Fact]
    public async Task EmptyBlockAsync()
    {
        using (var block = sut.StartTextBlock())
        {
        }

        Assert.Equal("BT\nET\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task MovePositionByAsync()
    {
        using (var block = sut.StartTextBlock())
        {
            block.MovePositionBy(3, 4);
        }

        Assert.Equal("BT\n3 4 Td\nET\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task MovePositionByWithLeadingAsync()
    {
        using (var block = sut.StartTextBlock())
        {
            block.MovePositionByWithLeading(3, 4);
        }

        Assert.Equal("BT\n3 4 TD\nET\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task SetTextMatrixAsync()
    {
        using (var block = sut.StartTextBlock())
        {
            block.SetTextMatrix(1, 2, 3, 4, 5, 6);
        }

        Assert.Equal("BT\n1 2 3 4 5 6 Tm\nET\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task ToNextLineAsync()
    {
        using (var block = sut.StartTextBlock())
        {
            block.MoveToNextTextLine();
        }

        Assert.Equal("BT\nT*\nET\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task ShowStringAsync()
    {
        using (var block = sut.StartTextBlock())
        {
            await block.ShowStringAsync("ABC");
        }

        Assert.Equal("BT\n(ABC)Tj\nET\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task NextLineAndShowStringAsync()
    {
        using (var block = sut.StartTextBlock())
        {
            await block.MoveToNextLineAndShowStringAsync("ABC");
        }

        Assert.Equal("BT\n(ABC)'\nET\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task NextLineAndShowString2Async()
    {
        using (var block = sut.StartTextBlock())
        {
            await block.MoveToNextLineAndShowStringAsync(2, 3, "ABC");
        }

        Assert.Equal("BT\n2 3(ABC)\"\nET\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task ShowSpacedStringAsync()
    {
        using (var block = sut.StartTextBlock())
        {
            await block.ShowSpacedStringAsync("A", 2, "B", 3, "C", "D", 4, 5);
        }

        Assert.Equal("BT\n[(A)2 (B)3 (C)(D)4 5 ]TJ\nET\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task ShowSpacedString2Async()
    {
        using (var block = sut.StartTextBlock())
        {
            var builder = sut.GetSpacedStringBuilder();
            await builder.SpacedStringComponentAsync("A".AsExtendedAsciiBytes().AsMemory());
            await builder.SpacedStringComponentAsync(2);
            await builder.SpacedStringComponentAsync("B".AsExtendedAsciiBytes().AsMemory());
            await builder.SpacedStringComponentAsync(3);
            await builder.SpacedStringComponentAsync("C".AsExtendedAsciiBytes().AsMemory());
            await builder.SpacedStringComponentAsync("D".AsExtendedAsciiBytes().AsMemory());
            await builder.SpacedStringComponentAsync(4);
            await builder.SpacedStringComponentAsync(5);
            await builder.DoneWritingAsync();
        }
        Assert.Equal("BT\n[(A)2 (B)3 (C)(D)4 5 ]TJ\nET\n", await WrittenTextAsync());
    }
}