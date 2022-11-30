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
    private ValueTask<BufferFromPipe> CreateSearchItem(string input) =>
        BufferFromPipe.Create(PipeReader.Create(new MemoryStream(input.AsExtendedAsciiBytes())));

        private async Task<long> RunSearch(string item, EndSearchStrategy strategy)
    {
        var src = await CreateSearchItem(item);
        SequencePosition endPos;
        while (!strategy.SearchForEndSequence(src, out endPos))
        {
            src = await src.InvalidateAndRefresh();
        }

        var length = src.Buffer.Slice(src.Buffer.Start, endPos).Length;
        return length;
    }


    [Theory]
    [InlineData(" 12345EI", 8)]
    [InlineData(" 12345EI AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", 8)]
    [InlineData(" 2312345EI A", 10)]
    [InlineData(" 12345EI\x00D3AEI ", 12)]
    public async Task SearchWithoutLength(string item, int position)
    {
        var length = await RunSearch(item, EndSearchStrategy.Instance);
        Assert.Equal(position, length);
    }

    [Theory]
    [InlineData(" 12345EI", 5, 8)]
    [InlineData(" 12345EI AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", 5, 8)]
    [InlineData(" 2312345EI A", 7, 10)]
    [InlineData(" 12345EI\x00D3AEI ", 9, 12)]
    [InlineData(" 12345EIAAEI ", 9, 12)] // cannot be found just by searching.
    public async Task SearchWithLength(string item, int declaredLength, int position)
    {
        var computedLen = await RunSearch(item, new WithLengthSearchStrategy(declaredLength));
        Assert.Equal(position, computedLen);
    }

    [Theory]
    [InlineData(" 12345EI", 5, 8)]
    [InlineData(" 12345EI AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", 5, 8)]
    [InlineData(" 2312345EI A", 7, 10)]
    [InlineData(" 12345EI\x00D3AEI ", 9, 12)]
    [InlineData(" 12345EIAAEI ", 9, 12)] // cannot be found just by searching.
    public async Task SearchWithLengthAndSkipWhites(string item, int declaredLength, int position)
    {
        var computedLen = await RunSearch("        " + item, new WithLengthSearchStrategy(declaredLength));
        Assert.Equal(position+8, computedLen);

        computedLen = await RunSearch(item, new WithLengthSearchStrategy(declaredLength));
        Assert.Equal(position, computedLen);
    }

}