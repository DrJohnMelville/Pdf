using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public abstract class Type1FunctionalShaderBase: PatternDisplayClass
{
    protected Type1FunctionalShaderBase(string helpText) : base(helpText)
    {
    }

    private PdfObject[]? function = null;
    protected override async ValueTask SetPagePropertiesAsync(PageCreator page)
    {
        function = await BuildFunction();
        await base.SetPagePropertiesAsync(page);
    }

    protected virtual async Task<PdfStream[]> BuildFunction()
    {
        var fbuilder = new SampledFunctionBuilder(4, SampledFunctionOrder.Linear);
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddOutput((x, y) => x, new ClosedInterval(0, 1));
        fbuilder.AddOutput((x, y) => y, new ClosedInterval(0, 1));
        fbuilder.AddOutput((x, y) => 1 - x, new ClosedInterval(0, 1));
        var ret = await fbuilder.CreateSampledFunction();
        return new PdfStream[]{ret};
    }

    protected override PdfObject CreatePattern(ILowLevelDocumentCreator arg) =>
        BuildPattern(arg, 
            BuildShader(arg, function ?? throw new InvalidOperationException("No func defined"), 
                new DictionaryBuilder()).AsDictionary(),
            new DictionaryBuilder()).AsDictionary();

    protected virtual DictionaryBuilder BuildPattern(
        ILowLevelDocumentCreator arg, PdfDictionary shading, DictionaryBuilder builder) => builder
            .WithItem(KnownNames.Shading, arg.Add(shading))
            .WithItem(KnownNames.Matrix, Matrix3x2.CreateScale(5 * 72, 3 * 72).AsPdfArray())
            .WithItem(KnownNames.PatternType, 2);

    protected virtual DictionaryBuilder BuildShader(
        ILowLevelDocumentCreator arg, PdfObject[] localFunc, DictionaryBuilder builder) => builder
            .WithItem(KnownNames.Function, arg.Add(ComputeLocalFunc(localFunc, arg)))
            .WithItem(KnownNames.ShadingType, 1)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB);

    private static PdfObject ComputeLocalFunc(PdfObject[] localFunc, ILowLevelDocumentCreator ldc)
    {
        if (localFunc.Length == 1) return localFunc[0];
        return new PdfArray(localFunc.Select(i => ldc.Add(i)));
    }
}