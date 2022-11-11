using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.OptionalContents;
using Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

namespace Melville.Pdf.Model.Renderers;

public partial class RenderEngine: IContentStreamOperations, IFontTarget
{
    [FromConstructor]private readonly IHasPageAttributes page;
    [FromConstructor]private readonly IRenderTarget target;
    [FromConstructor]private readonly DocumentRenderer renderer;
    [FromConstructor]private readonly IOptionalContentCounter optionalContent;
    
    #region Graphics State
    [DelegateTo] private IGraphicsState StateOps => target.GraphicsState;
    
    
    public void ModifyTransformMatrix(in Matrix3x2 newTransform)
    {
        if (newTransform.IsIdentity) return;
        StateOps.ModifyTransformMatrix(in newTransform);
    }


    public async ValueTask LoadGraphicStateDictionary(PdfName dictionaryName) =>
         await StateOps.LoadGraphicStateDictionary(
            await page.GetResourceAsync(ResourceTypeName.ExtGState, dictionaryName).CA() as 
                PdfDictionary ?? throw new PdfParseException($"Cannot find GraphicsState {dictionaryName}")).CA();
    #endregion
    
    #region Drawing Operations

    private IDrawTarget? currentShape = null;
    
    private IDrawTarget CurrentShape() => currentShape ??= CreateDrawTarget();
    public void EndPathWithNoOp()
    {
        (currentShape as IDisposable)?.Dispose();
        currentShape = null;
    }

    
    private double firstX, firstY;
    private double lastX, lasty;

    private void SetLast(double x, double y) => (lastX, lasty) = (x, y);
    private void SetFirst(double x, double y) => (firstX, firstY) = (x, y);

    public void MoveTo(double x, double y)
    {
        CurrentShape().MoveTo(x, y);
        SetLast(x,y);
        SetFirst(x,y);
    }

    public void LineTo(double x, double y)
    {
        CurrentShape().LineTo(x, y);
        SetLast(x,y);
    }

