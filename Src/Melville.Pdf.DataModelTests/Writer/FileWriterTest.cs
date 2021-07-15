using Melville.Pdf.LowLevel.Model.LowLevel;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer
{
    public class FileWriterTest
    {
        [Fact]
        public void EmptyDocument()
        {
            var doc = new PdfLowLevelDocument(1,7,new PdfDictionary(
                new Dictionary
                ))
        }
    }
}