using System;
using System.Buffers;
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
    private long RunSearch(string item, EndSearchStrategy strategy)
    {
        var src = new SequenceReader<byte>(new ReadOnlySequence<byte>(item.AsExtendedAsciiBytes()));

        Assert.True(strategy.SearchForEndSequence(src, true, out var endPos));

        return src.Sequence.Slice(src.Sequence.Start, endPos).Length;
    }


    [Theory]
    [InlineData(" 12345EI", 8)]
    [InlineData(" 12345EI AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", 8)]
    [InlineData(" 2312345EI A", 10)]
    [InlineData(" 12345EI\x00D3AEI ", 12)]
    public void SearchWithoutLength(string item, int position)
    {
        var length = RunSearch(item, EndSearchStrategy.Instance);
        Assert.Equal(position, length);
    }

    [Theory]
    [InlineData(" 12345EI", 5, 8)]
    [InlineData(" 12345EI AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", 5, 8)]
    [InlineData(" 2312345EI A", 7, 10)]
    [InlineData(" 12345EI\x00D3AEI ", 9, 12)]
    [InlineData(" 12345EIAAEI ", 9, 12)] // cannot be found just by searching.
    public void SearchWithLength(string item, int declaredLength, int position)
    {
        var computedLen = RunSearch(item, new WithLengthSearchStrategy(declaredLength));
        Assert.Equal(position, computedLen);
    }

    [Theory]
    [InlineData(" 12345EI", 5, 8)]
    [InlineData(" 12345EI AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", 5, 8)]
    [InlineData(" 2312345EI A", 7, 10)]
    [InlineData(" 12345EI\x00D3AEI ", 9, 12)]
    [InlineData(" 12345EIAAEI ", 9, 12)] // cannot be found just by searching.
    public void SearchWithLengthAndSkipWhites(string item, int declaredLength, int position)
    {
        var computedLen = RunSearch("        " + item, new WithLengthSearchStrategy(declaredLength));
        Assert.Equal(position + 8, computedLen);

        computedLen =  RunSearch(item, new WithLengthSearchStrategy(declaredLength));
        Assert.Equal(position, computedLen);
    }
}