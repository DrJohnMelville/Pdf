using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public class S7_3_7_DictionaryOperations
    {
        private async Task<PdfDictionary> CreateDict(string definition) =>
            (PdfDictionary) (await definition.ParseToPdfAsync());


        private Task<PdfDictionary> IndirectTestDict => CreateDict("<</HEIGHT true /WIDTH 1 0 R /AC 1 0 obj false endobj>>");

        [Fact]
        public async Task CountIsAccurate()
        {
            var d = await IndirectTestDict;
            Assert.Equal(3, d.Count);
        }
        [Fact]
        public async Task ContainsKeyWorks()
        {
            var d = await IndirectTestDict;

            Assert.True(d.ContainsKey(KnownNames.Height));
            Assert.False(d.ContainsKey(KnownNames.FormType));
        }
        [Fact]
        public async Task EnumerateHandlesIndirectReferences()
        {
            var d = await IndirectTestDict;

            Assert.Equal(new []{PdfBoolean.True, PdfBoolean.False, PdfBoolean.False}, 
                ((IEnumerable)d).OfType<KeyValuePair<PdfName,PdfObject>>().Select(i=>i.Value));
            Assert.Equal(new []{KnownNames.Height, KnownNames.Width, KnownNames.AC},
                ((IEnumerable)d).OfType<KeyValuePair<PdfName,PdfObject>>().Select(i=>i.Key));
            
        }
        [Fact]
        public async Task EnumerateKeys()
        {
            var d = await IndirectTestDict;
            Assert.Equal(new []{KnownNames.Height, KnownNames.Width, KnownNames.AC},d.Keys);
            
        }
        [Fact]
        public async Task IndexerHandlesIndirection()
        {
            var d = await IndirectTestDict;
            Assert.Equal(PdfBoolean.False, d[KnownNames.Width]);
        }
        [Fact]
        public async Task TryGetValueSucceed()
        {
            var d = await IndirectTestDict;
            AAssert.True(d.TryGetValue(KnownNames.Width, out var returned));
            Assert.Equal(PdfBoolean.False,returned);
        }
        [Fact]
        public async Task TryGetValueFails()
        {
            var d = await IndirectTestDict;
            AAssert.False(d.TryGetValue(KnownNames.Activation, out var returned));
            Assert.Null(returned);
        }
        [Fact]
        public async Task EnumerateValuesHandlesIndirectReferences()
        {
            var d = await IndirectTestDict;

            Assert.Equal(new []{PdfBoolean.True, PdfBoolean.False, PdfBoolean.False}, d.Values);
            
        }

        [Fact]
        public async Task DictionaryWithoutType()
        {
            Assert.Null((await IndirectTestDict).Type);
            Assert.Null((await IndirectTestDict).SubType);
        }
        [Fact]
        public async Task DictionaryWithType()
        {
            Assert.Equal(KnownNames.Image, (await CreateDict("<< /TYPE /IMAGE >>")).Type);
        }
        [Fact]
        public async Task DictionaryWithSubType()
        {
            Assert.Equal(KnownNames.Image, (await CreateDict("<< /SUBTYPE /IMAGE >>")).SubType);
        }
        [Fact]
        public async Task DictionaryWithAbbreviatedType()
        {
            Assert.Equal(KnownNames.Image, (await CreateDict("<< /S /IMAGE >>")).SubType);
        }
    }
}