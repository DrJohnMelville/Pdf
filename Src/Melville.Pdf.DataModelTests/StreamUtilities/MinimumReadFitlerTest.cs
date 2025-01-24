using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.StreamFilters;
using Melville.Parsing.Streams.Bases;
using Xunit;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public sealed class MinimumReadFitlerTest
{
    private class SizeReportingStream: DefaultBaseStream
    {
        public SizeReportingStream() : base(true, false, false)
        {
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
        {
            var datum = (byte) Math.Min(255, buffer.Length);
            for (int i = 0; i < 10; i++)
            {
                buffer.Span[i] = datum;
            }

            return new (10);
        }
    }
        
    private readonly Stream sut = new MinimumReadSizeFilter(new SizeReportingStream(), 10);

    [Fact]
    public async Task BigEnoughReadSucceedsAsync()
    {
        var buf = new byte[11];
        await sut.ReadExactlyAsync(buf, 0, 11);
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(11, buf[i]);
        }
    }

    [Fact]
    public async Task ReadBiggerThanIThoughtAsync()
    {
        var buf = new byte[1];
        for (var i = 0; i < 10; i++)
        {
            await sut.ReadExactlyAsync(buf, 0, 1);
            Assert.True(buf[0] > 9);
        }
    }
}