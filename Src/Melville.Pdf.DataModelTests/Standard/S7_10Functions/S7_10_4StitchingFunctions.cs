using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions;

public class S7_10_4StitchingFunctions
{
    private PdfValueDictionary LinearMapping(int min, int max)
    {
        var builder = new ExponentialFunctionBuilder(1);
        builder.AddFunction(min, max);
        return builder.Create();
    }

    [Fact]
    public async Task SimpleStitchAsync()
    {
        var builder = new StitchingFunctionBuilder(0);

        builder.AddFunction(LinearMapping(0, 1), 0.5);
        builder.AddFunction(LinearMapping(2,3), 1.0, (2,3));
        var stitched = builder.Create();
        await stitched.VerifyNumberAsync(KnownNames.FunctionTypeTName, 3);
        await stitched.VerifyPdfDoubleArrayAsync(KnownNames.DomainTName, 0, 1.0);
        await stitched.VerifyPdfDoubleArrayAsync(KnownNames.BoundsTName, 0.5);
        await stitched.VerifyPdfDoubleArrayAsync(KnownNames.EncodeTName, 0, 0.5, 2, 3);
    }

    [Fact]
    public void CannotAddBelowMinimum()
    {
        var builder = new StitchingFunctionBuilder(0);
        Assert.Throws<ArgumentException>(() => builder.AddFunction(new ValueDictionaryBuilder().AsDictionary(), -1));
    }
    [Fact]
    public void CannotAddBelowLast()
    {
        var builder = new StitchingFunctionBuilder(0);
        builder.AddFunction(new ValueDictionaryBuilder().AsDictionary(), 0.5);
        Assert.Throws<ArgumentException>(() => builder.AddFunction(new ValueDictionaryBuilder().AsDictionary(), 0.25));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0.1, 0.2)]
    [InlineData(0.25, 0.5)]
    [InlineData(0.75, 0.5)]
    [InlineData(0.9, 0.2)]
    [InlineData(1, 0)]
    public async Task TriangleFunctionAsync(double input, double output)
    {
        var innerFunc = LinearMapping(0, 1);
        var builder = new StitchingFunctionBuilder(0);
        builder.AddFunction(innerFunc, 0.5, (0, 1));
        builder.AddFunction(innerFunc, 1.0, (1.0, 0.0));
        var func = await builder.Create().CreateFunctionAsync();
        Assert.Equal(output, func.ComputeSingleResult(input), 3);
            
    }
}