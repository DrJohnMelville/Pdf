using System.Security.Cryptography;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Creators;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public class GraphicStateDictionary
{
    [Fact]
    public async Task name()
    {
        var pc = new PageCreator();
        pc.AddToContentStream(new DictionaryBuilder().AsStream("/GSDict gs"));
        pc.AddResourceObject(ResourceTypeName.ExtGState, NameDirectory.Get("GS1"), new DictionaryBuilder()
            .WithItem(KnownNames.Type, ResourceTypeName.ExtGState)
            .AsDictionary());
//        var gsd  = new GraphicStateDictionary()
    }
}