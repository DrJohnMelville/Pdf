using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

public readonly struct ShaderParser
{
    private readonly PdfDictionary patternDictionary;
    private readonly PdfDictionary shadingDictionary;

    public ShaderParser(PdfDictionary patternDictionary, PdfDictionary shadingDictionary)
    {
        this.patternDictionary = patternDictionary;
        this.shadingDictionary = shadingDictionary;
    }

    public async ValueTask<IShaderWriter> ParseShader()
    {
        var common = await CommonShaderValues.Parse(patternDictionary, shadingDictionary).CA();
        return await shadingDictionary.GetOrDefaultAsync(KnownNames.ShadingType, 0).CA() switch
        {
            1 => await new Type1PdfFunctionShaderFactory(shadingDictionary).Parse(common).CA(),
            2=> await new Type2Or3ShaderFactory(shadingDictionary).Parse(common, 4).CA(),
            3=> await new Type2Or3ShaderFactory(shadingDictionary).Parse(common, 6).CA(),
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