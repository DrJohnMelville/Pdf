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
    public PatternRenderer(
        Func<long, ValueTask<HasRenderableContentStream>> pageSource, int totalPages, 
        IDefaultFontMapper fontMapper, IDocumentPartCache cache, in Matrix3x2 patternTransform) : 
        base(pageSource, totalPages, fontMapper, cache)
    {
        this.patternTransform = patternTransform;
    }

    public override void InitializeRenderTarget(IRenderTarget innerRenderer, in PdfRect rect, double width, double height,
        in Matrix3x2 transform)
    {
        innerRenderer.MapUserSpaceToBitmapSpace(new PdfRect(0,0,40,40), 40,40, 
            patternTransform * Matrix3x2.CreateScale(1,-1)*Matrix3x2.CreateTranslation(0,40) );
    }
}