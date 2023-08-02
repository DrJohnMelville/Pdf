using System;
using System.IO;
using System.IO.Pipelines;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.ColorOperations;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

namespace Melville.Pdf.Model.Renderers;

internal partial class RenderEngine: IContentStreamOperations, IFontTarget, ISpacedStringBuilder
{
    private readonly IHasPageAttributes page;
    private readonly SinglePageRenderContext pageRenderContext;
    private readonly PathDrawingAdapter pathDrawing;
    private readonly SwitchingColorStrategy colorSwitcher;
    [DelegateTo] private IColorOperations colorOperations => colorSwitcher.CurrentTarget;
    

    public RenderEngine(IHasPageAttributes page, SinglePageRenderContext pageRenderContext)
    {
        this.page = page;
        this.pageRenderContext = pageRenderContext;
        colorSwitcher = pageRenderContext.CreateColorSwitcher(page);
        pathDrawing = new PathDrawingAdapter(pageRenderContext.Target.GraphicsState, null);
    }

    #region Graphics State
    [DelegateTo] private IGraphicsState StateOps => pageRenderContext.Target.GraphicsState;
    private GraphicsState GraphicsState => StateOps.CurrentState();
    
    
    public void ModifyTransformMatrix(in Matrix3x2 newTransform)
    {
        if (newTransform.IsIdentity) return;
        StateOps.ModifyTransformMatrix(in newTransform);
    }


    public async ValueTask LoadGraphicStateDictionaryAsync(PdfDirectObject dictionaryName) =>
         await GraphicsState.LoadGraphicStateDictionaryAsync(
            (await page.GetResourceAsync(ResourceTypeName.ExtGState, dictionaryName).CA())
            .TryGet(out PdfDictionary? dict)? dict: PdfDictionary.Empty).CA();
    #endregion
    
    #region Drawing Operations
    public IDrawTarget CreateDrawTarget() => 
        pageRenderContext.OptionalContent.WrapDrawTarget(
            pageRenderContext.Target.CreateDrawTarget());

    [DelegateTo]
    private IPathDrawingOperations PathDrawingOperations() =>
        pathDrawing.IsInvalid? pathDrawing.WithNewTarget(CreateDrawTarget()): pathDrawing;

    public async ValueTask DoAsync(PdfDirectObject name) =>
        await DoAsync((await page.GetResourceAsync(ResourceTypeName.XObject, name).CA())
            .TryGet(out PdfStream? stream)? 
            stream: throw new PdfParseException("Co command can only be called on Streams")).CA();

    public async ValueTask DoAsync(PdfStream inlineImage)
    {
        if (await pageRenderContext.OptionalContent.CanSkipXObjectDoOperationAsync(
                await inlineImage.GetOrNullAsync<PdfDictionary>(KnownNames.OCTName).CA()).CA())
            return;
        switch (inlineImage.SubTypeOrNull())
        {
            case {IsNull:true}:
            case var x when x.Equals(KnownNames.ImageTName):
                await TryRenderBitmapAsync(inlineImage).CA();
                break;
            case var x when x.Equals(KnownNames.FormTName):
                await RunTargetGroupAsync(inlineImage).CA();
                break;
            default: throw new PdfParseException("Cannot do the provided object");
        }
    }

    private async Task TryRenderBitmapAsync(PdfStream inlineImage)
    {
        try
        {
            await pageRenderContext.Target.RenderBitmapAsync(
                await inlineImage.WrapForRenderingAsync(page, GraphicsState.NonstrokeColor).CA()).CA();
        }
        catch (Exception)
        {
            // any error in loading the bitmap causes the bitmap to be ignored.
        }
    }

    public async ValueTask PaintShaderAsync(PdfDirectObject name)
    {
        var shader = await page.GetResourceAsync(ResourceTypeName.Shading, name).CA();
        if (!shader.TryGet(out PdfDictionary? shaderDict)) return;
        var factory = await ShaderParser.ParseShaderAsync(
            GraphicsState.TransformMatrix, shaderDict, true).CA();
        StateOps.SaveGraphicsState();
        MapBitmapToViewport();
        await pageRenderContext.Target.RenderBitmapAsync(new ShaderBitmap(factory,
            (int)GraphicsState.PageWidth, (int)GraphicsState.PageHeight)).CA();
        StateOps.RestoreGraphicsState();
    }

    private void MapBitmapToViewport()
    {
        var state = GraphicsState;
        if (!Matrix3x2.Invert(state.TransformMatrix, out var inv)) return;
        ModifyTransformMatrix(Matrix3x2.CreateScale(
            (float)state.PageWidth, (float)state.PageHeight)*inv);
    }
    
    private async ValueTask RunTargetGroupAsync(PdfStream xObjectAsStream)
    {
        SaveGraphicsState();
        var formXObject = new PdfFormXObject(xObjectAsStream, page);
        await TryApplyFormXObjectMatrixAsync(formXObject).CA();
        
        await TryClipToFormXObjectBoundingBoxAsync(formXObject).CA();

        await RenderAsync(formXObject).CA();
        RestoreGraphicsState();
    }

