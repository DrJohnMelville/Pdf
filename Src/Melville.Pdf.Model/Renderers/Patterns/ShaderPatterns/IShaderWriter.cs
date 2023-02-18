namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

/// <summary>
///Unsafe and async do not play well together.  The idea is to have an async factory that
/// discovers all the data needed to write the shader.  This creates an IShaderWriter which can
///synchronously to write the bitmap using a pointer.  We use pointers to accomodate the Writeablebitmap
/// in WPF.
/// </summary>
public interface IShaderWriter
{
    /// <summary>
    /// Write the pattern to a device bitmap
    /// </summary>
    /// <param name="bits">The bits of the bitmap -- must be height * width long.</param>
    /// <param name="width">Width </param>
    /// <param name="height"></param>
    unsafe void RenderBits(uint* bits, int width, int height);
}