using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Linq;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions;

public class S7_10_5PostscriptFunctions
{
    [Fact]
    public async Task CreatePostScriptFunction()
    {
        var builder = new PostscriptFunctionBuilder();
        builder.AddArgument((0,10));
        builder.AddOutput((0,20));
        var dict = builder.Create("2 mul", 
            new DictionaryBuilder().WithItem(KnownNames.Decode, KnownNames.FlateDecode));
        Assert.Equal(KnownNames.FlateDecode, await dict.GetAsync<PdfName>(KnownNames.Decode));
        await dict.VerifyNumber(KnownNames.FunctionType, 4);
        await dict.VerifyPdfDoubleArray(KnownNames.Domain, 0, 10);
        await dict.VerifyPdfDoubleArray(KnownNames.Range, 0, 20);
        Assert.Equal("2 mul", await (await dict.StreamContentAsync()).ReadAsStringAsync());
            
    }


    [Theory]
    [InlineData("10.3", "10.3", "{abs}")]
    [InlineData("-10.3", "10.3", "{abs}")]
    [InlineData("+10.3", "10.3", "{abs}")]
    [InlineData("5", "8", "{3.0 add}")]
    [InlineData("5", "2.5", "{-2.5 add}")]
    [InlineData("4,4", "45", "{atan}")]
    [InlineData("-1,1", "315", "{atan}")]
    [InlineData("1,0", "90", "{atan}")]
    [InlineData("0,1", "0", "{atan}")]
    [InlineData("-100,0", "270", "{atan}")]
    [InlineData("1.4", "2", "{ceiling}")]
    [InlineData("0", "1", "{cos}")]
    [InlineData("90", "0", "{cos}")]
    [InlineData("10.2", "10", "{cvi}")]
    [InlineData("10", "10", "{cvr}")]
    [InlineData("5,2", "2.5", "{div}")]
    [InlineData("5,2", "25", "{exp}")]
    [InlineData("5.2", "5", "{floor}")]
    [InlineData("10,3", "3", "{idiv}")]
    [InlineData("10", "2.303", "{ln}")]
    [InlineData("100", "4.605", "{ln}")]
    [InlineData("10", "1", "{log}")]
    [InlineData("100", "2", "{log}")]
    [InlineData("5, 3", "2", "{mod}")]
    [InlineData("-5, 3", "-2", "{mod}")]
    [InlineData("5, 2", "1", "{mod}")]
    [InlineData("1", "2", "{2 mul}")]
    [InlineData("2,3", "6", "{mul}")]
    [InlineData("4.5", "-4.5", "{neg}")]
    [InlineData("-3", "3", "{neg}")]
    [InlineData("3.2", "3", "{round}")]
    [InlineData("6.5", "7", "{round}")]
    [InlineData("-4.8", "-5.0", "{round}")]
    [InlineData("-6.5", "-6", "{round}")]
    [InlineData("99", "99", "{round}")]
    [InlineData("0", "0", "{sin}")]
    [InlineData("90", "1", "{sin}")]
    [InlineData("180", "0", "{sin}")]
    [InlineData("270", "-1", "{sin}")]
    [InlineData("81", "9", "{sqrt}")]
    [InlineData("5,4", "1", "{sub}")]
    [InlineData("3.2", "3", "{truncate}")]
    [InlineData("-4.8", "-4", "{truncate}")]
    [InlineData("99", "99", "{truncate}")]
    [InlineData("99,1", "1", "{and}")]
    [InlineData("52,7", "4", "{and}")]
    [InlineData("0,0", "0", "{and}")]
    [InlineData("0,1", "0", "{and}")]
    [InlineData("1,0", "0", "{and}")]
    [InlineData("1,1", "1", "{and}")]
    [InlineData("0,0", "0", "{or}")]
    [InlineData("0,1", "1", "{or}")]
    [InlineData("1,0", "1", "{or}")]
    [InlineData("1,1", "1", "{or}")]
    [InlineData("0,0", "0", "{xor}")]
    [InlineData("0,1", "1", "{xor}")]
    [InlineData("1,0", "1", "{xor}")]
    [InlineData("1,1", "0", "{xor}")]
    [InlineData("7,3", "56", "{bitshift}")]
    [InlineData("142, -3", "17", "{bitshift}")]
    [InlineData("0,0", "-1", "{eq}")]
    [InlineData("1,1", "-1", "{eq}")]
    [InlineData("1,0", "0", "{eq}")]
    [InlineData("0,0", "0", "{ne}")]
    [InlineData("1,1", "0", "{ne}")]
    [InlineData("1,0", "-1",  "{ne}")]
    [InlineData("4", "4,0", "{false}")]
    [InlineData("4", "4,-1", "{true}")]
    [InlineData("1,2", "0",  "{ge}")]
    [InlineData("2,2", "-1", "{ge}")]
    [InlineData("3,2", "-1", "{ge}")]
    [InlineData("1,2", "0",  "{gt}")]
    [InlineData("2,2", "0",  "{gt}")]
    [InlineData("3,2", "-1", "{gt}")]
    [InlineData("1,2", "-1", "{le}")]
    [InlineData("2,2", "-1", "{le}")]
    [InlineData("3,2", "0",  "{le}")]
    [InlineData("1,2", "-1", "{lt}")]
    [InlineData("2,2", "0",  "{lt}")]
    [InlineData("3,2", "0",  "{lt}")]
    [InlineData("0", "-1",  "{not}")]
    [InlineData("-1", "0",  "{not}")]
    [InlineData("1,2,3,2", "1,2,3,2,3", "{copy}")]
    [InlineData("1,2,3,3", "1,2,3,1,2,3", "{copy}")]
    [InlineData("1", "1,1", "{dup}")]
    [InlineData("1,2", "2,1", "{exch}")]
    [InlineData("1,2,3,1", "1,2,3,2", "{index}")]
    [InlineData("1,2,3,1", "1,2,3", "{pop}")]
    [InlineData("5,6,7,3,-1", "6,7,5", "{roll}")]
    [InlineData("5,6,7,3,1", "7,5,6", "{roll}")]
    [InlineData("5,6,7,3,0", "5,6,7", "{roll}")]
    [InlineData("5,6,7,3,-7", "6,7,5", "{roll}")]
    [InlineData("5,6,7,3,7", "7,5,6", "{roll}")]
    [InlineData("5,6,7,3,9", "5,6,7", "{roll}")]
    [InlineData("5,6,7,3,2", "6,7,5", "{roll}")]
    [InlineData("5,6,7,3,-2", "7,5,6", "{roll}")]
    [InlineData("10, -1", "10,12", "{ {12} if}")]
    [InlineData("10, 0", "10", "{ {12} if}")]
    [InlineData("10, -1", "10,12", "{ {12} {20} ifelse}")]
    [InlineData("10, 0", "10,20", "{ {12} {20} ifelse}")]
    [InlineData("3,4", "5", "{ dup mul exch dup mul add sqrt}")]
    public Task PostScriptTest(string inputs, string outputs, string code) => 
        InnerPostScriptTest(GetDoubles(inputs), GetDoubles(outputs), code);

