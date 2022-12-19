using System.Numerics;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;

internal interface ISkiaBrushCreator: IDisposable
{
    SKPaint CreateBrush(SkiaGraphicsState topState);
}

internal class SolidColorBrushCreator : ISkiaBrushCreator
{
    private readonly SKPaint value;

    public SolidColorBrushCreator(DeviceColor color)
    {
        value = new SKPaint() { Color = color.AsSkColor() };
    }

    public void Dispose() => value.Dispose();

    public SKPaint CreateBrush(SkiaGraphicsState topState) => value;
}

internal abstract class IntermediateBrushHolder<T>: ISkiaBrushCreator where T : IDisposable
{
    protected readonly T value;
    private readonly List<IDisposable> items = new();
    protected IntermediateBrushHolder(T value)
    {
        this.value = value;
    }

    public void Dispose()
    {
        foreach (var toDispose in items)
        {
            toDispose.Dispose();
        }
    }

    protected TLocal RegisterForDispose<TLocal>(TLocal product) where TLocal : class, IDisposable
    {
        items.Add(product);
        return product;
    }

    public abstract SKPaint CreateBrush(SkiaGraphicsState topState);
}

internal class SurfacePatternHolder : IntermediateBrushHolder<SKSurface>
{
    private readonly Matrix3x2 patternTransform;
    public SurfacePatternHolder(SKSurface value, Matrix3x2 patternTransform) : base(value)
    {
        this.patternTransform = patternTransform;
    }

    public override SKPaint CreateBrush(SkiaGraphicsState topState)
    {
        return RegisterForDispose(new SKPaint()
            {
                Shader = SKShader.CreateImage(value.Snapshot(),
                        SKShaderTileMode.Repeat, SKShaderTileMode.Repeat)
                    .WithLocalMatrix((patternTransform*topState.RevertToPixelsMatrix()).Transform())
            }
            );
    }
}

internal class ImagePatternHolder : IntermediateBrushHolder<SKBitmap>
{
    public ImagePatternHolder(SKBitmap value) : base(value)
    {
    }
    
    public override SKPaint CreateBrush(SkiaGraphicsState topState)
    {
        return RegisterForDispose(new SKPaint()
        {
            Shader = SKShader.CreateBitmap(value, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp)
                .WithLocalMatrix(topState.RevertToPixelsMatrix().Transform())
        });
    }
}