    public void CurveTo(double control1X, double control1Y, double control2X, double control2Y, double finalX, double finalY)
    {
        CurrentShape().CurveTo(control1X, control1Y, control2X, control2Y, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void CurveToWithoutInitialControl(double control2X, double control2Y, double finalX, double finalY)
    {
        CurrentShape().CurveTo(lastX, lasty, control2X, control2Y, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void CurveToWithoutFinalControl(double control1X, double control1Y, double finalX, double finalY)
    {
        CurrentShape().CurveTo(control1X, control1Y, finalX, finalY, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void ClosePath()
    {
        CurrentShape().ClosePath();
        SetLast(firstX, firstY);
    }

    public void Rectangle(double x, double y, double width, double height)
    {
        var target = CurrentShape();
        target.MoveTo(x,y);
        target.LineTo(x+width,y);
        target.LineTo(x+width,y+height);
        target.LineTo(x,y+height);
        target.ClosePath();
    }
    
    public void StrokePath() => PaintPath(true, false, false);
    public void CloseAndStrokePath() => CloseAndPaintPath(true, false, false);
    public void FillPath() => PaintPath(false, true, false);
    public void FillPathEvenOdd() => PaintPath(false, true, true);
    public void FillAndStrokePath() => PaintPath(true, true, false);
    public void FillAndStrokePathEvenOdd() => PaintPath(true, true, true);
    public void CloseFillAndStrokePath() => CloseAndPaintPath(true, true, false);
    public void CloseFillAndStrokePathEvenOdd() => CloseAndPaintPath(true, true, true);

    
    private void CloseAndPaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        ClosePath();
        PaintPath(stroke, fill, evenOddFillRule);
    }
    private void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        CurrentShape().PaintPath(stroke, fill, evenOddFillRule);
        EndPathWithNoOp();
    }

    public void ClipToPath() => CurrentShape().ClipToPath(false);

    public void ClipToPathEvenOdd() => CurrentShape().ClipToPath(true);

    public async ValueTask DoAsync(PdfName name) =>
        await DoAsync((await page.GetResourceAsync(ResourceTypeName.XObject, name).CA()) as PdfStream ??
                      throw new PdfParseException("Co command can only be called on Streams")).CA();

    public async ValueTask DoAsync(PdfStream inlineImage)
    {
        if (await optionalContent.CanSkipXObjectDoOperation(
                await inlineImage.GetOrNullAsync<PdfDictionary>(KnownNames.OC).CA()).CA())
            return;
        switch ((inlineImage.SubTypeOrNull()??KnownNames.Image).GetHashCode())
        {
            case KnownNameKeys.Image:
                await TryRenderBitmap(inlineImage);
                break;
            case KnownNameKeys.Form:
                await RunTargetGroup(inlineImage).CA();
                break;
            default: throw new PdfParseException("Cannot do the provided object");
        }
    }

    private async Task TryRenderBitmap(PdfStream inlineImage)
    {
        try
        {
            await target.RenderBitmap(
                await inlineImage.WrapForRenderingAsync(page, StateOps.CurrentState().NonstrokeColor).CA()).CA();
        }
        catch (Exception)
        {
            // any error in loading the bitmap causes the bitmap to be ignored.
        }
    }

    public async ValueTask PaintShader(PdfName name)
    {
        var shader = await page.GetResourceAsync(ResourceTypeName.Shading, name).CA();
        if (shader is not PdfDictionary shaderDict) return;
        var factory = await ShaderParser.ParseShader(
            StateOps.CurrentState().TransformMatrix, shaderDict, true).CA();
        StateOps.SaveGraphicsState();
        MapBitmapToViewport();
        await target.RenderBitmap(new ShaderBitmap(factory,
            (int)StateOps.CurrentState().PageWidth, (int)StateOps.CurrentState().PageHeight)).CA();
        StateOps.RestoreGraphicsState();
    }

    private void MapBitmapToViewport()
    {
        var state = StateOps.CurrentState();
        var source = state.TransformMatrix;
        if (!Matrix3x2.Invert(source, out var inv)) return;
        ModifyTransformMatrix(Matrix3x2.CreateScale(
            (float)state.PageWidth, (float)state.PageHeight)*inv);
    }
    
    private async ValueTask RunTargetGroup(PdfStream formXObject)
    {
        SaveGraphicsState();
        await TryApplyFormXObjectMatrix(formXObject).CA();
        
        await TryClipToFormXObjectBoundingBox(formXObject).CA();

        await Render(new PdfFormXObject(formXObject, page)).CA();
        RestoreGraphicsState();
    }

    private async Task TryApplyFormXObjectMatrix(PdfStream formXObject)
    {
        if (await formXObject.GetOrDefaultAsync<PdfObject>(KnownNames.Matrix, PdfTokenValues.Null).CA() 
                is PdfArray arr &&
            await arr.AsDoublesAsync().CA() is { } matrix)
            ModifyTransformMatrix(CreateMatrix(matrix));
    }

    private async Task TryClipToFormXObjectBoundingBox(PdfStream formXObject)
    {
        if (await formXObject.GetOrDefaultAsync<PdfObject>(KnownNames.BBox, PdfTokenValues.Null).CA() 
            is PdfArray{Count:4} arr2)
        {
            var clipRect = await PdfRect.CreateAsync(arr2).CA();
            Rectangle(clipRect.Left, clipRect.Bottom, clipRect.Width, clipRect.Height);
            ClipToPath();
            EndPathWithNoOp();
        }
    }

    private ValueTask Render(IHasPageAttributes xObject) =>
        new RenderEngine(xObject, target, renderer, optionalContent).RunContentStream();

    public async ValueTask RunContentStream() =>
        await new ContentStreamParser(this).Parse(
            PipeReader.Create(await page.GetContentBytes().CA())).CA();


    private static Matrix3x2 CreateMatrix(double[] matrix) =>
        new(
            (float)matrix[0],
            (float)matrix[1],
            (float)matrix[2],
            (float)matrix[3],
            (float)matrix[4],
            (float)matrix[5]
        );

    #endregion

    #region Color Implementation
    
    public async ValueTask SetStrokingColorSpace(PdfName colorSpace)
    {
        target.GraphicsState.SetStrokeColorSpace(
            await new ColorSpaceFactory(page).ParseColorSpace(colorSpace).CA());
    }

    public async ValueTask SetNonstrokingColorSpace(PdfName colorSpace) =>
        target.GraphicsState.SetNonstrokeColorSpace(
            await new ColorSpaceFactory(page).ParseColorSpace(colorSpace).CA());
    
    public ValueTask SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        if (colors.Length > 0) SetStrokeColor(colors);
        return SetStrokingPattern(patternName);
    }

    private async ValueTask SetStrokingPattern(PdfName? patternName)
    {
        if ((await GetPatternDict(patternName).CA()) is { } patternDict)
            await target.GraphicsState.CurrentState()
                .SetStrokePattern(patternDict, renderer).CA();
    }

    public ValueTask SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        if (colors.Length > 0) SetNonstrokingColor(colors);
        return SetNonstrokingPattern(patternName);
    }

    private async ValueTask SetNonstrokingPattern(PdfName? patternName)
    {
        if ((await GetPatternDict(patternName).CA()) is { } patternDict)
            await target.GraphicsState.CurrentState()
                .SetNonstrokePattern(patternDict, renderer).CA();
    }

    private async ValueTask<PdfDictionary?> GetPatternDict(PdfName? patternName) =>
        patternName != null && (await page.GetResourceAsync(ResourceTypeName.Pattern, patternName).CA()) is
        PdfDictionary patternDict
            ? patternDict
            : null;

        public async ValueTask SetStrokeGray(double grayLevel)
    {
        await SetStrokingColorSpace(KnownNames.DeviceGray).CA();
        SetStrokeColor(stackalloc double[] { grayLevel });
    }

    public async ValueTask SetStrokeRGB(double red, double green, double blue)
    {
        await SetStrokingColorSpace(KnownNames.DeviceRGB).CA();
        SetStrokeColor(stackalloc double[] { red, green, blue });
    }

    public async ValueTask SetStrokeCMYK(double cyan, double magenta, double yellow, double black)
    {
        await SetStrokingColorSpace(KnownNames.DeviceCMYK).CA();
        SetStrokeColor(stackalloc double[] { cyan, magenta, yellow, black });
    }

    public async ValueTask SetNonstrokingGray(double grayLevel)
    {
        await SetNonstrokingColorSpace(KnownNames.DeviceGray).CA();
        SetNonstrokingColor(stackalloc double[] { grayLevel });
    }

    public async ValueTask SetNonstrokingRGB(double red, double green, double blue)
    {
        await SetNonstrokingColorSpace(KnownNames.DeviceRGB).CA();
        SetNonstrokingColor(stackalloc double[] { red, green, blue });
    }

    public async ValueTask SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black)
    {
        await SetNonstrokingColorSpace(KnownNames.DeviceCMYK).CA();
        SetNonstrokingColor(stackalloc double[] { cyan, magenta, yellow, black });
    }
    #endregion

    #region Text Operations

    public void BeginTextObject()
    {
        StateOps.SetBothTextMatrices(Matrix3x2.Identity);
    }

    public void EndTextObject()
    {
    }

    public void MovePositionBy(double x, double y) =>
        StateOps.SetBothTextMatrices(
            Matrix3x2.CreateTranslation((float)x,(float)y) 
            * StateOps.CurrentState().TextLineMatrix);

    public void MovePositionByWithLeading(double x, double y)
    {
        StateOps.SetTextLeading(-y);
        MovePositionBy(x,y);
    }
 
    public void SetTextMatrix(double a, double b, double c, double d, double e, double f) =>
        StateOps.SetBothTextMatrices(new Matrix3x2(
            (float)a,(float)b,(float)c,(float)d,(float)e,(float)f));

    public void MoveToNextTextLine() => 
        MovePositionBy(0, - StateOps.CurrentState().TextLeading);

    public async ValueTask SetFont(PdfName font, double size)
    {
        var fontResource = await page.GetResourceAsync(ResourceTypeName.Font, font).CA();
        var typeFace = fontResource is PdfDictionary fontDic ?
            await FontFromDictionary(size, fontDic).CA():
            await SystemFontFromName(font, size).CA();
        
        StateOps.CurrentState().SetTypeface(await RendererSpecificFont(typeFace).CA());
        await StateOps.CurrentState().SetFont(font,size).CA();
    }

    private ValueTask<IRealizedFont> SystemFontFromName(PdfName font, double size) =>
        FontFromDictionary(size, new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.BaseFont, font)
            .AsDictionary()
        );

