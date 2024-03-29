﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_4Text;

public class TextStateOperatorsWriteTest: WriterTest
{
    
    public static object[] SimpleTextStateOperator(
        string code, Action<IContentStreamOperations> op) =>
        new object[] { code, op };

    public static IEnumerable<object[]> SimpleTextStateOperators() =>
        new[]
        {
            SimpleTextStateOperator("Tc", i => i.SetCharSpace(55)),
            SimpleTextStateOperator("Tw", i => i.SetWordSpace(55)),
            SimpleTextStateOperator("Tz", i => i.SetHorizontalTextScaling(55)),
            SimpleTextStateOperator("TL", i => i.SetTextLeading(55)),
            SimpleTextStateOperator("Ts", i => i.SetTextRise(55)),
        };

    [Theory]
    [MemberData(nameof(SimpleTextStateOperators))]
    public async Task WriteTextStateOperatorsAsync(string code, Action<IContentStreamOperations> op)
    {
        op(sut);
        Assert.Equal($"55 {code}\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task SetFontAsync()
    {
        await sut.SetFontAsync(BuiltInFontName.Helvetica, 12);
        Assert.Equal("/Helvetica 12 Tf\n", await WrittenTextAsync());
        
    }
    [Fact]
    public async Task SetFontStringAsync()
    {
        await sut.SetFontAsync("Jdm", 12);
        Assert.Equal("/Jdm 12 Tf\n", await WrittenTextAsync());
        
    }

    [Theory]
    [InlineData(TextRendering.Fill)]
    [InlineData(TextRendering.Stroke)]
    [InlineData(TextRendering.FillAndStroke)]
    [InlineData(TextRendering.Invisible)]
    [InlineData(TextRendering.StrokeAndClip)]
    [InlineData(TextRendering.FillAndClip)]
    public async Task TextRenderAsync(TextRendering rendering)
    {
        sut.SetTextRender(rendering);
        Assert.Equal($"{(int)rendering} Tr\n", await WrittenTextAsync());
        
    }

}