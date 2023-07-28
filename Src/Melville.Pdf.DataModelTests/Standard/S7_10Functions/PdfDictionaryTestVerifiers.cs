using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions;

public static class PdfDictionaryTestVerifiers
{
    public static async Task VerifyPdfDoubleArrayAsync(this PdfValueDictionary str, PdfDirectValue name, params double[] values)
    {
        var domain = await str.GetAsync<PdfValueArray>(name);
        Assert.Equal(domain.Count, values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            Assert.Equal(values[i], await domain.GetAsync<double>(i));
        }
    }
    public static async Task VerifyNumberAsync(this PdfValueDictionary str, PdfDirectValue name, int value)
    {
        var number = await str.GetAsync<int>(name);
        Assert.Equal(value, number);
    }
    public static async Task VerifyNumberAsync(this PdfValueDictionary str, PdfDirectValue name, double value)
    {
        var number = await str.GetAsync<double>(name);
        Assert.Equal(value, number);
    }
}