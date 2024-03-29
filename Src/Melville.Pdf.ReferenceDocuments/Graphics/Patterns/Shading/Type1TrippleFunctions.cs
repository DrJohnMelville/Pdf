﻿using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public class Type1TrippleFunctions : Type1FunctionalShaderBase
{
    public Type1TrippleFunctions() : base("Type 1 Shader with an array of functions")
    {
    }

    protected override async Task<PdfStream[]> BuildFunctionAsync()
    {
        var ret = new[]
        {
            await SingleMethodAsync((x, y) => x),
            await SingleMethodAsync((x, y) => y),
            await SingleMethodAsync((x, y) => (x + y) / 2.0)
        };
        return ret;
    }

    private static async Task<PdfStream> SingleMethodAsync(Func<double, double, double> defn)
    {
        var fbuilder = new SampledFunctionBuilder(4, SampledFunctionOrder.Linear);
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddOutput(defn, new ClosedInterval(0, 1));
        var ret = await fbuilder.CreateSampledFunctionAsync();
        return ret;
    }
}