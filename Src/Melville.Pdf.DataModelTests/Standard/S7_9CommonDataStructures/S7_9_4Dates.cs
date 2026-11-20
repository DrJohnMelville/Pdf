using System;
using System.Collections;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

namespace Melville.Pdf.DataModelTests.Standard.S7_9CommonDataStructures;

public class S7_9_4Dates
{
    public static IEnumerable<(string, PdfTime)> DateTimeTests() => 
    [
        ("D:1975", new PdfTime(new DateTime(1975,1,1))),
        ("D:197507", new PdfTime(new DateTime(1975,07,1))),
        ("D:19750728", new PdfTime(new DateTime(1975,07,28))),
        ("D:1975072801", new PdfTime(new DateTime(1975,07,28,01,00,00))),
        ("D:197507280123", new PdfTime(new DateTime(1975,07,28,01,23,00))),
        ("D:19750728012359", new PdfTime(new DateTime(1975,07,28,01,23,59))),
        ("D:19750728012359+05", new PdfTime(new DateTime(1975,07,28,01,23,59), 5)),
        ("D:19750728012359-11", new PdfTime(new DateTime(1975,07,28,01,23,59), -11)),
        ("D:19750728012359-11'43", new PdfTime(new DateTime(1975,07,28,01,23,59), -11, 43)),
    ];

    [Test]
    [MethodDataSource(nameof(DateTimeTests))]
    public void TestDateTimePrinter(string pdfFormat, PdfTime rec)
    {
        Assert.Equal(pdfFormat, rec.AsPdfBytes().AsTextString());
    }   

    [Test]
    [MethodDataSource(nameof(DateTimeTests))]
    public void TestDateTimeParserAscii(string pdfFormat, PdfTime rec)
    {
        Assert.Equal(rec, new PdfTimeParser(pdfFormat.AsExtendedAsciiBytes()).AsPdfTime());
    }
[Test]
    [MethodDataSource(nameof(DateTimeTests))]
    public void TestDateTimeParserUtf8(string pdfFormat, PdfTime rec)
    {
        Assert.Equal(rec, PdfDirectObject.CreateUtf8String(pdfFormat).AsPdfTime());
    }
[Test]
    [MethodDataSource(nameof(DateTimeTests))]
    public void TestDateTimeParserUtf16(string pdfFormat, PdfTime rec)
    {
        Assert.Equal(rec, PdfDirectObject.CreateUtf16String(pdfFormat).AsPdfTime());
    }
[Test]
    [MethodDataSource(nameof(DateTimeTests))]
    public void TestDateTimeParserWithApostrophes(string pdfFormat, PdfTime rec)
    {
        Assert.Equal(rec, PdfDirectObject.CreateUtf16String(pdfFormat+"'").AsPdfTime());
    }

    [Test]
    public void ParseWithZ()
    {
        Assert.Equal(new PdfTime(new DateTime(1975,07,28,01,23,59)), 
            PdfDirectObject.CreateString("D:19750728012359Z"u8).AsPdfTime());
            
    }
}