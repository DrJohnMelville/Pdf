using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.DataModelTests.Standard.S9_4Text;

public class TextStateOperatorsParserTest : ParserTest
{
    public static (string code, Expression<Action<IContentStreamOperations>> op) SimpleTextStateOperator(
        string code, Expression<Action<IContentStreamOperations>> op) =>
        (code, op );

    public static IEnumerable<(
        string code, Expression<Action<IContentStreamOperations>> op)> SimpleTextStateOperators() =>
        [
            SimpleTextStateOperator("Tc", i => i.SetCharSpace(55)),
            SimpleTextStateOperator("Tw", i => i.SetWordSpace(55)),
            SimpleTextStateOperator("Tz", i => i.SetHorizontalTextScaling(55)),
            SimpleTextStateOperator("TL", i => i.SetTextLeading(55)),
            SimpleTextStateOperator("Ts", i => i.SetTextRise(55)),
        ];


    [Test]
    [MethodDataSource(nameof(SimpleTextStateOperators))]
    public Task EmptyOperatorAsync(string code, Expression<Action<IContentStreamOperations>> op) =>
        TestInputAsync($"55 {code}\n", op);

    [Test]
    [Arguments(TextRendering.Fill)]
    [Arguments(TextRendering.Stroke)]
    [Arguments(TextRendering.FillAndStroke)]
    [Arguments(TextRendering.Invisible)]
    [Arguments(TextRendering.StrokeAndClip)]
    [Arguments(TextRendering.FillAndClip)]
    public Task TextRenderAsync(TextRendering rendering) => 
        TestInputAsync($"{(int)rendering} Tr", i => i.SetTextRender(rendering));

    [Test]
    public Task SetFontTestAsync() =>
        TestInputAsync("/Helvetica 12 Tf", i => i.SetFontAsync(BuiltInFontName.Helvetica, 12));

}