using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
using Melville.Parsing.MultiplexSources;
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
        var data = MultiplexSourceFactory.Create("""
            0001 0002
            0003 0004
            0005 0006
            0007
            0008
            """.BitsFromHex());

        var metrics = await new HorizontalMetricsParser(data.ReadPipeFrom(0), 3, 5).ParseAsync();

        metrics[0].Should().Be(new HorizontalMetric(1, 2));
        metrics[1].Should().Be(new HorizontalMetric(3, 4));
        metrics[2].Should().Be(new HorizontalMetric(5, 6));
        metrics[3].Should().Be(new HorizontalMetric(5, 7));
        metrics[4].Should().Be(new HorizontalMetric(5, 8));

    }
}