    private async Task TryApplyFormXObjectMatrixAsync(PdfFormXObject formXObject)
    {
        ModifyTransformMatrix(await formXObject.MatrixAsync().CA());
    }

    private async Task TryClipToFormXObjectBoundingBoxAsync(PdfFormXObject formXObject)
    {
        if ((await formXObject.BboxAsync().CA()) is {} clipRect )
        {
            Rectangle(clipRect.Left, clipRect.Bottom, clipRect.Width, clipRect.Height);
            ClipToPath();
            EndPathWithNoOp();
        }
    }

    private async ValueTask RenderAsync(IHasPageAttributes xObject)
    {
        if (!pageRenderContext.ItemsBeingRendered.TryPush(xObject.LowLevel)) return;
        var otherEngine = new RenderEngine(xObject, pageRenderContext);
        await otherEngine.RunContentStreamAsync().CA();
        CopyLastGlyphMetrics(otherEngine);
        pageRenderContext.ItemsBeingRendered.PopItem();
    }

    public async ValueTask RunContentStreamAsync() =>
        await new ContentStreamParser(this).ParseAsync(
            PipeReader.Create(await page.GetContentBytesAsync().CA())).CA();

    #endregion
    
    #region Text Operations

    public void BeginTextObject() => GraphicsState.SetBothTextMatrices(Matrix3x2.Identity);

    public void EndTextObject()
    {
    }

    public void MovePositionBy(double x, double y) =>
        GraphicsState.SetBothTextMatrices(
            Matrix3x2.CreateTranslation((float)x,(float)y) 
            * GraphicsState.TextLineMatrix);

    public void MovePositionByWithLeading(double x, double y)
    {
        StateOps.SetTextLeading(-y);
        MovePositionBy(x,y);
    }
 
    public void SetTextMatrix(double a, double b, double c, double d, double e, double f) =>
        GraphicsState.SetBothTextMatrices(new Matrix3x2(
            (float)a,(float)b,(float)c,(float)d,(float)e,(float)f));

    public void MoveToNextTextLine() => 
        MovePositionBy(0, - GraphicsState.TextLeading);

    public async ValueTask SetFontAsync(PdfDirectObject font, double size)
    {
        var fontResource = await page.GetResourceAsync(ResourceTypeName.Font, font).CA();
        var genericRealizedFont = fontResource.TryGet(out PdfDictionary fontDic) ?
            await FontFromDictionaryAsync(fontDic).CA():
            await SystemFontFromNameAsync(font).CA();
        
        GraphicsState.SetTypeface(await RendererSpecificFontAsync(genericRealizedFont).CA());
        await GraphicsState.SetFontAsync(font,size).CA();
    }

