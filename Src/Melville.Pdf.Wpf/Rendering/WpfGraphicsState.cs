using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;
using Melville.Pdf.Model.Renderers.Patterns.TilePatterns;

namespace Melville.Pdf.Wpf.Rendering;


public class WpfGraphicsState : GraphicsState<Func<Brush>>
{
    protected override Func<Brush> CreateSolidBrush(DeviceColor color)
    {
        var brush = new SolidColorBrush(color.AsWpfColor());
        return  ()=>brush;
    }

    protected override async ValueTask<Func<Brush>> CreatePatternBrush(PdfDictionary pattern,
        DocumentRenderer parentRenderer) =>
        await pattern.GetOrDefaultAsync(KnownNames.PatternType, 0).CA() switch
        {
            1 => await CreateTilePattern(pattern, parentRenderer),
            2 => await CreateShaderBrush(pattern),
            _ => ()=>Brushes.Transparent,
        };

    private Brush AdjustPattern(
        TileBrush tileBrush, in  Matrix3x2 patternMatrix)
    {
        tileBrush.ViewboxUnits = BrushMappingMode.Absolute;
        tileBrush.ViewportUnits = BrushMappingMode.Absolute;
        
        // The graphics state stack might change in the interval when the brush is created and used
        // By the time this method run this class may not be the top of the graphics state stack.  I pass
        // in a reference to the top state at the time of the usage
        var final = this.RevertToPixelsMatrix();
        tileBrush.Transform = (patternMatrix * final).WpfTransform();
        return tileBrush;
    }

    private async Task<Func<Brush>> CreateTilePattern(
        PdfDictionary pattern, DocumentRenderer parentRenderer)
    {
        var request = await TileBrushRequest.Parse(pattern);
        var pattternItem = await PatternRenderer(parentRenderer, request).Render();
        return ()=> AdjustPattern( CreatePatternBrush(pattternItem, request), request.PatternTransform);
    }

    private static DrawingBrush CreatePatternBrush(DrawingGroup pattternItem, TileBrushRequest request) =>
        new(pattternItem)
        {
            Stretch = Stretch.None,
            Viewbox = PatternSourceBox(request.BoundingBox),
            Viewport = PatternDestinationBox(request.RepeatSize),
            TileMode = TileMode.Tile,
        };

    private static Rect PatternDestinationBox(Vector2 repeatSize) => 
        new Rect(0, 0, repeatSize.X,repeatSize.Y);

    private static Rect PatternSourceBox(PdfRect bbox) => bbox.AsWpfRect();

    private RenderToDrawingGroup PatternRenderer(
        DocumentRenderer parentRenderer, in TileBrushRequest request)
    {
        return new RenderToDrawingGroup(parentRenderer.PatternRenderer(request, this), 0);
    }
    
    public async ValueTask<Func<Brush>> CreateShaderBrush(PdfDictionary pattern)
    {
        var bmp = RenderShaderToBitmap(await ShaderParser.ParseShader(pattern));
        var viewport = new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);
        var brush = new ImageBrush(bmp)
        {
            Stretch = Stretch.None,
            Viewbox = viewport,
            Viewport = viewport,
        };

        return () => AdjustPattern(brush, Matrix3x2.Identity);
    }

    private WriteableBitmap RenderShaderToBitmap(IShaderWriter writer)
    {
        var bmp = new WriteableBitmap((int)PageWidth, (int)PageHeight, 96, 96, PixelFormats.Pbgra32,
            null);
        bmp.Lock();
        unsafe
        { 
            writer.RenderBits((uint*)bmp.BackBuffer, bmp.PixelWidth, bmp.PixelHeight);
        }
        bmp.Unlock();
        return bmp;
    }
}