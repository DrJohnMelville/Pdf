using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions;

public class S7_10_3ExponentialInterpolationFunctions
{
    [Fact]
    public async Task DeclareExponentialFunctionAsync()
    {
        var builder = new ExponentialFunctionBuilder(10);
        builder.AddFunction(5, 10);
        builder.AddFunction(15, 20);
        var dict = builder.Create();
        await dict.VerifyNumberAsync(KnownNames.FunctionTypeTName, 2);
        await dict.VerifyPdfDoubleArrayAsync(KnownNames.DomainTName, 0, 1);
        Assert.False(dict.ContainsKey(KnownNames.RangeTName));
        await dict.VerifyNumberAsync(KnownNames.NTName, 10);
        await dict.VerifyPdfDoubleArrayAsync(KnownNames.C0TName, 5, 15);
        await dict.VerifyPdfDoubleArrayAsync(KnownNames.C1TName, 10, 20);
    }

    [Fact]
    public async Task DeclareWithExplicitRangeAsync()
    {
        var builder = new ExponentialFunctionBuilder(2, (2, 3));
        builder.AddFunction(0, 1);
        builder.AddFunction(0, 1, (4,9));
        var dict = builder.Create();
        await dict.VerifyPdfDoubleArrayAsync(KnownNames.DomainTName, 2, 3);
        await dict.VerifyPdfDoubleArrayAsync(KnownNames.RangeTName, double.MinValue, double.MaxValue, 4, 9);
    }
    [Fact]
    public async Task DeclareWithDefaultsAsync()
    {
        var builder = new ExponentialFunctionBuilder(20);
        builder.AddFunction(0, 1);
        var dict = builder.Create();
        await dict.VerifyNumberAsync(KnownNames.NTName, 20);
        await dict.VerifyPdfDoubleArrayAsync(KnownNames.DomainTName, 0, 1);
        Assert.False(dict.ContainsKey(KnownNames.RangeTName));
        Assert.False(dict.ContainsKey(KnownNames.C0TName));
        Assert.False(dict.ContainsKey(KnownNames.C1TName));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(0.432)]
    public async Task DefaultFunctionIsLinearInterpolationAsync(double value)
    {
        var builder = new ExponentialFunctionBuilder(1);
        builder.AddFunction(0,1);
        var func = await builder.Create().CreateFunctionAsync();
        Assert.Equal(value, func.ComputeSingleResult(value));
            
    }
    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(0.432)]
    public async Task OffsetInterpolationAsync(double value)
    {
        var builder = new ExponentialFunctionBuilder(1);
        builder.AddFunction(5,6);
        var func = await builder.Create().CreateFunctionAsync();
        Assert.Equal(5+value, func.ComputeSingleResult(value));
            
    }
    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(0.432)]
    public async Task OffsetRangeInterpolationAsync(double value)
    {
        var builder = new ExponentialFunctionBuilder(1);
        builder.AddFunction(5,55);
        var func = await builder.Create().CreateFunctionAsync();
        Assert.Equal(5+(50*value), func.ComputeSingleResult(value));
            
    }
    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(0.432)]
    public async Task ExponentiationAsync(double value)
    {
        var builder = new ExponentialFunctionBuilder(3);
        builder.AddFunction(0, 1);
        var func = await builder.Create().CreateFunctionAsync();
        Assert.Equal(value*value*value, func.ComputeSingleResult(value));
            
    }
}