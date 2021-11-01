using Melville.Pdf.LowLevelReader.PasswordDialogs;
using Xunit;

namespace Melville.Pdf.WpfToolTests.LowLevelReader.PasswordDialog;

public class PasswordDialogViewModelTest
{
    private readonly PasswordDialogViewModel sut = new();
    [Fact]
    public void InitiallyBothEmptyAndBothVisible()
    {
        Assert.Equal("", sut.UserPassword);
        Assert.Equal("", sut.OwnerPassword);
        Assert.True(sut.UserPasswordEnabled);
        Assert.True(sut.OwnerPasswordEnabled);
        Assert.False(sut.CanOk);
    }

    [Fact]
    public void UserInvalidatesOwner()
    {
        sut.UserPassword = "d";
        Assert.Equal("", sut.OwnerPassword);
        Assert.True(sut.UserPasswordEnabled);
        Assert.False(sut.OwnerPasswordEnabled);
        Assert.True(sut.CanOk);
            
    }
    [Fact]
    public void OwnerInvalidatesUser()
    {
        sut.OwnerPassword = "d";
        Assert.Equal("", sut.UserPassword);
        Assert.True(sut.OwnerPasswordEnabled);
        Assert.False(sut.UserPasswordEnabled);
        Assert.True(sut.CanOk);
    }
}