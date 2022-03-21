using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model.Renderers;

public class PatternRenderer: DocumentRenderer
{
    private readonly Matrix3x2 patternTransform;
    private readonly PdfRect bounds;
    public PatternRenderer(
        Func<long, ValueTask<HasRenderableContentStream>> pageSource, int totalPages, 
        IDefaultFontMapper fontMapper, IDocumentPartCache cache, in Matrix3x2 patternTransform, in PdfRect bounds) : 
        base(pageSource, totalPages, fontMapper, cache)
    {
        this.patternTransform = patternTransform;
        this.bounds = bounds;
    }

    public override void InitializeRenderTarget(IRenderTarget innerRenderer, in PdfRect rect, double width, double height,
        in Matrix3x2 transform)
    {
        innerRenderer.MapUserSpaceToBitmapSpace(bounds, bounds.Width,bounds.Height, 
            patternTransform * InvertYDimension() );
    }

    private Matrix3x2 InvertYDimension() => 
        Matrix3x2.CreateScale(1,-1)*Matrix3x2.CreateTranslation(0,(float)bounds.Height);
}