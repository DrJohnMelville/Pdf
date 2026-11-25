using System.Collections.Generic;
using System.Linq;
using Melville.Linq;
using Melville.Pdf.ComparingReader.REPLs;
using Melville.Pdf.DataModelTests.Architecture;
using NetArchTest.Rules;

namespace Melville.Pdf.WpfToolTests.ComparingReader;

public class ComoaringReaderArchitecture
{
    private static Types AllTypes => Types.InAssembly(typeof(ReplViewModel).Assembly);

    public static IEnumerable<(string, string)> Exclusion()
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
        var td = new List<(string, string)>();

        (from i in names from j in names where i != j select (i, j))
            .ForEach(i => td.Add((i.i, i.j)));
        return td;
    }

    [Test]
    [MethodDataSource(nameof(Exclusion))]
    public void Exclusives(string reference, string definition)
    {
        AllTypes.That()
            .ResideInNamespace(reference)
            .Should()
            .NotHaveDependencyOn(definition).
            ShouldSucceed();
    }

    [Test]
    [Arguments("Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree", "Melville.Pdf.ReferenceDocuments")]
    [Arguments("Melville.Pdf.ComparingReader.Viewers.WindowsViewer", "Windows.Data.Pdf")]
    [Arguments("Melville.Pdf.ComparingReader.Viewers.SkiaViewer", "Melville.Pdf.SkiaSharp")]
    [Arguments("Melville.Pdf.ComparingReader.Viewers.SkiaViewer", "Melville.Pdf.FontLibrary")]
    [Arguments("Melville.Pdf.ComparingReader.Viewers.ExtractedImages", "Melville.Pdf.ImageExtractor")]
    [Arguments("Melville.Pdf.ComparingReader.Viewers.FormViewer", "Melville.Pdf.FormReader")]
    public void CabinExternalDependencies(string reference, string def) =>
        AllTypes
            .That()
            .DoNotResideInNamespace(reference)
            .Should()
            .NotHaveDependencyOn(def)
            .ShouldSucceed();
}