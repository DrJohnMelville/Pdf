using System.Numerics;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public abstract class Type3RadialShaderBase : PatternDisplayClass
{
    protected Type3RadialShaderBase(string helpText) : base(helpText)
    {
    }
    
    private PdfDictionary? function = null;
    protected override async ValueTask SetPagePropertiesAsync(PageCreator page)
    {
        function = await BuildFunctionAsync();
        await base.SetPagePropertiesAsync(page);
    }

    protected virtual async Task<PdfDictionary> BuildFunctionAsync()
    {
        var fbuilder = new SampledFunctionBuilder(4, SampledFunctionOrder.Linear);
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddOutput((double _) => 1, new ClosedInterval(0, 1));
        fbuilder.AddOutput(x => x, new ClosedInterval(0, 1));
        fbuilder.AddOutput(x => x, new ClosedInterval(0, 1));
        return await fbuilder.CreateSampledFunctionAsync();
    }

    protected override PdfObject CreatePattern(IPdfObjectRegistry arg) =>
        BuildPattern(arg, 
            BuildShader(arg, function ?? throw new InvalidOperationException("No func defined"), 
                new DictionaryBuilder()).AsDictionary(),
            new DictionaryBuilder()).AsDictionary();

    protected virtual DictionaryBuilder BuildPattern(
        IPdfObjectRegistry arg, PdfDictionary shading, DictionaryBuilder builder) => builder
        .WithItem(KnownNames.Shading, arg.Add(shading))
        .WithItem(KnownNames.Matrix, Matrix3x2.CreateScale(5 * 72, 3 * 72).AsPdfArray())
        .WithItem(KnownNames.PatternType, 2);

    protected virtual DictionaryBuilder BuildShader(
        IPdfObjectRegistry arg, PdfDictionary localFunc, DictionaryBuilder builder) => builder
        .WithItem(KnownNames.Function, arg.Add(localFunc))
        .WithItem(KnownNames.Coords, new PdfArray(0.25, .4, 0.1, .35, .4, .01))
        .WithItem(KnownNames.ShadingType, 3)
        .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB);
}

public class Type3RadialShader: Type3RadialShaderBase
{
    public Type3RadialShader() : base("A simple radial shader")
    {
    }
}

public class Type3RadialShaderWithBackground: Type3RadialShaderBase
{
    public Type3RadialShaderWithBackground() : base("A simple radial shader with a background")
    {
    }

    protected override DictionaryBuilder BuildShader(IPdfObjectRegistry arg, PdfDictionary localFunc, DictionaryBuilder builder) => 
        base.BuildShader(arg, localFunc, builder).WithItem(KnownNames.Background, new PdfArray(0,0, 1));
}