    private async ValueTask<IRealizedFont> FontFromDictionary(double size, PdfDictionary fontDic) => 
        BlockFontDispose.AsNonDisposableTypeface(await CheckCacheForFont(size, fontDic).CA());

    //The renderer is allowed to optimize the font rendering using render specific structures, so we include the
    // type of the renderer just so that we do not get unexpected structures if a document is rendered
    // in more than one renderer.
    private ValueTask<IRealizedFont> RendererSpecificFont(IRealizedFont typeFace) =>
        renderer.Cache.Get(typeFace, r=> new ValueTask<IRealizedFont>(
            ((IRenderTarget)target).WrapRealizedFont(r)));

    private ValueTask<IRealizedFont> CheckCacheForFont(double size, PdfDictionary fontDic) =>
        renderer.Cache.Get(new FontRecord(fontDic, size), 
            r=> FontReader().DictionaryToRealizedFont(r.Dictionary,r.Size));
     
    private FontReader FontReader() => new(renderer.FontMapper);

    private record struct FontRecord(PdfDictionary Dictionary, double Size);

    public async ValueTask ShowString(ReadOnlyMemory<byte> decodedString)
    {
        var font = StateOps.CurrentState().Typeface;
        using var writer = font.BeginFontWrite(this);
        var remainingI = decodedString;
        while (remainingI.Length > 0)
        {
            var (character, glyph) = GetNextCharacterAndGlyph(font, ref remainingI);
            var measuredGlyphWidth = await writer.AddGlyphToCurrentString(glyph, CharacterPositionMatrix()).CA();
            AdjustTextPositionForCharacter(font.CharacterWidth(character, measuredGlyphWidth), character);
        }
        writer.RenderCurrentString(StateOps.CurrentState().TextRender);
    }

