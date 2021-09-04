using System.Threading.Tasks;
using Melville.Hacks;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevelReader.PasswordDialogs;
using Moq;
using Xunit;

namespace Melville.Pdf.WpfToolTests.LowLevelReader.PasswordDialog
{
    public class PasswordQueryTest
    {
        private readonly Mock<IMvvmDialog> dlgMock = new();
        private readonly Mock<IStaWorker> staWorkerMock = new();
        private readonly PasswordQuery sut;

        public PasswordQueryTest()
        {
            sut = new PasswordQuery(dlgMock.Object, staWorkerMock.Object);
        }

        [Fact]
        public async Task Cancel()
        {
            dlgMock.Setup(i => i.ShowModalDialog(
                It.IsAny<PasswordDialogViewModel>(),
                It.IsAny<double>(), It.IsAny<double>(),
                It.IsAny<string>())).Returns(false);

            Assert.Equal((null, PasswordType.User), await sut.GetPassword());
        }
        [Fact]
        public async Task OkUserPassword()
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

            Assert.Equal(("User", PasswordType.User), await sut.GetPassword());
        }
        [Fact]
        public async Task OkOwnerPassword()
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

            Assert.Equal(("Owner", PasswordType.Owner), await sut.GetPassword());
        }
    }
}