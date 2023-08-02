using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

/// <summary>
/// Parses a PdfDictionary representing a shader into an IShaderWriter
/// </summary>
public static class ShaderParser
{
    /// <summary>
    /// Parse a shader dictionary into an IShaderWriter
    /// </summary>
    /// <param name="patternDictionary">The PdfDictioanry representing the pattern.</param>
    /// <returns>An IShaderWriter to write the shader data</returns>
    public static async ValueTask<IShaderWriter> ParseShaderAsync(PdfDictionary patternDictionary)
    {
        var shadingDictionary = await patternDictionary.GetAsync<PdfDictionary>(KnownNames.Shading).CA();
        var patternToPixels = await (await patternDictionary.GetOrNullAsync<PdfArray>(KnownNames.Matrix).CA())
            .AsMatrix3x2OrIdentityAsync().CA();
        
        return await ParseShaderAsync(patternToPixels, shadingDictionary, false).CA();
    }

    internal static async Task<IShaderWriter> ParseShaderAsync(
        Matrix3x2 patternToPixels, PdfDictionary shadingDictionary, bool supressBackground)
    {
        var common = await CommonShaderValues.ParseAsync(
            patternToPixels, shadingDictionary, supressBackground).CA();
        return await shadingDictionary.GetOrDefaultAsync(KnownNames.ShadingType, 0).CA() switch
        {
            1 => await new Type1PdfFunctionShaderFactory(shadingDictionary).ParseAsync(common).CA(),
            2 => await new Type2Or3ShaderFactory(shadingDictionary).ParseAsync(common, 4).CA(),
            3 => await new Type2Or3ShaderFactory(shadingDictionary).ParseAsync(common, 6).CA(),
            _ => throw new PdfParseException("Invalid Shader type")
        };
    }
}