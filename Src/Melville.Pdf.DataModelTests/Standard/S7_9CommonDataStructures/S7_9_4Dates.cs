using System;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Postscript.Interpreter.Values;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_9CommonDataStructures;

public class S7_9_4Dates
{
    public static object[][] DateTimeTests() => new[]
    {
        new object[] {"D:1975", new PdfTime(new DateTime(1975,1,1))},
        new object[] {"D:197507", new PdfTime(new DateTime(1975,07,1))},
        new object[] {"D:19750728", new PdfTime(new DateTime(1975,07,28))},
        new object[] {"D:1975072801", new PdfTime(new DateTime(1975,07,28,01,00,00))},
        new object[] {"D:197507280123", new PdfTime(new DateTime(1975,07,28,01,23,00))},
        new object[] {"D:19750728012359", new PdfTime(new DateTime(1975,07,28,01,23,59))},
        new object[] {"D:19750728012359+05", new PdfTime(new DateTime(1975,07,28,01,23,59), 5)},
        new object[] {"D:19750728012359-11", new PdfTime(new DateTime(1975,07,28,01,23,59), -11)},
        new object[] {"D:19750728012359-11'43", new PdfTime(new DateTime(1975,07,28,01,23,59), -11, 43)},
    };

    [Theory]
    [MemberData(nameof(DateTimeTests))]
    public void TestDateTimePrinter(string pdfFormat, PdfTime rec)
    {
        Assert.Equal(pdfFormat, PdfString.CreateDate(rec).AsTextString());
    }   

    [Theory]
    [MemberData(nameof(DateTimeTests))]
    public void TestDateTimeParserAscii(string pdfFormat, PdfTime rec)
    {
        Assert.Equal(rec, PdfString.CreateAscii(pdfFormat).AsPdfTime());
    }
[Theory]
    [MemberData(nameof(DateTimeTests))]
    public void TestDateTimeParserUtf8(string pdfFormat, PdfTime rec)
    {
        Assert.Equal(rec, PdfString.CreateUtf8(pdfFormat).AsPdfTime());
    }
[Theory]
    [MemberData(nameof(DateTimeTests))]
    public void TestDateTimeParserUtf16(string pdfFormat, PdfTime rec)
    {
        Assert.Equal(rec, PdfString.CreateUtf16(pdfFormat).AsPdfTime());
    }
[Theory]
    [MemberData(nameof(DateTimeTests))]
    public void TestDateTimeParserWithApostrophes(string pdfFormat, PdfTime rec)
    {
        Assert.Equal(rec, PdfString.CreateUtf16(pdfFormat+"'").AsPdfTime());
    }

    [Fact]
    public void ParseWithZ()
    {
        Assert.Equal(new PdfTime(new DateTime(1975,07,28,01,23,59)), 
            new PdfTimeParser(PdfDirectValue.CreateString("D:19750728012359Z"u8).Get<StringSpanSource>().GetSpan()).AsPdfTime());
            
    }
}