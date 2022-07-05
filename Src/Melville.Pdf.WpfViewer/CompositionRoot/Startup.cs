using System;
using System.IO;
using System.Windows;
using Melville.IOC.IocContainers;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.RootWindows;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevelViewerParts.ExceptionDumpers;
using Melville.Pdf.LowLevelViewerParts.FakeFileDialog;
using Melville.Pdf.LowLevelViewerParts.PasswordDialogs.PasswordDialogs;
using Melville.Pdf.WpfViewer.Home;
using Melville.WpfAppFramework.StartupBases;

namespace Melville.Pdf.WpfViewer.CompositionRoot;

public class Startup:StartupBase
{
    [STAThread]
    public static void Main(string[] arguments)
    {
        ApplicationRootImplementation.Run(new Startup(arguments));
    }

    private Startup(string[] arguments) : base(arguments)
    {
        ExceptionDumper.RegisterExceptionDumper();
    }

    protected override void RegisterWithIocContainer(IBindableIocService service)
    {
        service.RegisterHomeViewModel<HomeViewModel>();
        service.Bind<IOpenSaveFile>().To<OpenSaveFileAdapter>();
        service.Bind<Window>().And<IRootNavigationWindow>().To<RootNavigationWindow>().AsSingleton();
        service.Bind<IPasswordSource>().To<PasswordQuery>();
        TryRegistedCommandLine(service);
    }
    
    private void TryRegistedCommandLine(IBindableIocService service)
    {
        if (CommandLineParameters.Length > 0 && File.Exists(CommandLineParameters[0]))
        {
            service.Bind<IOpenSaveFile>().ToConstant(new FakeOpenAdapter(CommandLineParameters[0]));
        }
    }

}