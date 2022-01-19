using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Model.Renderers.GraphicsStates;
namespace Melville.Pdf.Model.Renderers;

public partial class RenderEngine: IContentStreamOperations, IFontTarget
{
    private readonly IHasPageAttributes page;
    private readonly IRenderTarget target;
    private readonly FontReader fontReader;
    public RenderEngine(IHasPageAttributes page, IRenderTarget target, FontReader fontReader)
    {
        this.page = page;
        this.target = target;
        this.fontReader = fontReader;
    }
    
    [DelegateTo]
    private IMarkedContentCSOperations Marked => throw new NotImplementedException("Marked Operations not implemented");
    [DelegateTo]
    private ICompatibilityOperations Compat => 
        throw new NotImplementedException("Compatibility Operations not implemented");

    #region Graphics State
    [DelegateTo] private IGraphiscState StateOps => target.GrapicsStateChange;
    
    public void SaveGraphicsState()
    {
        StateOps.SaveGraphicsState();
        target.SaveTransformAndClip();
    }

    public void RestoreGraphicsState()
    {
        StateOps.RestoreGraphicsState();
        target.RestoreTransformAndClip();
    }

    public void ModifyTransformMatrix(in System.Numerics.Matrix3x2 newTransform)
    {
        StateOps.ModifyTransformMatrix(in newTransform);
        target.Transform(newTransform);
    }


    public async ValueTask LoadGraphicStateDictionary(PdfName dictionaryName) =>
        await StateOps.LoadGraphicStateDictionary(
            await page.GetResourceAsync(ResourceTypeName.ExtGState, dictionaryName) as 
                PdfDictionary ?? throw new PdfParseException($"Cannot find GraphicsState {dictionaryName}"));
    #endregion
    
    #region Drawing Operations

    private double firstX, firstY;
    private double lastX, lasty;

    private void SetLast(double x, double y) => (lastX, lasty) = (x, y);
    private void SetFirst(double x, double y) => (firstX, firstY) = (x, y);

    public void MoveTo(double x, double y)
    {
        target.MoveTo(x, y);
        SetLast(x,y);
        SetFirst(x,y);
    }

    public void LineTo(double x, double y)
    {
        target.LineTo(x, y);
        SetLast(x,y);
    }

