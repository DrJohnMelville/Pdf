using System;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.MVVM.WaitingServices;
using Melville.MVVM.Wpf.EventBindings.SearchTree;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.Pdf.LowLevelReader.MainDisplay;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;
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
    private readonly Mock<IVisualTreeRunner> runner = new();
        
    private readonly MainDisplayViewModel sut;

    public MainDisplayViewModelTest()
    {
        file.SetupGet(i => i.Path).Returns("c:\\ddd.pdf");
        sut = new(new LowLevelViewModel(parser.Object));
    }

    [Fact]
    public void RootTest() => ((LowLevelViewModel)sut.Model!).AssertProperty(i=>i.ParsedDoc, 
        new ParsedLowLevelDocument(new DocumentPart[1], NoPageLookup.Instance), i=>i.Root!);
    [Fact]
    public void SelectedTest() => ((LowLevelViewModel)sut.Model!).AssertProperty(i=>i.Selected, new DocumentPart("s"));

    [Fact]
    public async Task ShowFileDisplaySucceed()
    {
        dlg.Setup(i => i.GetLoadFile(null, "pdf", It.IsAny<string>(), "File to open"))
            .Returns(file.Object);
        await sut.OpenFile(dlg.Object, closer.Object, runner.Object);
        closer.VerifyNoOtherCalls();
        Assert.Single(runner.Invocations);
    }
    [Fact]
    public async Task ShowFileDisplayFail()
    {
        dlg.Setup(i => i.GetLoadFile(null, "pdf", "Portable Document Format (*.pdf)|*.pdf", "File to open"))
            .Returns((IFile?)null);
        await sut.OpenFile(dlg.Object, closer.Object, runner.Object);
        closer.Verify(i=>i.Close());
        parser.VerifyNoOtherCalls();
        runner.VerifyNoOtherCalls();
    }
}