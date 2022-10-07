using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer;

public class NameWriterTest
{
    [Theory]
    [InlineData("/","")]
    [InlineData("/Name1","Name1")]
    [InlineData("/ASomewhatLongerName","ASomewhatLongerName")]
    [InlineData("/A;NameWith-Various***Characters?","A;NameWith-Various***Characters?")]
    [InlineData("/1.2","1.2")]
    [InlineData("/$$","$$")]
    [InlineData("/@pattern","@pattern")]
    [InlineData("/.notdef",".notdef")]
    [InlineData("/Lime#20Green","Lime Green")]
    [InlineData("/Paired#28#29parentheses","Paired()parentheses")]
    [InlineData("/The_Key_of_F#23_Minor","The_Key_of_F#_Minor")]
    public async Task WriteName(string printedAs, string nameText)
    {
        var correctResult = printedAs.AsExtendedAsciiBytes();
        var dest = new MemoryStream();
        await NameWriter.Write(PipeWriter.Create(dest), NameDirectory.Get(nameText));
        Assert.Equal(correctResult, dest.ToArray());
    }
}