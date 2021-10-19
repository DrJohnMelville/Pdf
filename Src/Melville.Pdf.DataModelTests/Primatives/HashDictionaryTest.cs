using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Primitives;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Primatives
{
    public class HashDictionaryTest
    {
        private const string ColidingName2 = "quists";
        private const string ColidingName1 = "creamwove";
        private readonly NameDictionay sut = new NameDictionay();
        [Fact]
        public void CreateName()
        {
            var name = sut.GetOrCreate("xrt23");
            Assert.Equal("/xrt23", name.ToString());
        }

        [Fact]
        public void Unique()
        {
            Assert.True(object.ReferenceEquals(sut.GetOrCreate("ikhkl"), sut.GetOrCreate("ikhkl")));
        }

        [Fact]
        public void TestColision()
        {
            // FFV-1 colision courtesy of
            // https://softwareengineering.stackexchange.com/questions/49550/which-hashing-algorithm-is-best-for-uniqueness-and-speed
            var o1 = sut.GetOrCreate(ColidingName1);
            var o2 = sut.GetOrCreate(ColidingName2);
            Assert.NotEqual(o1, o2);
            Assert.True(ReferenceEquals(o1, sut.GetOrCreate(ColidingName1)));
            Assert.True(ReferenceEquals(o2, sut.GetOrCreate(ColidingName2)));
        }

        [Fact]
        public void Synonym()
        {
            var target = sut.GetOrCreate("xxyy21");
            sut.AddSynonym(ColidingName1, target);
            Assert.Equal(target, sut.GetOrCreate(ColidingName1));
        }
    }
}