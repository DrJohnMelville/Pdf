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


public class WpfGraphicsState : GraphicsState<Brush>
{
    protected override Brush CreateSolidBrush(DeviceColor color) => new SolidColorBrush(color.AsWpfColor());

    protected override async ValueTask<Brush> CreatePatternBrush(
        PdfDictionary pattern, DocumentRenderer parentRenderer) =>
        await pattern.GetOrDefaultAsync(KnownNames.PatternType,0).CA() switch
        {
            1 => await CreateTilePattern(pattern, parentRenderer),
            2 => await CreateShaderBrush(pattern),
            _=> Brushes.Transparent,
        };

    private async Task<Brush> CreateTilePattern(PdfDictionary pattern, DocumentRenderer parentRenderer)
    {
        var request = await TileBrushRequest.Parse(pattern);
        var pattternItem = await PatternRenderer(parentRenderer, request).Render();
        return CreateBrush(pattternItem, request);
    }

    private static Brush CreateBrush(DrawingGroup pattternItem, in TileBrushRequest request) =>
        new DrawingBrush(pattternItem)
        {
            Stretch = Stretch.None,
            Viewbox = PatternSourceBox(request.BoundingBox),
            ViewboxUnits = BrushMappingMode.Absolute,
            Viewport = PatternDestinationBox(request.RepeatSize),
            ViewportUnits = BrushMappingMode.Absolute,
            TileMode = TileMode.Tile,
            Transform = request.PatternTransform.WpfTransform()
        };

    private static Rect PatternDestinationBox(Vector2 repeatSize) => 
        new Rect(0, 0, repeatSize.X,repeatSize.Y);

    private static Rect PatternSourceBox(PdfRect bbox) => bbox.AsWpfRect();

    private RenderToDrawingGroup PatternRenderer(
        DocumentRenderer parentRenderer, in TileBrushRequest request)
    {
        return new RenderToDrawingGroup(parentRenderer.PatternRenderer(request, this), 0);
    }
    
    public async ValueTask<Brush> CreateShaderBrush(PdfDictionary pattern)
    {
        var bmp = RenderShaderToBitmap(await new ShaderParser(pattern,
            await pattern.GetAsync<PdfDictionary>(KnownNames.Shading)).ParseShader());
        var viewport = new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);
        return new ImageBrush(bmp)
        {
            Stretch = Stretch.None,
            Viewbox = viewport,
            Viewport = viewport,
            ViewboxUnits = BrushMappingMode.Absolute,
            ViewportUnits = BrushMappingMode.Absolute
        };
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