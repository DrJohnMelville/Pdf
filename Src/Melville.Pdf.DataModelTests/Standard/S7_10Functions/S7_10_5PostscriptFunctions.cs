using System;
using System.Linq;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.ShortStrings;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using System.Globalization;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions;

public class S7_10_5PostscriptFunctions
{
    [Test]
    public async Task CreatePostScriptFunctionAsync()
    {
        var builder = new PostscriptFunctionBuilder();
        builder.AddArgument((0,10));
        builder.AddOutput((0,20));
        var dict = builder.Create("2 mul", 
            new DictionaryBuilder().WithItem(KnownNames.Decode, KnownNames.FlateDecode));
        Assert.Equal(KnownNames.FlateDecode, await dict[KnownNames.Decode]);
        await dict.VerifyNumberAsync(KnownNames.FunctionType, 4);
        await dict.VerifyPdfDoubleArrayAsync(KnownNames.Domain, 0, 10);
        await dict.VerifyPdfDoubleArrayAsync(KnownNames.Range, 0, 20);
        Assert.Equal("2 mul", await (await dict.ReaderForContentAsync()).ReadAsStringAsync());
            
    }

    [Test]
    public void HashLowerCase()
    {
        Assert.Equal(FnvHash.HashLowerCase("abs"u8), FnvHash.HashLowerCase("Abs"u8));
    }


