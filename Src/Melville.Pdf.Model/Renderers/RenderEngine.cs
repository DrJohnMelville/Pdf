﻿using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Annotations;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.ColorOperations;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

namespace Melville.Pdf.Model.Renderers;

internal partial class RenderEngine: IContentStreamOperations, IFontTarget
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

    IRenderTarget IFontTarget.RenderTarget => pageRenderContext.Target;

    [DelegateTo]
    private IPathDrawingOperations PathDrawingOperations() =>
        pathDrawing.IsInvalid? pathDrawing.WithNewTarget(CreateDrawTarget()): pathDrawing;

    public async ValueTask DoAsync(PdfDirectObject name) =>
        await DoAsync((await page.GetResourceAsync(ResourceTypeName.XObject, name).CA())
            .TryGet(out PdfStream? stream)? 
            stream: throw new PdfParseException("Do command can only be called on Streams")).CA();

    public async ValueTask DoAsync(PdfStream inlineImage)
    {
        if (await pageRenderContext.OptionalContent.CanSkipXObjectDoOperationAsync(
                await inlineImage.GetOrNullAsync<PdfDictionary>(KnownNames.OC).CA()).CA())
            return;
        switch (inlineImage.SubTypeOrNull())
        {
            case {IsNull:true}:
            case var x when x.Equals(KnownNames.Image):
                await TryRenderBitmapAsync(inlineImage).CA();
                break;
            case var x when x.Equals(KnownNames.Form):
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
        ModifyTransformMatrix(await formXObject.MatrixAsync().CA());
        
        await TryClipToFormXObjectBoundingBoxAsync(formXObject).CA();

        await RenderAsync(formXObject).CA();
        RestoreGraphicsState();
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
        await otherEngine.RunContentStreamAsync(i=>i).CA();
        CopyLastGlyphMetrics(otherEngine);
        pageRenderContext.ItemsBeingRendered.PopItem();
    }

    public async ValueTask RunContentStreamAsync(Func<IContentStreamOperations, IContentStreamOperations> wrapOutput)
    {
        using var reader = MultiplexSourceFactory.SingleReaderForStream(
            await page.GetContentBytesAsync().CA());
        await new ContentStreamParser(wrapOutput(this)).ParseAsync(
            reader).CA();
        await TryRenderAnnotationsAsync().CA();
    }

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
        var genericRealizedFont = fontResource.TryGet(out PdfDictionary? fontDic) ?
            await FontFromDictionaryAsync(fontDic).CA():
            await SystemFontFromNameAsync(font).CA();
        
        GraphicsState.SetTypeface(await GetRenderWrappedFontAsync(genericRealizedFont, fontDic).CA());
        await GraphicsState.SetFontAsync(font,size).CA();
    }

    private ValueTask<IRealizedFont> SystemFontFromNameAsync(PdfDirectObject font) =>
        FontFromDictionaryAsync(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.BaseFont, font)
            .AsDictionary()
        );

    private async ValueTask<IRealizedFont> FontFromDictionaryAsync(PdfDictionary fontDic) => 
        await CheckCacheForFontAsync(fontDic).CA();

    private ValueTask<IRealizedFont> GetRenderWrappedFontAsync(
        IRealizedFont typeFace, PdfDictionary? fontDeclaration) =>
        pageRenderContext.Renderer.Cache.GetAsync(CreateWrappedFontKey(typeFace), 
            r=> pageRenderContext.Target.WrapRealizedFontAsync(r.TypeFace, fontDeclaration));

    private record WrappedFontKey(Type RenderTargetType, IRealizedFont TypeFace);

    private WrappedFontKey CreateWrappedFontKey(IRealizedFont typeFace) =>
        new(pageRenderContext.Target.GetType(), typeFace);

    //Notice that IRealizedFont does not implement IDisposable, but most of the real implementations do.  This is intentional.
    //Most users of fonts should not dispose of them, so IRealizedFont is not disposable.  When the cache gets disposed, it
    // will try to cast each item to IDisposable and dispose of the ones that implement that interface.
    private ValueTask<IRealizedFont> CheckCacheForFontAsync(PdfDictionary fontDic) =>
        pageRenderContext.Renderer.Cache.GetAsync(fontDic, async r=> 
            (await FontReader().DictionaryToRealizedFontAsync(r).CA()));
     
    private FontReader FontReader() => new(pageRenderContext.Renderer.FontMapper);

    public ISpacedStringBuilder GetSpacedStringBuilder() => new PdfStringWriter(this);

    public async ValueTask ShowStringAsync(ReadOnlyMemory<byte> decodedString)
    {
        await using var writer = GetSpacedStringBuilder();
        await writer.SpacedStringComponentAsync(decodedString).CA();
    }

    private Matrix3x2 CharacterPositionMatrix() => GraphicsState.GlyphTransformMatrix();

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

    #region Annotations

    public async ValueTask TryRenderAnnotationsAsync()
    {
        if (await page.LowLevel.GetOrNullAsync<PdfArray>(KnownNames.Annots).CA() is { } annots)
        {
            await foreach (var annot in annots.CA())
            {
                if (annot.TryGet(out PdfDictionary? annotDict)) 
                  await RenderSingleAnnotationAsync(new Annotation(annotDict)).CA();
            }
        }
    }

    private async ValueTask RenderSingleAnnotationAsync(Annotation annotation)
    {
        if (await annotation.GetVisibleFormAsync().CA() is { } form)
        {
            SaveGraphicsState();
            var formXObject = new PdfFormXObject(form, page);

            var matrix = await formXObject.MatrixAsync().CA();
            var bBox = await formXObject.BboxAsync().CA();
            var rect = await annotation.RectAsync().CA();

            var xform = CreateAnnotationTransform(matrix, bBox, rect);

            ModifyTransformMatrix(xform);


            await TryClipToFormXObjectBoundingBoxAsync(formXObject).CA();

            await RenderAsync(formXObject).CA();
            RestoreGraphicsState();
        }
    }

    private Matrix3x2 CreateAnnotationTransform(Matrix3x2 matrix, PdfRect? bBox, PdfRect? rect)
    {
        if (!(bBox.HasValue && rect.HasValue)) return matrix;
        var targetBox = bBox.Value.BoundTransformedRect(matrix);
        return targetBox.TransformTo(rect.Value); }

    #endregion
}