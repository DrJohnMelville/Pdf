using System.Numerics;
using Melville.CSJ2K.Color;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public class ShadingOperator: Card3x5{
    public ShadingOperator() : base("Use the shading operator to display a radial gradient")
    {
    }

    protected override async ValueTask SetPagePropertiesAsync(PageCreator page)
    {
        var func = await BuildFunction();
        page.AddResourceObject(ResourceTypeName.Shading, NameDirectory.Get("Sh1") ,
            ll=> new DictionaryBuilder()
                .WithItem(KnownNames.Function, ll.Add(func))
                .WithItem(KnownNames.Coords, new PdfArray(0.25, .4, 0.1, .35, .4, .01))
                .WithItem(KnownNames.ShadingType, 3)
                .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB)
                .WithItem(KnownNames.Background, new PdfArray(0,0,1))
                .AsDictionary());
        await base.SetPagePropertiesAsync(page);
    }

    private async Task<PdfDictionary> BuildFunction()
    {
        var fbuilder = new SampledFunctionBuilder(4, SampledFunctionOrder.Linear);
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddOutput((double _) => 1, new ClosedInterval(0, 1));
        fbuilder.AddOutput(x => x, new ClosedInterval(0, 1));
        fbuilder.AddOutput(x => x, new ClosedInterval(0, 1));
        return await fbuilder.CreateSampledFunction();
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.ModifyTransformMatrix(Matrix3x2.CreateScale(72f*5f, 72f*3f));
        await csw.PaintShader(NameDirectory.Get("Sh1"));
        await base.DoPaintingAsync(csw);
    }
}