    private static (uint character, uint glyph) GetNextCharacterAndGlyph(
        IRealizedFont font, ref ReadOnlyMemory<byte> remainingI)
    {
        var (character, glyph, bytesUsed) = font.GetNextGlyph(remainingI.Span);
        remainingI = remainingI[bytesUsed..];
        return (character, glyph);
    }

    private Matrix3x2 CharacterPositionMatrix() =>
        (GlyphAdjustmentMatrix() * StateOps.CurrentState().TextMatrix);

    private Matrix3x2 GlyphAdjustmentMatrix() => new(
        (float)StateOps.CurrentState().HorizontalTextScale / 100, 0,
        0, 1,
        0, (float)StateOps.CurrentState().TextRise);

    private void AdjustTextPositionForCharacter(double width, uint character)
    {
        var delta = CharacterSpacingAdjustment(character);
        UpdateTextPosition(width+delta);
    }

    private double CharacterSpacingAdjustment(uint character) =>
        StateOps.CurrentState().CharacterSpacing + ApplicableWordSpacing(character);

    private double ApplicableWordSpacing(uint character) => 
        IsSpaceCharacter(character)? StateOps.CurrentState().WordSpacing:0;

    private bool IsSpaceCharacter(uint character) => character == 0x20;

    private void UpdateTextPosition(double width)
    { 
        StateOps.CurrentState().SetTextMatrix(
            IncrementAlongActiveVector(ScaleHorizontalOffset(width))*
            StateOps.CurrentState().TextMatrix
        );
    }