    private async Task InnerPostScriptTest(double[] inputs, double[] outputs, string code)
    {
        var func = await CreateFunction(code, inputs.Length, outputs.Length).CreateFunctionAsync();
        Assert.Equal(outputs, func.Compute(inputs).Select(i=>Math.Round(i,3)));
    }

    private static PdfDictionary CreateFunction(string code, int inputCount, int outputCount)
    {
        var builder = new PostscriptFunctionBuilder();
        for (int i = 0; i < inputCount; i++)
        {
            builder.AddArgument((-1000, 1000));
        }

        for (int i = 0; i < outputCount; i++)
        {
            builder.AddOutput((-1000, 1000));
        }

        return builder.Create(code);
    }

    private static double[] GetDoubles(string inputs) =>
        inputs
            .Split(new[] { ',' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(i => double.Parse(i))
            .ToArray();

    [Theory]
    [InlineData("{12 if}")]
    [InlineData("{12 ifelse}")]
    [InlineData("{12 {1} {2} if}")]
    [InlineData("{12 {1}  ifelse}")]
    [InlineData("{12 {1}  add}")]
    [InlineData("{12")]
    public Task PostscriptCompilerError(string code) => 
        Assert.ThrowsAsync<PdfParseException>(() => CreateFunction(code, 1, 1).CreateFunctionAsync().AsTask());
}