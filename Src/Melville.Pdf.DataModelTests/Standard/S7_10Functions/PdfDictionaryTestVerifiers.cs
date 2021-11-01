using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions;

public static class PdfDictionaryTestVerifiers
{
    public static async Task VerifyPdfDoubleArray(this PdfDictionary str, PdfName name, params double[] values)
    {
        var domain = await str.GetAsync<PdfArray>(name);
        Assert.Equal(domain.Count, values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            Assert.Equal(values[i], await domain.GetAsync<PdfNumber>(i));
        }
    }
    public static async Task VerifyNumber(this PdfDictionary str, PdfName name, int value)
    {
        var number = await str.GetAsync<PdfNumber>(name);
        Assert.Equal(value, number.IntValue);
    }
    public static async Task VerifyNumber(this PdfDictionary str, PdfName name, double value)
    {
        var number = await str.GetAsync<PdfNumber>(name);
        Assert.Equal(value, number.DoubleValue);
    }
}