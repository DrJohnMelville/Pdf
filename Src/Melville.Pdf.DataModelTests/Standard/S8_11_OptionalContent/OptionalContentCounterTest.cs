using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers.OptionalContents;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_11_OptionalContent;

public class OptionalContentCounterTest
{
    private readonly PdfValueDictionary On = new ValueDictionaryBuilder().AsDictionary();
    private readonly PdfValueDictionary Off = new ValueDictionaryBuilder().AsDictionary();
    private readonly Mock<IOptionalContentState> state = new();
    private readonly Mock<IHasPageAttributes> attrs = new();

    private readonly OptionalContentCounter sut;

    public OptionalContentCounterTest()
    {
        state.Setup(i => i.IsGroupVisibleAsync(It.IsAny<PdfValueDictionary>())).
            Returns((PdfValueDictionary d) => new(d == On));
        attrs.SetupGet(i => i.LowLevel).Returns(
            new ValueDictionaryBuilder()
                .WithItem(KnownNames.PropertiesTName, new ValueDictionaryBuilder()
                    .WithItem(KnownNames.ONTName, On)
                    .WithItem(KnownNames.OFFTName, Off)
                    .AsDictionary())
                .AsDictionary());
        sut = new OptionalContentCounter(state.Object);
    }

    [Fact]
    public void DefaultCounterIsVisible()
    {
        Assert.False(sut.IsHidden);
    }

    [Fact]
    public async Task EncounterInvisibleGroupMakesInvisibleAsync()
    {
        await sut.EnterGroupAsync(KnownNames.OCTName, Off);
        Assert.True(sut.IsHidden);
    }
    [Fact]
    public async Task OCPrefixRequiredAsync()
    {
        await sut.EnterGroupAsync(KnownNames.ACTName, Off);
        Assert.False(sut.IsHidden);
    }
    [Fact]
    public async Task EncounterInvisibleGroupNameMakesInvisibleAsync()
    {
        await sut.EnterGroupAsync(KnownNames.OCTName, KnownNames.OFFTName, attrs.Object);
        Assert.True(sut.IsHidden);
    }
    [Fact]
    public async Task EncounterVisibleGroupNameLeavesVisibleAsync()
    {
        await sut.EnterGroupAsync(KnownNames.OCTName, KnownNames.OFFTName, attrs.Object);
        Assert.True(sut.IsHidden);
    }
    [Fact]
    public async Task PopOutOfInvisibleGroupAsync()
    {
        await sut.EnterGroupAsync(KnownNames.OCTName, Off);
        Assert.True(sut.IsHidden);
        await sut.EnterGroupAsync(KnownNames.OCTName, On);
        Assert.True(sut.IsHidden);
        sut.PopContentGroup();
        Assert.True(sut.IsHidden);
        sut.PopContentGroup();
        Assert.False(sut.IsHidden);
    }

    [Fact]
    public async Task EncounterVisibleGroupStaysVisibleAsync()
    {
        await sut.EnterGroupAsync(KnownNames.OCTName, On);
        Assert.False(sut.IsHidden);
    }
}