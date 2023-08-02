using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions;

public class S7_10_2SampledFunctions
{
    private static async Task<PdfValueStream> ComplexSampledFunctionAsync()
    {
        var builder = new SampledFunctionBuilder(8, SampledFunctionOrder.Cubic);
        builder.AddInput(12, (1, 10), (1, 10));
        builder.AddOutput(x => 5 * x, (5, 50), (0, 255));
        return await builder.CreateSampledFunctionAsync(new ValueDictionaryBuilder().WithFilter(FilterName.ASCIIHexDecode));
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(0, 10)]
    [InlineData(0.5, 6.5)]
    public async Task SampledFunctionDecodeAsync(double input, double output)
    {
        var funcDict = new ValueDictionaryBuilder()
            .WithItem(KnownNames.BitsPerSampleTName, 8)
            .WithItem(KnownNames.FunctionTypeTName, 0)
            .WithItem(KnownNames.DecodeTName, new PdfValueArray(3, 10))
            .WithItem(KnownNames.RangeTName, new PdfValueArray(-10, 20))
            .WithItem(KnownNames.DomainTName, new PdfValueArray(0, 1))
            .WithItem(KnownNames.SizeTName, new PdfValueArray(2))
            .AsStream(new byte[] { 0xFF, 0x00 });
        var func = await funcDict.CreateFunctionAsync();
        Assert.Equal(output, func.ComputeSingleResult(input));

    }

    [Fact]
    public async Task CreateFullySpecifiedFunctionAsync()
    {
        var str = await ComplexSampledFunctionAsync();
        Assert.Equal(0, await str.GetAsync<int>(KnownNames.FunctionTypeTName));
        await str.VerifyPdfDoubleArrayAsync(KnownNames.DomainTName, 1, 10);
        await str.VerifyPdfDoubleArrayAsync(KnownNames.RangeTName, 5, 50);
        await str.VerifyPdfDoubleArrayAsync(KnownNames.SizeTName, 12);
        await str.VerifyNumberAsync(KnownNames.BitsPerSampleTName, 8);
        await str.VerifyNumberAsync(KnownNames.OrderTName, 3);
        await str.VerifyPdfDoubleArrayAsync(KnownNames.EncodeTName, 1, 10);
        await str.VerifyPdfDoubleArrayAsync(KnownNames.DecodeTName, 0, 255);
        await StreamTest.VerifyStreamContentAsync(
            "00050A0F14191E23282D3237", await str.StreamContentAsync(StreamFormat.ImplicitEncryption));
    }

    [Theory]
    [InlineData(-200, 5)]
    [InlineData(0.9, 5)]
    [InlineData(1, 5)]
    [InlineData(3, 15)]
    [InlineData(10, 50)]
    [InlineData(2.5, 12.5)]
    [InlineData(10.1, 50)]
    [InlineData(10000, 50)]
    public async Task EvaluateFullySpecifiedFunctionAsync(double input, double output)
    {
        var str = await ComplexSampledFunctionAsync();
        var func = await str.CreateFunctionAsync();
        Assert.Equal(output, func.ComputeSingleResult(input));
    }

    [Theory]
    [InlineData(1,2)]
    [InlineData(1.24,3.14)]
    [InlineData(9,9)]
    public async Task TwoDimensionalTwoOutputFunctionAsync(double inputA, double inputB)
    {
        var builder = new SampledFunctionBuilder(8);
        builder.AddInput(10,(0,9));
        builder.AddInput(10,(0,9));
        builder.AddOutput((x,y)=>2*x+3*y, (0, 255));
        builder.AddOutput((x,y)=>3*x+4*y, (0, 255));
        var str = await builder.CreateSampledFunctionAsync();
        var func = await str.CreateFunctionAsync();
        var result = func.Compute(new[] { inputA, inputB });
        Assert.Equal(2*inputA + 3*inputB, result[0]);
        Assert.Equal(3*inputA + 4 * inputB, result[1]);
    }        

    [Theory]
    [InlineData(1,2)]
    [InlineData(1.24,3.14)]
    [InlineData(9,9)]
    public async Task TwoDimensionalTwoOutputFunctionWithMultiByteSamplesAsync(double inputA, double inputB)
    {
        var builder = new SampledFunctionBuilder(24);
        builder.AddInput(10,(0,9));
        builder.AddInput(10,(0,9));
        builder.AddOutput((x,y)=>2*x+3*y, (0, 255));
        builder.AddOutput((x,y)=>3*x+4*y, (0, 255));
        var str = await builder.CreateSampledFunctionAsync();
        var func = await str.CreateFunctionAsync();
        var result = func.Compute(new[] { inputA, inputB });
        Assert.Equal(2*inputA + 3*inputB, result[0]);
        Assert.Equal(3*inputA + 4 * inputB, result[1]);
    }        
        
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 4)]
    [InlineData(5.25, 27.75)]
    [InlineData(9, 81)]
    public async Task CubicInterpolationAsync(double inputA, double output)
    {
        var builder = new SampledFunctionBuilder(8);
        builder.AddInput(11,(0,10));
        builder.AddOutput(x=>x*x, (0, 100), (0,255));
        var str = await builder.CreateSampledFunctionAsync();
        var func = await str.CreateFunctionAsync();
        Assert.Equal(output, func.ComputeSingleResult(inputA));
    }
        
        


    [Fact]
    public async Task DoNotStateUnneededOptionalArgumentsAsync()
    {
        var builder = new SampledFunctionBuilder(8);
        builder.AddInput(12,(1,10));
        builder.AddOutput(x=>5*x, (0, 255));
        var str = await builder.CreateSampledFunctionAsync();
            
        Assert.False(str.ContainsKey(KnownNames.OrderTName));
        Assert.False(str.ContainsKey(KnownNames.EncodeTName));
        Assert.False(str.ContainsKey(KnownNames.DecodeTName));
    }
}