using System;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.MVVM.WaitingServices;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.Pdf.LowLevelReader.DocumentParts;
using Melville.Pdf.LowLevelReader.MainDisplay;
using Melville.TestHelpers.InpcTesting;
using Moq;
using Xunit;

namespace Melville.Pdf.WpfToolTests.LowLevelReader;

public class MainDisplayViewModelTest
{
    private readonly Mock<IOpenSaveFile> dlg = new();
    private readonly Mock<IPartParser> parser = new();
    private readonly Mock<IFile> file = new();
    private readonly Mock<ICloseApp> closer = new();
    private readonly Mock<IWaitingService> waiting = new();
        
    private readonly MainDisplayViewModel sut = new();

    [Fact]
    public void RootTest() => sut.AssertProperty(i=>i.Root, new DocumentPart[1]);
    [Fact]
    public void SelectedTest() => sut.AssertProperty(i=>i.Selected, new DocumentPart("s"));

    [Fact]
    public async Task ShowFileDisplaySucceed()
    {
        var result = new DocumentPart[1];
        dlg.Setup(i => i.GetLoadFile(null, "pdf", "Portable Document Format (*.pdf)|*.pdf", "File to open"))
            .Returns(file.Object);
        parser.Setup(i => i.ParseAsync(file.Object, waiting.Object)).ReturnsAsync(result);
        await sut.OpenFile(dlg.Object, parser.Object, closer.Object, waiting.Object);
        Assert.Equal(result, sut.Root);
        closer.VerifyNoOtherCalls();
        waiting.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task ShowFileDisplayFail()
    {
        dlg.Setup(i => i.GetLoadFile(null, "pdf", "Portable Document Format (*.pdf)|*.pdf", "File to open"))
            .Returns((IFile?)null);
        await sut.OpenFile(dlg.Object, parser.Object, closer.Object, waiting.Object);
        closer.Verify(i=>i.Close());
        parser.VerifyNoOtherCalls();
        waiting.VerifyNoOtherCalls();
    }
}