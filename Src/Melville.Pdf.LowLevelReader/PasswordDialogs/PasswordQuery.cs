using System.Threading.Tasks;
using Melville.Hacks;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.RunOnWindowThreads;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevelReader.PasswordDialogs
{
    public class PasswordQuery:IPasswordSource
    {
        private readonly IMvvmDialog dialogLauncher;
        private readonly IRunOnWindowThread worker;
        public PasswordQuery(IMvvmDialog dialogLauncher, IRunOnWindowThread worker)
        {
            this.dialogLauncher = dialogLauncher;
            this.worker = worker;
        }

        public ValueTask<(string?, PasswordType)> GetPassword()
        {
            var vm = new PasswordDialogViewModel();
            var dialogResult = worker.Run(()=> dialogLauncher.ShowModalDialog(
                vm,300,150,"Please enter password for encrypted file"));
            return new(ComputeReturn(dialogResult, vm));
        }

        private static (string?, PasswordType) ComputeReturn(bool? dialogResult, PasswordDialogViewModel vm) =>
            (dialogResult, vm.UserPassword, vm.OwnerPassword) switch
            {
                (null or false, _, _) => (null, PasswordType.User),
                (true, "", { Length: > 0 } owner) => (owner, PasswordType.Owner),
                (true, var user, _) => (user, PasswordType.User),
            };
    }
}