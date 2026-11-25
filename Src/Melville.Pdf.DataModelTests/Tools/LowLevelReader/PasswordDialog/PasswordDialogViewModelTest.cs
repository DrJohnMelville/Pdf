using Melville.Pdf.DataModelTests.Asserts;
using Melville.Pdf.LowLevelViewerParts.PasswordDialogs.PasswordDialogs;

namespace Melville.Pdf.WpfToolTests.LowLevelReader.PasswordDialog;

public class PasswordDialogViewModelTest
{
    private readonly PasswordDialogViewModel sut = new();
    [Test]
    public void InitiallyBothEmptyAndBothVisible()
    {
        Assert.Equal("", sut.UserPassword);
        Assert.Equal("", sut.OwnerPassword);
        Assert.True(sut.UserPasswordEnabled);
        Assert.True(sut.OwnerPasswordEnabled);
        Assert.False(sut.CanOk);
    }

    [Test]
    public void UserInvalidatesOwner()
    {
        sut.UserPassword = "d";
        Assert.Equal("", sut.OwnerPassword);
        Assert.True(sut.UserPasswordEnabled);
        Assert.False(sut.OwnerPasswordEnabled);
        Assert.True(sut.CanOk);
            
    }
    [Test]
    public void OwnerInvalidatesUser()
    {
        sut.OwnerPassword = "d";
        Assert.Equal("", sut.UserPassword);
        Assert.True(sut.OwnerPasswordEnabled);
        Assert.False(sut.UserPasswordEnabled);
        Assert.True(sut.CanOk);
    }
}