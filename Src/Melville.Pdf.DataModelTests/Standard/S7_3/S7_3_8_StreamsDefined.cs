﻿using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Hacks.Reflection;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_8_StreamsDefined
{
    private const string ObjectPrefix = "1 2 obj ";

    // for reference 
    // \n = 0xA = 10 = LF
    // \r = 0xd = 13 = CR
    [Theory]
    [InlineData("<</Length 6>> stream\r\n123456\r\nendstream")]
    [InlineData("<</Length 6>> stream\n123456\r\nendstream")]
    // PDF Spec section 7.3.8.1 says this is illegal but real pdf files do it, and PDF reader accepts it.
    [InlineData("<</Length 6>> stream\r123456\r\nendstream")]
    // PDF Spec section 7.3.8.1 does not specifically say this is legal but real pdf files do it, and PDF reader accepts it.
    [InlineData("<</Length 6>> stream \r\n123456\r\nendstream")]
    [InlineData("                                    <</Length 6>> stream\r123456\r\nendstream")]
    public async Task ParseSimpleStreamAsync(string data)
    {
        await RunStreamTestAsync(data, "123456");
    }

    [Theory]
    [InlineData("<</Length 6>> stream\r\n\n12345\r\nendstream", "\n12345")]
    [InlineData("<</Length 6>> stream\n\n12345\r\nendstream", "\n12345")]
    public static async Task RunStreamTestAsync(string data, string expected)
    {
        var (obj, context) = await $"{ObjectPrefix}{data}\r\nendobj".ParseRootObjectAsync();
        await using var cSharpStream = await obj.Get<PdfStream>().StreamContentAsync();
        Assert.Equal(expected, await new StreamReader(cSharpStream).ReadToEndAsync());
        context.Dispose();
    }
}