using System.Numerics;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public abstract class Type2LinearShaderBase : PatternDisplayClass
{
    protected Type2LinearShaderBase(string helpText) : base(helpText)
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
        fbuilder.AddOutput(x => x, new ClosedInterval(0, 1));
        fbuilder.AddOutput(x => x*0, new ClosedInterval(0, 1));
        fbuilder.AddOutput(x => 1 - x, new ClosedInterval(0, 1));
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
            .WithItem(KnownNames.CoordsTName, new PdfValueArray(0, .2, 0, .8))
            .WithItem(KnownNames.ShadingTypeTName, 2)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceRGBTName);
}

public class Type2LinearShader : Type2LinearShaderBase
{
    public Type2LinearShader() : base("A simple Linear Shader")
    {
    }
}

public class Type2ExtendLow : Type2LinearShaderBase
{
    public Type2ExtendLow() : base ("Axial shader that extends its low extent"){}

    protected override ValueDictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfDictionary localFunc, ValueDictionaryBuilder builder) =>
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.ExtendTName, new PdfValueArray(true, false));
}

public class Type2ExtendHigh : Type2LinearShaderBase
{
    public Type2ExtendHigh() : base ("Axial shader that extends its high extent"){}

    protected override ValueDictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfDictionary localFunc, ValueDictionaryBuilder builder) =>
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.ExtendTName, new PdfValueArray(false, true));
}

public class Type2ExtendBoth : Type2LinearShaderBase
{
    public Type2ExtendBoth() : base ("Axial shader that extends both ends"){}

    protected override ValueDictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfDictionary localFunc, ValueDictionaryBuilder builder) =>
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.ExtendTName, new PdfValueArray(true, true));
}

public class Type2ExtendBothWithBackground : Type2LinearShaderBase
{
    public Type2ExtendBothWithBackground() : base ("Axial shader that extends both ends with a background"){}

    protected override ValueDictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfDictionary localFunc, ValueDictionaryBuilder builder) =>
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.ExtendTName, new PdfValueArray(true, true))
            .WithItem(KnownNames.BackgroundTName, new PdfValueArray(0, 1, 0));
}

public class Type2ExtendNoneWithBackground : Type2LinearShaderBase
{
    public Type2ExtendNoneWithBackground() : base ("Axial shader that extends both ends with a background"){}

    protected override ValueDictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfDictionary localFunc, ValueDictionaryBuilder builder) =>
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.BackgroundTName, new PdfValueArray(0, 1, 0));
}