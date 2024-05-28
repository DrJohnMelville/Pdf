using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;
using Melville.INPC;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.GlyphViewer;

public partial class GlyphsViewModel
{
    [FromConstructor] private IGlyphSource glyphSource;
    [AutoNotify] private IList<GlyphPointViewModel>? points;

    partial void OnConstructed()
    {
        PageSelector.MinPage = 0;
        PageSelector.MaxPage = glyphSource.GlyphCount;
        PageSelector.WhenMemberChanges(nameof(PageSelector.Page), LoadNewGlyph);
        LoadNewGlyph();
    }

    public PageSelectorViewModel PageSelector { get; } = new();

    private async void LoadNewGlyph()
    {
        var target = new GlyphPointRecorder();
        if (glyphSource is TrueTypeGlyphSource ttgs)
            await ttgs.ParsePointsAsync((uint)PageSelector.Page,target, Matrix3x2.Identity);
        Points = target.Points;
    }
}

public class GlyphPointRecorder : ITrueTypePointTarget
{
    public List<GlyphPointViewModel> Points { get; }  = new();
    public void AddPoint(double x, double y, bool onCurve, bool isContourStart, bool isContourEnd)
    {
        Points.Add(new(x, y, StatusString(onCurve, isContourStart, isContourEnd)));
    }

    private string StatusString(bool onCurve, bool isContourStart, bool isContourEnd) =>
      (onCurve ? "On " : "") + (isContourStart ? "Start" : "") + (isContourEnd ? "End" : "");
}

public record GlyphPointViewModel(double X, double Y, string Type);