﻿using System;
using System.IO;
using System.Windows;
using Melville.IOC.IocContainers;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.RootWindows;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevelViewer.MainDisplay;
using Melville.Pdf.LowLevelViewer.Services;
using Melville.Pdf.LowLevelViewerParts.FakeFileDialog;
using Melville.Pdf.LowLevelViewerParts.PasswordDialogs.PasswordDialogs;
using Melville.WpfAppFramework.StartupBases;
namespace Melville.Pdf.LowLevelViewer.CompositionRoot;

public class Startup: StartupBase
{
    [STAThread]
    public static void Main(string[] arguments)
    {
         ApplicationRootImplementation.Run(new Startup(arguments));
    }

    private Startup(string[] arguments) : base(arguments)
    {
    }

    protected override void RegisterWithIocContainer(IBindableIocService service)
    {
        service.RegisterHomeViewModel<MainDisplayViewModel>();
        RegisterMainWindow(service);
        TryRegistedCommandLine(service);
    }

    private void TryRegistedCommandLine(IBindableIocService service)
    {
        if (CommandLineParameters.Length > 0 && File.Exists(CommandLineParameters[0]))
        {
            service.Bind<IOpenSaveFile>().ToConstant(new FakeOpenAdapter(CommandLineParameters[0]));
        }
    }

    private static void RegisterMainWindow(IBindableIocService service)
    {
        service.Bind<Application>().ToSelf()
            .FixResult(i=>i.ShutdownMode=ShutdownMode.OnMainWindowClose)
            .AsSingleton();
        service.Bind<IRootNavigationWindow>()
            .And<Window>()
            .To<RootNavigationWindow>()
            .AsSingleton();
        service.Bind<IPasswordSource>().To<PasswordQuery>();
        service.Bind<ICloseApp>().To<CloseWpfApp>();
        service.Bind<IOpenSaveFile>().To<OpenSaveFileAdapter>();
    }
}