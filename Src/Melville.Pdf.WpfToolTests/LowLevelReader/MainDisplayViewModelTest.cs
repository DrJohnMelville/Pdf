using System;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.MVVM.WaitingServices;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.Pdf.LowLevelReader.MainDisplay;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;
using Melville.TestHelpers.InpcTesting;
using Moq;
using Xunit;
using DocumentPart = Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.DocumentPart;

namespace Melville.Pdf.WpfToolTests.LowLevelReader;

public class MainDisplayViewModelTest
{
    private readonly Mock<IOpenSaveFile> dlg = new();
    private readonly Mock<IPartParser> parser = new();
    private readonly Mock<IFile> file = new();
    private readonly Mock<ICloseApp> closer = new();
    private readonly Mock<IWaitingService> waiting = new();
        
    private readonly MainDisplayViewModel sut;

    public MainDisplayViewModelTest()
    {
        sut = new(new LowLevelViewModel(parser.Object));
    }

    [Fact]
    public void RootTest() => sut.Model.AssertProperty(i=>i.Root, new DocumentPart[1]);
    [Fact]
    public void SelectedTest() => sut.Model.AssertProperty(i=>i.Selected, new DocumentPart("s"));

    [Fact]
    public async Task ShowFileDisplaySucceed()
    {
        dlg.Setup(i => i.GetLoadFile(null, "pdf", "Portable Document Format (*.pdf)|*.pdf", "File to open"))
            .Returns(file.Object);
        await sut.OpenFile(dlg.Object, closer.Object, waiting.Object);
        closer.VerifyNoOtherCalls();
        waiting.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task ShowFileDisplayFail()
    {
        dlg.Setup(i => i.GetLoadFile(null, "pdf", "Portable Document Format (*.pdf)|*.pdf", "File to open"))
            .Returns((IFile?)null);
        await sut.OpenFile(dlg.Object, closer.Object, waiting.Object);
        closer.Verify(i=>i.Close());
        parser.VerifyNoOtherCalls();
        waiting.VerifyNoOtherCalls();
    }
}