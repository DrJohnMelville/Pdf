using System.Threading.Tasks;
using FluentAssertions;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.Annotations;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S12_5Annotations;

public class AnnotationTest
{
    [Fact]
    public async Task GetSimpleFormAsync()
    {
        var frm = new DictionaryBuilder().AsStream(" dSWa");
        var appearance = new DictionaryBuilder().WithItem(KnownNames.N, frm).AsDictionary();
        var annot = new Annotation(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Annot)
            .WithItem(KnownNames.Subtype, KnownNames.Watermark)
            .WithItem(KnownNames.Rect, new PdfArray(0, 0, 300, 300))
            .WithItem(KnownNames.AP, appearance)
            .AsDictionary());

        (await annot.GetVisibleFormAsync() as object).Should().Be(frm);
    }
    [Fact]
    public async Task GetFormWithAppearanceStateAsync()
    {
        var frm = new DictionaryBuilder().AsStream(" dSWa");
        var states = new DictionaryBuilder().WithItem(KnownNames.ON, frm).AsDictionary();
        var appearance = new DictionaryBuilder().WithItem(KnownNames.N, states).AsDictionary();
        var annot = new Annotation(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Annot)
            .WithItem(KnownNames.Subtype, KnownNames.Watermark)
            .WithItem(KnownNames.Rect, new PdfArray(0, 0, 300, 300))
            .WithItem(KnownNames.AP, appearance)
            .WithItem(KnownNames.AS, KnownNames.ON)
            .AsDictionary());

        (await annot.GetVisibleFormAsync() as object).Should().Be(frm);
    }
    [Fact]
    public async Task SkipIfNoAppearanceAsync()
    {
        var annot = new Annotation(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Annot)
            .WithItem(KnownNames.Subtype, KnownNames.Watermark)
            .WithItem(KnownNames.Rect, new PdfArray(0, 0, 300, 300))
            .AsDictionary());

        (await annot.GetVisibleFormAsync() as object).Should().BeNull();
    }
}