using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure
{
    public class S7_5_6IncrementalUpdates
    {
        public async Task<PdfLoadedLowLevelDocument> CompositeDocument(Action<ILowLevelDocumentCreator> create,
            Func<PdfLoadedLowLevelDocument, ILowLevelDocumentModifier,Task> modify)
        {
            var creator = new LowLevelDocumentCreator();
            create(creator);
            var doc = creator.CreateDocument();
            var stream = new MultiBufferStream();
            await doc.WriteTo(stream);

            var ld = await RandomAccessFileParser.Parse(stream.CreateReader());
            var modifier = new LowLevelDocumentModifier(ld);
            await modify(ld, modifier);
            await modifier.WriteModificationTrailer(stream);
            
            return await RandomAccessFileParser.Parse(stream.CreateReader());
        }
        
        [Fact]
        public async Task ParseModification()
        {
            var ld2 = await CompositeDocument(creator =>
            {
                var e1 = creator.Add(PdfBoolean.True);
                creator.AddToTrailerDictionary(KnownNames.Root, e1);
                creator.Add(new PdfInteger(2));
            }, async (ld, modifier) =>
            {
                Assert.Equal("true", (await ld.TrailerDictionary[KnownNames.Root]).ToString());
                Assert.Equal("2", (await ld.Objects[(2, 0)].Target.DirectValue()).ToString());
                modifier.AssignValueToReference((PdfIndirectReference) ld.TrailerDictionary.RawItems[KnownNames.Root],
                    PdfBoolean.False);
            });

            Assert.Equal("false", (await ld2.TrailerDictionary[KnownNames.Root]).ToString());
            Assert.Equal("2", (await ld2.Objects[(2,0)].Target.DirectValue()).ToString());

        }
        [Fact]
        public async Task DeleteItem()
        {
            var ld2 = await CompositeDocument(creator =>
            {
                var e1 = creator.Add(PdfBoolean.True);
                creator.AddToTrailerDictionary(KnownNames.Root, e1);
                creator.Add(new PdfInteger(200));
            }, async (ld, modifier) =>
            {
                var item = ld.Objects[(2, 0)];
                Assert.Equal("200", (await item.Target.DirectValue()).ToString());
                modifier.DeleteObject(item);
            });

            Assert.Equal("true", (await ld2.TrailerDictionary[KnownNames.Root]).ToString());
            Assert.Equal("Free Item. Next = 0", (await ld2.Objects[(2,0)].Target.DirectValue()).ToString());
            Assert.Equal(2, ld2.FirstFreeBlock);
            
        }
    }
}