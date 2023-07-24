using System.Numerics;
using Melville.Pdf.LowLevel.Model.Objects2;
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

    protected override PdfObject CreatePattern(IPdfObjectCreatorRegistry arg) =>
        BuildPattern(arg, 
            BuildShader(arg, function ?? throw new InvalidOperationException("No func defined"), 
                new ValueDictionaryBuilder()).AsDictionary(),
            new ValueDictionaryBuilder()).AsDictionary();

    protected virtual ValueDictionaryBuilder BuildPattern(
        IPdfObjectCreatorRegistry arg, PdfDictionary shading, ValueDictionaryBuilder builder) => builder
        .WithItem(KnownNames.ShadingTName, arg.Add(shading))
        .WithItem(KnownNames.MatrixTName, Matrix3x2.CreateScale(5 * 72, 3 * 72).AsPdfValueArray())
        .WithItem(KnownNames.PatternTypeTName, 2);

    protected virtual ValueDictionaryBuilder BuildShader(
        IPdfObjectCreatorRegistry arg, PdfDictionary localFunc, ValueDictionaryBuilder builder) => builder
        .WithItem(KnownNames.FunctionTName, arg.Add(localFunc))
        .WithItem(KnownNames.CoordsTName, new PdfValueArray(0.25, .4, 0.1, .35, .4, .01))
        .WithItem(KnownNames.ShadingTypeTName, 3)
        .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceRGBTName);
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

    protected override ValueDictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfDictionary localFunc, ValueDictionaryBuilder builder) => 
        base.BuildShader(arg, localFunc, builder).WithItem(KnownNames.BackgroundTName, new PdfValueArray(0,0, 1));
}