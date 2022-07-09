using System.Collections.Immutable;
using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public abstract class Type1FunctionalShaderBase: PatternDisplayClass
{
    protected Type1FunctionalShaderBase(string helpText) : base(helpText)
    {
    }

    private PdfObject? function = null;
    protected override async ValueTask SetPagePropertiesAsync(PageCreator page)
    {
        var fbuilder = new SampledFunctionBuilder(4, SampledFunctionOrder.Linear);
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddInput(2, new ClosedInterval(0,1));
        fbuilder.AddOutput((x,y)=>x, new ClosedInterval(0,1));
        fbuilder.AddOutput((x,y)=>y, new ClosedInterval(0,1));
        fbuilder.AddOutput((x,y)=>1-x, new ClosedInterval(0,1));    
        function = await fbuilder.CreateSampledFunction();

        await base.SetPagePropertiesAsync(page);
    }

    protected override PdfObject CreatePattern(ILowLevelDocumentCreator arg)
    {
        if (function is not { } localFunc) throw new InvalidOperationException("No func defined");
        var shading = new DictionaryBuilder()
            .WithItem(KnownNames.Function, arg.Add(localFunc))
            .WithItem(KnownNames.ShadingType, 1)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB)
            .AsDictionary();
        return new DictionaryBuilder()
            .WithItem(KnownNames.Shading, arg.Add(shading))
            .WithItem(KnownNames.Matrix, Matrix3x2.CreateScale(5*72,3*72).AsPdfArray())
            .WithItem(KnownNames.PatternType, 2)
            .AsDictionary();
    }
}

public class Type1FunctionalShader : Type1FunctionalShaderBase
{
    public Type1FunctionalShader() : base("Type 1 shader from a pdffunction")
    {
        
    }
}
