using System.Text;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.MetricViewers;

public class HorizontalMetricsViewModel
{
    public string Title => "Horizontal Metrics";
    public IList<string> Metrics { get; }
    public HorizontalMetricsViewModel(ParsedHorizontalMetrics metrics)
    {
        Metrics = new List<string>(metrics.HMetrics.Length + metrics.LeftSideBearings.Length + 1);
        int glyph = 0;
        ushort lastWidth = 0;
        foreach (var metric in metrics.HMetrics)
        {
            lastWidth = metric.AdvanceWidth;
            Metrics.Add($"0x{glyph++:X} => ({lastWidth}, {metric.LeftSideBearing})");
        }
        Metrics.Add("Implicit Metrics");
        foreach (var bearing in metrics.LeftSideBearings)
        {
            Metrics.Add($"{glyph++:X} => ({lastWidth}, {bearing})");
            
        }
        ;
    }
}