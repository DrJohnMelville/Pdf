using System.Linq;
using Melville.Linq;
using Melville.Pdf.ComparingReader.REPLs;
using Melville.Pdf.DataModelTests.Architecture;
using NetArchTest.Rules;
using Xunit;

namespace Melville.Pdf.WpfToolTests.ComparingReader;

public class ComoaringReaderArchitecture
{
    private static Types AllTypes => Types.InAssembly(typeof(ReplViewModel).Assembly);

    public static TheoryData<string, string> Exclusion()
    {
        var names = new[]
        {
            "Melville.Pdf.ComparingReader.Viewers.WindowsViewer",
            "Melville.Pdf.ComparingReader.Viewers.SkiaViewer",
            "Melville.Pdf.ComparingReader.Viewers.FormViewer",
            "Melville.Pdf.ComparingReader.Viewers.ExtractedImages",
            "Melville.Pdf.ComparingReader.Viewers.ExtractedTexts",
            "Melville.Pdf.ComparingReader.Viewers.LowLevel",
            "Melville.Pdf.ComparingReader.Viewers.SystemViewers",
            "Melville.Pdf.ComparingReader.Viewers.WpfViewers",
        };
        var td = new TheoryData<string, string>();

        (from i in names from j in names where i != j select (i, j))
            .ForEach(i => td.Add(i.i, i.j));
        return td;
    }

    [Theory]
    [MemberData(nameof(Exclusion))]
    public void Exclusives(string reference, string definition)
    {
        AllTypes.That()
            .ResideInNamespace(reference)
            .Should()
            .NotHaveDependencyOn(definition).
            ShouldSucceed();
    }

    [Theory]
    [InlineData("Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree", "Melville.Pdf.ReferenceDocuments")]
    [InlineData("Melville.Pdf.ComparingReader.Viewers.WindowsViewer", "Windows.Data.Pdf")]
    [InlineData("Melville.Pdf.ComparingReader.Viewers.SkiaViewer", "Melville.Pdf.SkiaSharp")]
    [InlineData("Melville.Pdf.ComparingReader.Viewers.SkiaViewer", "Melville.Pdf.FontLibrary")]
    [InlineData("Melville.Pdf.ComparingReader.Viewers.ExtractedImages", "Melville.Pdf.ImageExtractor")]
    [InlineData("Melville.Pdf.ComparingReader.Viewers.FormViewer", "Melville.Pdf.FormReader")]
    public void CabinExternalDependencies(string reference, string def) =>
        AllTypes
            .That()
            .DoNotResideInNamespace(reference)
            .Should()
            .NotHaveDependencyOn(def)
            .ShouldSucceed();
}