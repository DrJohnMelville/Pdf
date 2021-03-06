using System.Threading.Tasks;
using Melville.Hacks.Reflection;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_11_OptionalContent;

public class OptionalContentCounterTest
{
    private readonly PdfDictionary On = new DictionaryBuilder().AsDictionary();
    private readonly PdfDictionary Off = new DictionaryBuilder().AsDictionary();
    private readonly Mock<IOptionalContentState> state = new();
    private readonly Mock<IHasPageAttributes> attrs = new();
    
    private readonly OptionalContentCounter sut;

    public OptionalContentCounterTest()
    {
        state.Setup(i => i.IsGroupVisible(It.IsAny<PdfDictionary>())).
            Returns((PdfDictionary d) => new(d == On));
        attrs.SetupGet(i => i.LowLevel).Returns(
            new DictionaryBuilder()
                .WithItem(KnownNames.Properties, new DictionaryBuilder()
                    .WithItem(KnownNames.ON, On)
                    .WithItem(KnownNames.OFF, Off)
                    .AsDictionary())
                .AsDictionary());
        sut = new OptionalContentCounter(state.Object, attrs.Object);
    }

    [Fact]
    public void DefaultCounterIsVisible()
    {
        Assert.False(sut.IsHidden);
    }

    [Fact]
    public async Task EncounterInvisibleGroupMakesInvisible()
    {
        await sut.EnterGroup(KnownNames.OC, Off);
        Assert.True(sut.IsHidden);
    }
    [Fact]
    public async Task OCPrefixRequired()
    {
        await sut.EnterGroup(KnownNames.AC, Off);
        Assert.False(sut.IsHidden);
    }
    [Fact]
    public async Task EncounterInvisibleGroupNameMakesInvisible()
    {
        await sut.EnterGroup(KnownNames.OC, KnownNames.OFF);
        Assert.True(sut.IsHidden);
    }
    [Fact]
    public async Task EncounterVisibleGroupNameLeavesVisible()
    {
        await sut.EnterGroup(KnownNames.OC, KnownNames.OFF);
        Assert.True(sut.IsHidden);
    }
    [Fact]
    public async Task PopOutOfInvisibleGroup()
    {
        await sut.EnterGroup(KnownNames.OC, Off);
        Assert.True(sut.IsHidden);
        await sut.EnterGroup(KnownNames.OC, On);
        Assert.True(sut.IsHidden);
        sut.Pop();
        Assert.True(sut.IsHidden);
        sut.Pop();
        Assert.False(sut.IsHidden);
    }

    [Fact]
    public async Task EncounterVisibleGroupStaysVisible()
    {
        await sut.EnterGroup(KnownNames.OC, On);
        Assert.False(sut.IsHidden);
    }
}