    public void CurveTo(double control1X, double control1Y, double control2X, double control2Y, double finalX, double finalY)
    {
        target.CurveTo(control1X, control1Y, control2X, control2Y, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void CurveToWithoutInitialControl(double control2X, double control2Y, double finalX, double finalY)
    {
        target.CurveTo(lastX, lasty, control2X, control2Y, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void CurveToWithoutFinalControl(double control1X, double control1Y, double finalX, double finalY)
    {
        target.CurveTo(control1X, control1Y, finalX, finalY, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void ClosePath()
    {
        target.ClosePath();
        SetLast(firstX, firstY);
    }

    public void Rectangle(double x, double y, double width, double height)
    {
        target.MoveTo(x,y);
        target.LineTo(x+width,y);
        target.LineTo(x+width,y+height);
        target.LineTo(x,y+height);
        target.ClosePath();
    }

    public void EndPathWithNoOp() => target.EndPath();
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
        target.PaintPath(stroke, fill, evenOddFillRule);
        EndPathWithNoOp();
    }

    public void ClipToPath() => target.ClipToPath(false);

    public void ClipToPathEvenOdd() => target.ClipToPath(true);

    public async ValueTask DoAsync(PdfName name) =>
        await DoAsync((await page.GetResourceAsync(ResourceTypeName.XObject, name)) as PdfStream ??
                      throw new PdfParseException("Co command can only be called on Streams"));

    public async ValueTask DoAsync(PdfStream inlineImage)
    {
        switch ((await inlineImage.GetAsync<PdfName>(KnownNames.Subtype)).GetHashCode())
        {
            case KnownNameKeys.Image:
                await target.RenderBitmap(
                    await inlineImage.WrapForRenderingAsync(page, StateOps.CurrentState().NonstrokeColor));
                break;
            case KnownNameKeys.Form:
                await RunTargetGroup(inlineImage);
                break;
            default: throw new PdfParseException("Cannot do the provided object");
        }
    }

    private async ValueTask RunTargetGroup(PdfStream formXObject)
    {
        SaveGraphicsState();
        if(await formXObject.GetOrDefaultAsync<PdfObject>(KnownNames.Matrix, PdfTokenValues.Null) is PdfArray arr &&
           (await arr.AsDoublesAsync()) is {} matrix)
            ModifyTransformMatrix(CreateMatrix(matrix));
        
        if ((await formXObject.GetOrDefaultAsync<PdfObject>(KnownNames.BBox, PdfTokenValues.Null)) is PdfArray arr2 &&
            (await arr2.AsDoublesAsync()) is { } bbArray)
        {
            Rectangle(bbArray[0], bbArray[1], bbArray[2], bbArray[3]);
            ClipToPath();
            EndPathWithNoOp();
        }
        await new PdfFormXObject(formXObject, page).RenderTo(target, fontReader);
        RestoreGraphicsState();
    }

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
        target.GrapicsStateChange.SetStrokeColorSpace(
            await ColorSpaceFactory.ParseColorSpace(colorSpace, page));
    }

    public async ValueTask SetNonstrokingColorSpace(PdfName colorSpace) =>
        target.GrapicsStateChange.SetNonstrokeColorSpace(
            await ColorSpaceFactory.ParseColorSpace(colorSpace, page));
    
    public ValueTask SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        if (patternName != null) throw new NotImplementedException("Patterns not implemented yet");
        SetStrokeColor(colors);
        return ValueTask.CompletedTask;
    }
    
    public ValueTask SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        if (patternName != null) throw new NotImplementedException("Patterns not implemented yet");
        SetNonstrokingColor(colors);
        return ValueTask.CompletedTask;
    }

    public async ValueTask SetStrokeGray(double grayLevel)
    {
        await SetStrokingColorSpace(KnownNames.DeviceGray);
        SetStrokeColor(stackalloc double[] { grayLevel });
    }

    public async ValueTask SetStrokeRGB(double red, double green, double blue)
    {
        await SetStrokingColorSpace(KnownNames.DeviceRGB);
        SetStrokeColor(stackalloc double[] { red, green, blue });
    }

    public async ValueTask SetStrokeCMYK(double cyan, double magenta, double yellow, double black)
    {
        await SetStrokingColorSpace(KnownNames.DeviceCMYK);
        SetStrokeColor(stackalloc double[] { cyan, magenta, yellow, black });
    }

    public async ValueTask SetNonstrokingGray(double grayLevel)
    {
        await SetNonstrokingColorSpace(KnownNames.DeviceGray);
        SetNonstrokingColor(stackalloc double[] { grayLevel });
    }

    public async ValueTask SetNonstrokingRGB(double red, double green, double blue)
    {
        await SetNonstrokingColorSpace(KnownNames.DeviceRGB);
        SetNonstrokingColor(stackalloc double[] { red, green, blue });
    }

    public async ValueTask SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black)
    {
        await SetNonstrokingColorSpace(KnownNames.DeviceCMYK);
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
        var fontMapping = await page.GetResourceAsync(ResourceTypeName.Font, font) is PdfDictionary fontDic ?
            await fontReader.DictionaryToMappingAsync(fontDic, this, size) :
            fontReader.NameToMapping(font, CharacterEncodings.Standard);
        await SetTypeface(fontMapping, size);
    }

    private async ValueTask SetTypeface(IFontMapping fontMapping, double size)
    {
        if (fontMapping.Font is IRealizedFont rf)
            StateOps.CurrentState().SetTypeface(rf);
        else
            await target.SetFont(fontMapping, size);
    }

    public async ValueTask ShowString(ReadOnlyMemory<byte> decodedString)
    {
        var writer = StateOps.CurrentState().Typeface.BeginFontWrite();
        for (int i = 0; i < decodedString.Length; i++)
        {
            var character = GetAt(decodedString,  i);
            var (w, h) = await writer.AddGlyphToCurrentString(character);
            AdjustTextPositionForCharacter(w, h, character);
        }
        writer.RenderCurrentString(StateOps.CurrentState().TextRender);
    }

    private byte GetAt(ReadOnlyMemory<byte> decodedString, int i) => decodedString.Span[i];
    
    private void AdjustTextPositionForCharacter(double width, double height, byte character)
    {
        var delta = CharacterSpacingAdjustment(character);
        UpdateTextPosition(width+delta, height + delta);
    }

    private double CharacterSpacingAdjustment(byte character) =>
        StateOps.CurrentState().CharacterSpacing + ApplicableWordSpacing(character);

    private double ApplicableWordSpacing(byte character) => 
        IsSpaceCharacter(character)? StateOps.CurrentState().WordSpacing:0;

    private bool IsSpaceCharacter(byte character) => character == 0x20;

    private void UpdateTextPosition(double width, double height)
    { 
        StateOps.CurrentState().SetTextMatrix(
            IncrementAlongActiveVector(ScaleHorizontalOffset(width), height)*
            StateOps.CurrentState().TextMatrix
        );
    }

    private double ScaleHorizontalOffset(double width) => 
        width * StateOps.CurrentState().HorizontalTextScale/100.0;

    private Matrix3x2 IncrementAlongActiveVector(double width, double height) =>
        StateOps.CurrentState().WritingMode == WritingMode.TopToBottom
            ? Matrix3x2.CreateTranslation(0f, (float)-height)
            : Matrix3x2.CreateTranslation((float)width, 0.0f);

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
                    var delta = value.Floating / 1000;
                    UpdateTextPosition(-delta, delta);
                    break;
                case ContentStreamValueType.Memory:
                    await ShowString(value.Bytes);
                    break;
                default:
                    throw new PdfParseException("Invalid ShowSpacedString argument");
            }
        }
    }


    #endregion

    #region Type 3 font rendering

    public IDrawTarget CreateDrawTarget() => target.CreateDrawTarget();

    public async ValueTask<(double width, double height)> RenderType3Character(
        Stream s, Matrix3x2 fontMatrix)
    {
        if (StateOps.CurrentState().TextRender != TextRendering.Invisible)
        {
            await DrawType3Character(s, fontMatrix);
        }
        var ret = CharacterSizeInTextSpace(fontMatrix);
        return (ret.X, ret.Y);
    }

    private async Task DrawType3Character(Stream s, Matrix3x2 fontMatrix)
    {
        SaveGraphicsState();
        var textMatrix = StateOps.CurrentState().TextMatrix;
        ModifyTransformMatrix(fontMatrix * textMatrix);
        await new ContentStreamParser(this).Parse(PipeReader.Create(s));
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
}