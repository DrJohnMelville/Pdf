using System;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.StreamInterfaces;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_3_NumbersDefined
{

    [Test]
    // from standard section 7.3.3 example 1
    [Arguments("123", 123)]
    [Arguments("43455", 43455)]
    [Arguments("+17", 17)]
    [Arguments("-98", -98)]
    [Arguments("0", 0)]
    public Task SpecExample1ItemsAsync(string input, int value) => ParseNumberSucceedAsync(input, value, value);

    [Test]
    // from standard section 7.3.3 example 2
    [Arguments("34.5", 34.5)]
    [Arguments("-3.62", -3.62)]
    [Arguments("+123.6", 123.6)]
    [Arguments("4.", 4)]
    [Arguments("-.002", -0.002)]
    public async Task SpecExample2ItemsAsync(string input, double value)
    {
        await ParseNumberSucceedAsync(input, (int)Math.Round(value, MidpointRounding.AwayFromZero),
                    value);
    }


    [Test]
    [Arguments("%comment\r\n34/", 34, 34)]
    // from standard sec
    public async Task ParseNumberSucceedAsync(string source, int intValue, double doubleValue)
    {
        var num = await (await source.ParseValueObjectAsync()).LoadValueAsync(); 
        Assert.Equal(intValue, num.Get<long>());
        Assert.Equal(doubleValue, num.Get<double>());
    }
}