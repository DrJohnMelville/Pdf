using System;
using JetBrains.dotMemoryUnit;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing.NameParsing;
using Xunit;
using Xunit.Abstractions;

namespace Melville.Pdf.DataModelTests.Parser
{
    [DotMemoryUnit(CollectAllocations = true,
        SavingStrategy = SavingStrategy.OnAnyFail,
        WorkspaceNumberLimit = 5,
        FailIfRunWithoutSupport = false,
        Directory = @"C:\Users\jmelv\Documents\Scratch\Temp")]
    public class ParserMemoryAllocation
    {
        public ParserMemoryAllocation(ITestOutputHelper helper)
        {
            GC.KeepAlive(KnownNames.A);
            DotMemoryUnitTestOutput.SetOutputMethod(helper.WriteLine);
            
        }
        [Theory]
        [InlineData("/WIDTH %", 0)]
        [InlineData("/WIDTh %", 1)]
        //eventually expand to more types
        public void ParsingNamesKnownDoesNotAllocate(string text, int newObjs)
        {
            var src = text.AsSequenceReader();
            var cp1 = dotMemory.Check();
            Assert.True(NameParser.TryParse(ref src, out _));
            dotMemory.Check(i =>
            {
                Assert.Equal(newObjs, i.GetDifference(cp1).GetNewObjects(i=>i.Assembly.Like("Melville*"))
                    .ObjectsCount);
            });
        }

    }
}