using System.Collections.Generic;
using System.Linq;
using Melville.Fonts;
using Melville.Linq;
using Melville.Pdf.Model;
using NetArchTest.Rules;

namespace Melville.Pdf.DataModelTests.Architecture;

public class ModelArchitecture
{
    private static Types AllTypes => Types.InAssembly(typeof(PdfReader).Assembly);

    [Test]
    [Arguments("Melville.Pdf.Model.Renderers.Colors", "Melville.Icc")]
    [Arguments("Melville.Pdf.Model.Renderers", "Melville.Pdf.Model.DocumentRenderers")]
    public void CabinExternalDependencies(string localNamespace, string foreignNamespace) =>
        AllTypes
            .That()
            .DoNotResideInNamespace(localNamespace)
            .Should()
            .NotHaveDependencyOn(foreignNamespace)
            .ShouldSucceed();

    [Test]
    public void CabinFontsUsage() =>
        AllTypes
            .That()
            .DoNotResideInNamespace("Melville.Pdf.Model.Renderers.FontRenderings.FreeType")
            .And().DoNotImplementInterface(typeof(IGlyphTarget))
            .And().DoNotHaveName("PathDrawingAdapter")
            .Should()
            .NotHaveDependencyOn("Melville.Fonts")
            .ShouldSucceed();

    public static IEnumerable<(string, string)> Exclusion()
    {
        var names = new[]
        {
            "Melville.Pdf.Model.FontMappings", 
            "Melville.Pdf.Model.Documents",
            "Melville.Pdf.Model.Creators",
            "Melville.Pdf.Model.OptionalContent",
        };
        var td = new List<(string, string)>();

        (from i in names from j in names where i != j select (i,j))
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
            And().NotHaveDependencyOn("Melville.Pdf.Model.Renderers").
            And().NotHaveDependencyOn("Melville.Pdf.Model.DocumentRenderers").
            ShouldSucceed();
    }
}