    [Test]
    [Arguments("10.3", "10.3", "{abs}")]
    [Arguments("-10.3", "10.3", "{abs}")]
    [Arguments("+10.3", "10.3", "{abs}")]
    [Arguments("5", "8", "{3.0 add}")]
    [Arguments("5", "2.5", "{-2.5 add}")]
    [Arguments("4,4", "45", "{atan}")]
    [Arguments("-1,1", "-45", "{atan}")]
    [Arguments("1,0", "90", "{atan}")]
    [Arguments("0,1", "0", "{atan}")]
    [Arguments("-100,0", "-90", "{atan}")]
    [Arguments("1.4", "2", "{ceiling}")]
    [Arguments("0", "1", "{cos}")]
    [Arguments("90", "0", "{cos}")]
    [Arguments("10.2", "10", "{cvi}")]
    [Arguments("10", "10", "{cvr}")]
    [Arguments("5,2", "2.5", "{div}")]
    [Arguments("5,2", "25", "{exp}")]
    [Arguments("5.2", "5", "{floor}")]
    [Arguments("10,3", "3", "{idiv}")]
    [Arguments("10", "2.303", "{ln}")]
    [Arguments("100", "4.605", "{ln}")]
    [Arguments("10", "1", "{log}")]
    [Arguments("100", "2", "{log}")]
    [Arguments("5, 3", "2", "{mod}")]
    [Arguments("-5, 3", "-2", "{mod}")]
    [Arguments("5, 2", "1", "{mod}")]
    [Arguments("1", "2", "{2 mul}")]
    [Arguments("2,3", "6", "{mul}")]
    [Arguments("4.5", "-4.5", "{neg}")]
    [Arguments("-3", "3", "{neg}")]
    [Arguments("3.2", "3", "{round}")]
    [Arguments("6.5", "7", "{round}")]
    [Arguments("-4.8", "-5.0", "{round}")]
    [Arguments("-6.5", "-7", "{round}")]
    [Arguments("99", "99", "{round}")]
    [Arguments("0", "0", "{sin}")]
    [Arguments("90", "1", "{sin}")]
    [Arguments("180", "0", "{sin}")]
    [Arguments("270", "-1", "{sin}")]
    [Arguments("81", "9", "{sqrt}")]
    [Arguments("5,4", "1", "{sub}")]
    [Arguments("3.2", "3", "{truncate}")]
    [Arguments("-4.8", "-4", "{truncate}")]
    [Arguments("99", "99", "{truncate}")]
    [Arguments("99,1", "1", "{and}")]
    [Arguments("52,7", "4", "{and}")]
    [Arguments("0,0", "0", "{and}")]
    [Arguments("0,1", "0", "{and}")]
    [Arguments("1,0", "0", "{and}")]
    [Arguments("1,1", "1", "{and}")]
    [Arguments("0,0", "0", "{or}")]
    [Arguments("0,1", "1", "{or}")]
    [Arguments("1,0", "1", "{or}")]
    [Arguments("1,1", "1", "{or}")]
    [Arguments("0,0", "0", "{xor}")]
    [Arguments("0,1", "1", "{xor}")]
    [Arguments("1,0", "1", "{xor}")]
    [Arguments("1,1", "0", "{xor}")]
    [Arguments("7,3", "56", "{bitshift}")]
    [Arguments("142, -3", "17", "{bitshift}")]
    [Arguments("0,0", "-1", "{eq}")]
    [Arguments("1,1", "-1", "{eq}")]
    [Arguments("1,0", "0", "{eq}")]
    [Arguments("0,0", "0", "{ne}")]
    [Arguments("1,1", "0", "{ne}")]
    [Arguments("1,0", "-1",  "{ne}")]
    [Arguments("4", "4,0", "{false}")]
    [Arguments("4", "4,-1", "{true}")]
    [Arguments("1,2", "0",  "{ge}")]
    [Arguments("2,2", "-1", "{ge}")]
    [Arguments("3,2", "-1", "{ge}")]
    [Arguments("1,2", "0",  "{gt}")]
    [Arguments("2,2", "0",  "{gt}")]
    [Arguments("3,2", "-1", "{gt}")]
    [Arguments("1,2", "-1", "{le}")]
    [Arguments("2,2", "-1", "{le}")]
    [Arguments("3,2", "0",  "{le}")]
    [Arguments("1,2", "-1", "{lt}")]
    [Arguments("2,2", "0",  "{lt}")]
    [Arguments("3,2", "0",  "{lt}")]
    [Arguments("0", "-1",  "{not}")]
    [Arguments("-1", "0",  "{not}")]
    [Arguments("1,2,3,2", "1,2,3,2,3", "{copy}")]
    [Arguments("1,2,3,3", "1,2,3,1,2,3", "{copy}")]
    [Arguments("1", "1,1", "{dup}")]
    [Arguments("1,2", "2,1", "{exch}")]
    [Arguments("1,2,3,1", "1,2,3,2", "{index}")]
    [Arguments("1,2,3,1", "1,2,3", "{pop}")]
    [Arguments("5,6,7,3,-1", "6,7,5", "{roll}")]
    [Arguments("5,6,7,3,1", "7,5,6", "{roll}")]
    [Arguments("5,6,7,3,0", "5,6,7", "{roll}")]
    [Arguments("5,6,7,3,-7", "6,7,5", "{roll}")]
    [Arguments("5,6,7,3,7", "7,5,6", "{roll}")]
    [Arguments("5,6,7,3,9", "5,6,7", "{roll}")]
    [Arguments("5,6,7,3,2", "6,7,5", "{roll}")]
    [Arguments("5,6,7,3,-2", "7,5,6", "{roll}")]
    [Arguments("10, -1", "10,12", "{dup eq {12} if}")]
    [Arguments("10, 0", "10", "{dup ne {12} if}")]
    [Arguments("10, -1", "10,12", "{dup eq {12} {20} ifelse}")]
    [Arguments("10, 0", "10,20", "{dup ne {12} {20} ifelse}")]
    [Arguments("3,4", "5", "{ dup mul exch dup mul add sqrt}")]
    public Task PostScriptTestAsync(string inputs, string outputs, string code) => 
        InnerPostScriptTestAsync(GetDoubles(inputs), GetDoubles(outputs), code);

    private async Task InnerPostScriptTestAsync(double[] inputs, double[] outputs, string code)
    {
        var func = await CreateFunction(code, inputs.Length, outputs.Length).CreateFunctionAsync();
        Assert.SeqEqual(outputs, func.Compute(inputs).Select(i=>Math.Round(i,3)));
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
            .Select(i => double.Parse(i, CultureInfo.InvariantCulture))
            .ToArray();
}