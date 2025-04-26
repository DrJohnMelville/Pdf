using Melville.Pdf.LowLevel.Filters.JpegFilter;
using NetArchTest.Rules;
using Xunit;

namespace Melville.Pdf.DataModelTests.Architecture;

public class LowLevelArchitecture
{
    private static Types AllTypes => Types.InAssembly(typeof(DctCodec).Assembly);

    [Theory]
    [InlineData("Melville.Pdf.LowLevel.Filters.JpegFilter", "Melville.JpegLibrary")]
    [InlineData("Melville.Pdf.LowLevel.Filters.Jbig2Filter", "Melville.JBig2")]
    [InlineData("Melville.Pdf.LowLevel.Filters.JpxDecodeFilters", "CoreJ2K")]
    [InlineData("Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters", "Melville.CCITT")]
    public void CabinExternalDependencies(string localNamespace, string foreignNamespace)
    {
        AllTypes
            .That()
            .DoNotResideInNamespace(localNamespace)
            .Should()
            .NotHaveDependencyOn(foreignNamespace)
            .ShouldSucceed();
    }

    [Theory]
    [InlineData("Melville.Pdf.LowLevel.Model", "Melville.Pdf.LowLevel.Parsing")]
    [InlineData("Melville.Pdf.LowLevel.Visitors", "Melville.Pdf.LowLevel.Parsing")]
    [InlineData("Melville.Pdf.LowLevel.Filters", "Melville.Pdf.LowLevel.Parsing")]
    [InlineData("Melville.Pdf.LowLevel.Model", "Melville.Pdf.LowLevel.Writers")]
    [InlineData("Melville.Pdf.LowLevel.Visitors", "Melville.Pdf.LowLevel.Writers")]
    [InlineData("Melville.Pdf.LowLevel.Filters", "Melville.Pdf.LowLevel.Writers")]
    public void BannedDependencies(string reference, string decl)
    {
        AllTypes
            .That()
            .ResideInNamespace(reference)
            .Should()
            .NotHaveDependencyOn(decl)
            .ShouldSucceed();
    }

    [Fact]
    public void WritersShouldNotReferenceReaders()
    {
        AllTypes
            .That()
            .ResideInNamespace("Melville.Pdf.LowLevel.Writers")
            .And().DoNotHaveName("ContentStreamPrettyPrinter")
            .Should()
            .NotHaveDependencyOn("Melville.Pdf.LowLevel.Parsing")
            .ShouldSucceed();
    }

    [Fact]
    public void ReadersShouldNotReferenceWriters()
    {
        AllTypes
            .That()
            .ResideInNamespace("Melville.Pdf.LowLevel.Parsing")
            .Should()
            .NotHaveDependencyOn("Melville.Pdf.LowLevel.Writers")
            .ShouldSucceed();
    }
}