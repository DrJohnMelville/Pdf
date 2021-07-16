using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.LowLevel;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer
{
    public class FileWriterTest
    {
        [Fact]
        public void EmptyDocument()
        {
            var builder = new LowLevelDocumentBuilder();
            builder.AddRootElement(builder.NewDictionary((KnownNames.Type, KnownNames.Catalog)));
            var doc = builder.CreateDocument();

        }
    }
}