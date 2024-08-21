using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.ReferenceDocuments.Graphics;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt;

public class HorizontalMetricReaderTest
{
    [Fact]
    public async Task ReadHMetricsAsync()
    {
        // 3 hMetrics and 2 left side bearings
        using var data = MultiplexSourceFactory.Create("""
            0001 0002
            0003 0004
            0005 0006
            0007
            0008
            """.BitsFromHex());

        using var pipe = data.ReadPipeFrom(0);
        var metrics = await new HorizontalMetricsParser(pipe, 3, 5, 1)
            .ParseAsync();

        metrics[0].Should().Be(new HorizontalMetric(1, 2));
        metrics[1].Should().Be(new HorizontalMetric(3, 4));
        metrics[2].Should().Be(new HorizontalMetric(5, 6));
        metrics[3].Should().Be(new HorizontalMetric(5, 7));
        metrics[4].Should().Be(new HorizontalMetric(5, 8));

        metrics.GlyphWidth(0).Should().Be(1);
        metrics.GlyphWidth(1).Should().Be(3);
        metrics.GlyphWidth(2).Should().Be(5);
        metrics.GlyphWidth(3).Should().Be(5);
        metrics.GlyphWidth(4).Should().Be(5);
        metrics.GlyphWidth(40).Should().Be(5);


    }
}