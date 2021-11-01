using Melville.INPC;

namespace Melville.Pdf.LowLevelReader.PasswordDialogs;

public sealed partial class PasswordDialogViewModel
{
    [AutoNotify] private string userPassword = "";
    [AutoNotify] private string ownerPassword = "";
    [AutoNotify] public bool UserPasswordEnabled => OwnerPassword.Length == 0;
    [AutoNotify] public bool OwnerPasswordEnabled => UserPassword.Length == 0;
    [AutoNotify] public bool CanOk => !(UserPasswordEnabled && OwnerPasswordEnabled);
}