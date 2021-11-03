using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_4Text;

public class TextStateOperatorsParserTest : ParserTest
{
    public static object[] SimpleTextStateOperator(
        string code, Expression<Action<IContentStreamOperations>> op) =>
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
    public Task EmptyOperator(string code, Expression<Action<IContentStreamOperations>> op) =>
        TestInput($"55 {code}\n", op);

    [Theory]
    [InlineData(TextRendering.Fill)]
    [InlineData(TextRendering.Stroke)]
    [InlineData(TextRendering.FillAndStroke)]
    [InlineData(TextRendering.Invisible)]
    [InlineData(TextRendering.StrokeAndClip)]
    [InlineData(TextRendering.FillAndClip)]
    public Task TextRender(TextRendering rendering) => 
        TestInput($"{(int)rendering} Tr", i => i.SetTextRender(rendering));

    [Fact]
    public Task SetFontTest() =>
        TestInput("/Helvetica 12 Tf", i => i.SetFont(BuiltInFontName.Helvetica, 12));

}