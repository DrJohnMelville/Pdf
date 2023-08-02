using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public abstract class Type1FunctionalShaderBase: PatternDisplayClass
{
    protected Type1FunctionalShaderBase(string helpText) : base(helpText)
    {
    }

    private PdfStream[] function;
    protected override async ValueTask SetPagePropertiesAsync(PageCreator page)
    {
        function = await BuildFunctionAsync();
        await base.SetPagePropertiesAsync(page);
    }

    protected virtual async Task<PdfStream[]> BuildFunctionAsync()
    {
        var fbuilder = new SampledFunctionBuilder(4, SampledFunctionOrder.Linear);
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddOutput((x, y) => x, new ClosedInterval(0, 1));
        fbuilder.AddOutput((x, y) => y, new ClosedInterval(0, 1));
        fbuilder.AddOutput((x, y) => 1 - x, new ClosedInterval(0, 1));
        var ret = await fbuilder.CreateSampledFunctionAsync();
        return new PdfStream[]{ret};
    }

    protected override PdfIndirectObject CreatePattern(IPdfObjectCreatorRegistry arg) =>
        BuildPattern(arg, 
            BuildShader(arg, function , 
                new DictionaryBuilder()).AsDictionary(),
            new DictionaryBuilder()).AsDictionary();

    protected virtual DictionaryBuilder BuildPattern(
        IPdfObjectCreatorRegistry arg, PdfDictionary shading, DictionaryBuilder builder) => builder
            .WithItem(KnownNames.ShadingTName, arg.Add(shading))
            .WithItem(KnownNames.MatrixTName, Matrix3x2.CreateScale(5 * 72, 3 * 72).AsPdfArray())
            .WithItem(KnownNames.PatternTypeTName, 2);

    protected virtual DictionaryBuilder BuildShader(
        IPdfObjectCreatorRegistry arg, PdfStream[] localFunc, DictionaryBuilder builder) => builder
            .WithItem(KnownNames.FunctionTName, arg.Add(ComputeLocalFunc(localFunc, arg)))
            .WithItem(KnownNames.ShadingTypeTName, 1)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceRGBTName);

    private static PdfDirectObject ComputeLocalFunc(PdfStream[] localFunc, IPdfObjectCreatorRegistry ldc)
    {
        if (localFunc.Length == 1) return localFunc[0];
        return new PdfArray(localFunc.Select(i => ldc.Add(i)).ToArray());
    }
}