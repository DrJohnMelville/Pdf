using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_9Images;

public class InlineImageDEtectLengthTest
{
    private ValueTask<BufferFromPipe> CreateSearchItemAsync(string input) =>
        BufferFromPipe.CreateAsync(PipeReader.Create(new MemoryStream(input.AsExtendedAsciiBytes())));

        private async Task<long> RunSearchAsync(string item, EndSearchStrategy strategy)
    {
        var src = await CreateSearchItemAsync(item);
        SequencePosition endPos;
        while (!strategy.SearchForEndSequence(src, out endPos))
        {
            src = await src.InvalidateAndRefreshAsync();
        }

        var length = src.Buffer.Slice(src.Buffer.Start, endPos).Length;
        return length;
    }


    [Theory]
    [InlineData(" 12345EI", 8)]
    [InlineData(" 12345EI AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", 8)]
    [InlineData(" 2312345EI A", 10)]
    [InlineData(" 12345EI\x00D3AEI ", 12)]
    public async Task SearchWithoutLengthAsync(string item, int position)
    {
        var length = await RunSearchAsync(item, EndSearchStrategy.Instance);
        Assert.Equal(position, length);
    }

    [Theory]
    [InlineData(" 12345EI", 5, 8)]
    [InlineData(" 12345EI AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", 5, 8)]
    [InlineData(" 2312345EI A", 7, 10)]
    [InlineData(" 12345EI\x00D3AEI ", 9, 12)]
    [InlineData(" 12345EIAAEI ", 9, 12)] // cannot be found just by searching.
    public async Task SearchWithLengthAsync(string item, int declaredLength, int position)
    {
        var computedLen = await RunSearchAsync(item, new WithLengthSearchStrategy(declaredLength));
        Assert.Equal(position, computedLen);
    }

    [Theory]
    [InlineData(" 12345EI", 5, 8)]
    [InlineData(" 12345EI AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", 5, 8)]
    [InlineData(" 2312345EI A", 7, 10)]
    [InlineData(" 12345EI\x00D3AEI ", 9, 12)]
    [InlineData(" 12345EIAAEI ", 9, 12)] // cannot be found just by searching.
    public async Task SearchWithLengthAndSkipWhitesAsync(string item, int declaredLength, int position)
    {
        var computedLen = await RunSearchAsync("        " + item, new WithLengthSearchStrategy(declaredLength));
        Assert.Equal(position+8, computedLen);

        computedLen = await RunSearchAsync(item, new WithLengthSearchStrategy(declaredLength));
        Assert.Equal(position, computedLen);
    }

}