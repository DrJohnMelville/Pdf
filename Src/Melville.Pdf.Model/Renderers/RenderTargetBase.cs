using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

/// <summary>
/// This is a base class to help target implementations implement IRenderTarget.
/// </summary>
/// <typeparam name="T">The object which the renderer will write the page data to.</typeparam>
/// <typeparam name="TState"></typeparam>
public abstract class RenderTargetBase<T, TState>: IRenderTarget
   where TState:GraphicsState, new()
{
    /// <summary>
    /// The object that the renderer will write the PDF data to.
    /// </summary>
    protected T Target { get; }
    /// <summary>
    /// The current graphics state.
    /// </summary>
    protected GraphicsStateStack<TState> State { get; } = new();
    
    /// <summary>
    /// The current graphics state.
    /// </summary>
    public IGraphicsState GraphicsState => State;

    /// <summary>
    /// Create a RenderTargetBase
    /// </summary>
    /// <param name="target">The tar</param>
    protected RenderTargetBase(T target)
    {
        Target = target;
    }


    /// <inheritdoc />
    public void MapUserSpaceToBitmapSpace(in PdfRect rect, double xPixels, double yPixels, in Matrix3x2 adjustOutput)
    {
        var xform = adjustOutput *
                    Matrix3x2.CreateTranslation((float)-rect.Left, (float)-rect.Bottom) *
                    Matrix3x2.CreateScale((float)(xPixels / rect.Width), (float)(-yPixels / rect.Height)) *
                    Matrix3x2.CreateTranslation(0, (float)yPixels);
        GraphicsState.ModifyTransformMatrix(xform);
        GraphicsState.CurrentState().StoreInitialTransform();
    }
    
    #region Draw Shapes

    public abstract IDrawTarget CreateDrawTarget();
    #endregion

    /// <inheritdoc />
    public void CloneStateFrom(GraphicsState priorState)
    {
        if (priorState is TState ts) State.StronglyTypedCurrentState().CopyFrom(ts);
        State.StronglyTypedCurrentState().ResetTransformMatrix();
            
    }

    /// <inheritdoc />
    public virtual void Dispose() => State.Dispose();

    /// <inheritdoc />
    public abstract ValueTask RenderBitmap(IPdfBitmap bitmap);

    /// <inheritdoc />
    public abstract void SetBackgroundRect(
        in PdfRect rect, double width, double height, in Matrix3x2 transform);

    /// <inheritdoc />
    public virtual IRealizedFont WrapRealizedFont(IRealizedFont font) => font;
}