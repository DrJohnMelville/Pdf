using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public class Type1FunctionalGray : Type1FunctionalShaderBase
{
    public Type1FunctionalGray() : base("Type 1 shader from a pdffunction")
    {
        
    }

    protected override async Task<PdfStream[]> BuildFunctionAsync()
    {
        var fbuilder = new SampledFunctionBuilder(4, SampledFunctionOrder.Linear);
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddOutput((x, y) => x*y, new ClosedInterval(0, 1));
        var ret = await fbuilder.CreateSampledFunctionAsync();
        return new PdfStream[] { ret };
    }

    protected override DictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfStream[] localFunc,
        DictionaryBuilder builder) => 
        base.BuildShader(arg, localFunc, builder).WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceGrayTName);
}