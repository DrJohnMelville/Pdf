using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.RunOnWindowThreads;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevelViewerParts.PasswordDialogs.PasswordDialogs;
using Moq;
using Xunit;

namespace Melville.Pdf.WpfToolTests.LowLevelReader.PasswordDialog;

public class PasswordQueryTest
{
    private readonly Mock<IMvvmDialog> dlgMock = new();
    private readonly PasswordQuery sut;

    public PasswordQueryTest()
    {
        sut = new PasswordQuery(dlgMock.Object, new RunOnWindowThreadStub());
    }

    private class RunOnWindowThreadStub : IRunOnWindowThread
    {
        public void Run(Action action, DispatcherPriority priority = DispatcherPriority.Send) => action();
        public T Run<T>(Func<T> action, DispatcherPriority priority = DispatcherPriority.Send) => action();
    }

    [Fact]
    public async Task CancelAsync()
    {
        dlgMock.Setup(i => i.ShowModalDialog(
            It.IsAny<PasswordDialogViewModel>(),
            It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<string>())).Returns(false);

        Assert.Equal((null, PasswordType.User), await sut.GetPasswordAsync());
    }
    [Fact]
    public async Task OkUserPasswordAsync()
    {
        dlgMock.Setup(i => i.ShowModalDialog(
            It.IsAny<PasswordDialogViewModel>(),
            It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<string>())).Returns(
            (PasswordDialogViewModel vm, double width, double height, string title) =>
            {
                vm.UserPassword = "User";
                return true;
            });

        Assert.Equal(("User", PasswordType.User), await sut.GetPasswordAsync());
    }
    [Fact]
    public async Task OkOwnerPasswordAsync()
    {
        dlgMock.Setup(i => i.ShowModalDialog(
            It.IsAny<PasswordDialogViewModel>(),
            It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<string>())).Returns(
            (PasswordDialogViewModel vm, double width, double height, string title) =>
            {
                vm.OwnerPassword = "Owner";
                return true;
            });

        Assert.Equal(("Owner", PasswordType.Owner), await sut.GetPasswordAsync());
    }
}