    private double ScaleHorizontalOffset(double width) => 
        width * StateOps.CurrentState().HorizontalTextScale/100.0;

    private Matrix3x2 IncrementAlongActiveVector(double width) =>
            Matrix3x2.CreateTranslation((float)width, 0.0f);

    public ValueTask MoveToNextLineAndShowString(ReadOnlyMemory<byte> decodedString)
    {
        MoveToNextTextLine();
        return ShowString(decodedString);
    }

    public ValueTask MoveToNextLineAndShowString(double wordSpace, double charSpace, ReadOnlyMemory<byte> decodedString)
    {
        SetWordSpace(wordSpace);
        SetCharSpace(charSpace);
        return MoveToNextLineAndShowString(decodedString);
    }

    public ValueTask ShowSpacedString(in Span<ContentStreamValueUnion> values)
    {
        var ary = ArrayPool<ContentStreamValueUnion>.Shared.Rent(values.Length);
        values.CopyTo(ary);
        return new ValueTask(ShowSpacedString(ary, values.Length).ContinueWith(_ =>
            ArrayPool<ContentStreamValueUnion>.Shared.Return(ary)));
    }

    private async Task ShowSpacedString(ContentStreamValueUnion[] values, int length)
    {
        foreach (var value in values.Take(length))
        {
            switch (value.Type)
            {
                case ContentStreamValueType.Number:
                    var delta = StateOps.CurrentState().FontSize * value.Floating / 1000.0;
                    UpdateTextPosition(-delta);
                    break;
                case ContentStreamValueType.Memory:
                    await ShowString(value.Bytes).CA();
                    break;
                default:
                    throw new PdfParseException("Invalid ShowSpacedString argument");
            }
        }
    }


    #endregion

    #region Type 3 font rendering

    public IDrawTarget CreateDrawTarget() => optionalContent.WrapDrawTarget(target.CreateDrawTarget());

    public async ValueTask<double> RenderType3Character(
        Stream s, Matrix3x2 fontMatrix)
    {
        if (StateOps.CurrentState().TextRender != TextRendering.Invisible)
        {
            await DrawType3Character(s, fontMatrix).CA();
        }
        var ret = CharacterSizeInTextSpace(fontMatrix);
        return ret.X;
    }

    private async Task DrawType3Character(Stream s, Matrix3x2 fontMatrix)
    {
        SaveGraphicsState();
        var textMatrix = StateOps.CurrentState().TextMatrix;
        ModifyTransformMatrix(fontMatrix* textMatrix);
        await new ContentStreamParser(this).Parse(PipeReader.Create(s)).CA();
        RestoreGraphicsState();
    }

    private Vector2 CharacterSizeInTextSpace(Matrix3x2 fontMatrix) =>
        Vector2.Transform(new Vector2((float)lastWx, (float)(lastUry - lastLly)),
            fontMatrix);

    private double lastWx, lastWy, lastLlx, lastLly, lastUrx, lastUry;

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
    }

    #endregion

    #region Marked Operations

    public void MarkedContentPoint(PdfName tag) { }

    public ValueTask MarkedContentPointAsync(PdfName tag, PdfName properties) => ValueTask.CompletedTask;

    public ValueTask MarkedContentPointAsync(PdfName tag, PdfDictionary dictionary) => ValueTask.CompletedTask;

    public void BeginMarkedRange(PdfName tag) {}

    public ValueTask BeginMarkedRangeAsync(PdfName tag, PdfName dictName) => 
        optionalContent.EnterGroup(tag, dictName, page);

    public ValueTask BeginMarkedRangeAsync(PdfName tag, PdfDictionary dictionary) =>
        optionalContent.EnterGroup(tag, dictionary);

    public void EndMarkedRange() => optionalContent.PopContentGroup();
    #endregion

    #region Compatability Operators
    public void BeginCompatibilitySection() { }
    public void EndCompatibilitySection() { }
    #endregion
}