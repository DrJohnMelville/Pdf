using System.Numerics;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public abstract class Type1FunctionalShaderBase: PatternDisplayClass
{
    protected Type1FunctionalShaderBase(string helpText) : base(helpText)
    {
    }

    private PdfValueStream[] function;
    protected override async ValueTask SetPagePropertiesAsync(PageCreator page)
    {
        function = await BuildFunctionAsync();
        await base.SetPagePropertiesAsync(page);
    }

    protected virtual async Task<PdfValueStream[]> BuildFunctionAsync()
    {
        var fbuilder = new SampledFunctionBuilder(4, SampledFunctionOrder.Linear);
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddOutput((x, y) => x, new ClosedInterval(0, 1));
        fbuilder.AddOutput((x, y) => y, new ClosedInterval(0, 1));
        fbuilder.AddOutput((x, y) => 1 - x, new ClosedInterval(0, 1));
        var ret = await fbuilder.CreateSampledFunctionAsync();
        return new PdfValueStream[]{ret};
    }

    protected override PdfIndirectValue CreatePattern(IPdfObjectCreatorRegistry arg) =>
        BuildPattern(arg, 
            BuildShader(arg, function , 
                new ValueDictionaryBuilder()).AsDictionary(),
            new ValueDictionaryBuilder()).AsDictionary();

    protected virtual ValueDictionaryBuilder BuildPattern(
        IPdfObjectCreatorRegistry arg, PdfValueDictionary shading, ValueDictionaryBuilder builder) => builder
            .WithItem(KnownNames.ShadingTName, arg.Add(shading))
            .WithItem(KnownNames.MatrixTName, Matrix3x2.CreateScale(5 * 72, 3 * 72).AsPdfArray())
            .WithItem(KnownNames.PatternTypeTName, 2);

    protected virtual ValueDictionaryBuilder BuildShader(
        IPdfObjectCreatorRegistry arg, PdfValueStream[] localFunc, ValueDictionaryBuilder builder) => builder
            .WithItem(KnownNames.FunctionTName, arg.Add(ComputeLocalFunc(localFunc, arg)))
            .WithItem(KnownNames.ShadingTypeTName, 1)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceRGBTName);

    private static PdfDirectValue ComputeLocalFunc(PdfValueStream[] localFunc, IPdfObjectCreatorRegistry ldc)
    {
        if (localFunc.Length == 1) return localFunc[0];
        return new PdfValueArray(localFunc.Select(i => ldc.Add(i)).ToArray());
    }
}