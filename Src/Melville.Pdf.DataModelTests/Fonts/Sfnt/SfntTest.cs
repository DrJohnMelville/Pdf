using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Parsing.MultiplexSources;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt;

public class SfntTest
{
    [Fact]
    public async Task LoadTableBytesTestAsync()
    {
        var source = MultiplexSourceFactory.Create("01234567890"u8.ToArray());
        var sfnt = new SFnt(source, [
            new TableRecord(0, 2,4)
        ]);

        var result = await sfnt.GetTableBytesAsync(sfnt.Tables[0]);
        result.Should().BeEquivalentTo("2345"u8.ToArray());
    }
}