    private ValueTask<IRealizedFont> SystemFontFromNameAsync(PdfDirectObject font) =>
        FontFromDictionaryAsync(new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type1TName)
            .WithItem(KnownNames.BaseFontTName, font)
            .AsDictionary()
        );

    private async ValueTask<IRealizedFont> FontFromDictionaryAsync(PdfDictionary fontDic) => 
        BlockFontDispose.AsNonDisposableTypeface(await CheckCacheForFontAsync(fontDic).CA());

    private ValueTask<IRealizedFont> RendererSpecificFontAsync(IRealizedFont typeFace) =>
        pageRenderContext.Renderer.Cache.GetAsync(typeFace, r=> new ValueTask<IRealizedFont>(pageRenderContext.Target.WrapRealizedFont(r)));

    private ValueTask<IRealizedFont> CheckCacheForFontAsync(PdfDictionary fontDic) =>
        pageRenderContext.Renderer.Cache.GetAsync(fontDic, r=> FontReader().DictionaryToRealizedFontAsync(r));
     
    private FontReader FontReader() => new(pageRenderContext.Renderer.FontMapper);

    public async ValueTask ShowStringAsync(ReadOnlyMemory<byte> decodedString)
    {
        var font = GraphicsState.Typeface;
        using var writer = font.BeginFontWrite(this);
        var remainingI = decodedString;
        while (remainingI.Length > 0)
        {
            var (character, glyph) = GetNextCharacterAndGlyph(font, ref remainingI);
            var measuredGlyphWidth = await writer.AddGlyphToCurrentStringAsync(
                character, glyph, CharacterPositionMatrix()).CA();
            AdjustTextPositionForCharacter(font.CharacterWidth(character, measuredGlyphWidth), character);
        }
        writer.RenderCurrentString(GraphicsState.TextRender, CharacterPositionMatrix());
    }

    private static (uint character, uint glyph) GetNextCharacterAndGlyph(
        IRealizedFont font, ref ReadOnlyMemory<byte> remainingI)
    {
        var (character, glyph, bytesUsed) = font.GetNextGlyph(remainingI.Span);
        remainingI = remainingI[bytesUsed..];
        return (character, glyph);
    }

    private Matrix3x2 CharacterPositionMatrix() => GraphicsState.GlyphTransformMatrix();


    private void AdjustTextPositionForCharacter(double width, uint character)
    {
        var delta = CharacterSpacingAdjustment(character);
        UpdateTextPosition(width*GraphicsState.FontSize+delta);
    }

    private double CharacterSpacingAdjustment(uint character) =>
        GraphicsState.CharacterSpacing + ApplicableWordSpacing(character);

    private double ApplicableWordSpacing(uint character) => 
        IsSpaceCharacter(character)? GraphicsState.WordSpacing:0;

    private bool IsSpaceCharacter(uint character) => character == 0x20;

    private void UpdateTextPosition(double width)
    { 
        GraphicsState.SetTextMatrix(
            IncrementAlongActiveVector(ScaleHorizontalOffset(width))*
            GraphicsState.TextMatrix
        );
    }

    private double ScaleHorizontalOffset(double width) => 
        width * GraphicsState.HorizontalTextScale;

    private Matrix3x2 IncrementAlongActiveVector(double width) =>
            Matrix3x2.CreateTranslation((float)width, 0.0f);

    public ValueTask MoveToNextLineAndShowStringAsync(ReadOnlyMemory<byte> decodedString)
    {
        MoveToNextTextLine();
        return ShowStringAsync(decodedString);
    }

    public ValueTask MoveToNextLineAndShowStringAsync(double wordSpace, double charSpace, ReadOnlyMemory<byte> decodedString)
    {
        SetWordSpace(wordSpace);
        SetCharSpace(charSpace);
        return MoveToNextLineAndShowStringAsync(decodedString);
    }

    public ISpacedStringBuilder GetSpacedStringBuilder() => this;
    ValueTask ISpacedStringBuilder.SpacedStringComponentAsync(double value)
    {
        var delta = GraphicsState.FontSize * value/ 1000.0;
        UpdateTextPosition(-delta);
        return ValueTask.CompletedTask;
    }

    ValueTask ISpacedStringBuilder.SpacedStringComponentAsync(Memory<byte> value) => 
        ShowStringAsync(value);

    ValueTask ISpacedStringBuilder.DoneWritingAsync() => ValueTask.CompletedTask;

    #endregion

    #region Type 3 font rendering
    public async ValueTask<double> RenderType3CharacterAsync(Stream s, Matrix3x2 fontMatrix,
        PdfDictionary fontDictionary)
    {
        if (!(GraphicsState.TextRender is TextRendering.Invisible or TextRendering.Clip))
        {
            await DrawType3CharacterAsync(s, fontMatrix, fontDictionary).CA();
            colorSwitcher.TurnOn();
        }
        var ret = CharacterSizeInTextSpace(fontMatrix);
        return ret.X;
    }

    private async Task DrawType3CharacterAsync(Stream s, Matrix3x2 fontMatrix, PdfDictionary fontDictionary)
    {
        SaveGraphicsState();
        var textMatrix = CharacterPositionMatrix();
        ModifyTransformMatrix(fontMatrix * textMatrix);
        await RenderAsync(new Type3FontPseudoPage(page, fontDictionary, s)).CA();
      RestoreGraphicsState();
    }
    
    private Vector2 CharacterSizeInTextSpace(Matrix3x2 fontMatrix) =>
        Vector2.Transform(new Vector2((float)lastWx, (float)(lastUry - lastLly)),
            fontMatrix);

    private double lastWx, lastWy, lastLlx, lastLly, lastUrx, lastUry;

    private void CopyLastGlyphMetrics(RenderEngine other)
    {
        lastWx = other.lastWx;
        lastWy = other.lastWy;
        lastLlx = other.lastLlx;
        lastLly = other.lastLly;
        lastUrx = other.lastUrx;
        lastUry = other.lastUry;
    }

    public void SetColoredGlyphMetrics(double wX, double wY)
    {
        lastWx = wX;
        lastWy = wY;
    }

    public void SetUncoloredGlyphMetrics(double wX, double wY, double llX, double llY, double urX, double urY)
    {
        SetColoredGlyphMetrics(wX, wY);
        lastLlx = llX;
        lastLly = llY;
        lastUrx = urX;
        lastUry = urY;
        colorSwitcher.TurnOff();
    }

    #endregion

    #region Marked Operations

    public void MarkedContentPoint(PdfDirectObject tag) { }

    public ValueTask MarkedContentPointAsync(PdfDirectObject tag, PdfDirectObject properties) => ValueTask.CompletedTask;

    public ValueTask MarkedContentPointAsync(PdfDirectObject tag, PdfDictionary dictionary) => ValueTask.CompletedTask;

    public void BeginMarkedRange(PdfDirectObject tag) {}

    public ValueTask BeginMarkedRangeAsync(PdfDirectObject tag, PdfDirectObject dictName) => 
        pageRenderContext.OptionalContent.EnterGroupAsync(tag, dictName, page);

    public ValueTask BeginMarkedRangeAsync(PdfDirectObject tag, PdfDictionary dictionary) =>
        pageRenderContext.OptionalContent.EnterGroupAsync(tag, dictionary);

    public void EndMarkedRange() => pageRenderContext.OptionalContent.PopContentGroup();
    #endregion

    #region Compatability Operators
    public void BeginCompatibilitySection() { }
    public void EndCompatibilitySection() { }
    #endregion
}