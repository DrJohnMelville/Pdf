using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Renderers.Bitmaps;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

public static class ShaderParser
{
    public static async ValueTask<IShaderWriter> ParseShader(PdfDictionary patternDictionary)
    {
        var shadingDictionary = await patternDictionary.GetAsync<PdfDictionary>(KnownNames.Shading).CA();
        var patternToPixels = await (await patternDictionary.GetOrNullAsync<PdfArray>(KnownNames.Matrix).CA())
            .AsMatrix3x2OrIdentityAsync().CA();
        
        return await ParseShader(patternToPixels, shadingDictionary, false).CA();
    }

    public static async Task<IShaderWriter> ParseShader(
        Matrix3x2 patternToPixels, PdfDictionary shadingDictionary, bool supressBackground)
    {
        var common = await CommonShaderValues.Parse(
            patternToPixels, shadingDictionary, supressBackground).CA();
        return await shadingDictionary.GetOrDefaultAsync(KnownNames.ShadingType, 0).CA() switch
        {
            1 => await new Type1PdfFunctionShaderFactory(shadingDictionary).Parse(common).CA(),
            2 => await new Type2Or3ShaderFactory(shadingDictionary).Parse(common, 4).CA(),
            3 => await new Type2Or3ShaderFactory(shadingDictionary).Parse(common, 6).CA(),
            _ => throw new PdfParseException("Invalid Shader type")
        };
    }
}

//Unsafe and async do not play well together.  The idea is to have an async factory that
// discovers all the data needed to write the shader.  This creates an IShaderWriter which can
//synchronously to write the bitmap using a pointer.  We use pointers to accomodate the Writeablebitmap
// in WPF.
public interface IShaderWriter
{
    unsafe void RenderBits(uint* bits, int width, int height);
}

public class ShaderBitmap : IPdfBitmap
{
    private readonly IShaderWriter source;
    public int Width { get; }
    public int Height { get; }

    public ShaderBitmap(IShaderWriter source, int width, int height)
    {
        this.source = source;
        Width = width;
        Height = height;
    }

    public bool DeclaredWithInterpolation => true;
    public unsafe ValueTask RenderPbgra(byte* buffer)
    {
        source.RenderBits((uint*)buffer, Width, Height);
        return ValueTask.CompletedTask;
    }
}