﻿using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Postscript.Interpreter.Tokenizers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Primatives;

public class PdfObjectTokenizerTest
{
    [Theory]
    [InlineData("true false", "true", "false")]
    [InlineData("true%hello\r\nfalse", "true", "false")]
    public async Task TwoTokenTestAsync(string content, string text1, string text2)
    {
        var source = new MemoryWrapper(content.AsExtendedAsciiBytes());
        var parser = new PdfTokenizer(source);

        Assert.Equal(text1, (await parser.NextTokenAsync()).ToString());
        Assert.Equal(text2, (await parser.NextTokenAsync()).ToString());
    }
}