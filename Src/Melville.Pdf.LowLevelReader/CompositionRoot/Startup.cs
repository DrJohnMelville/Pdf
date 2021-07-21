using System;
using System.Windows;
using Melville.IOC.IocContainers;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.Pdf.LowLevelReader.MainDisplay;
using Melville.Pdf.LowLevelReader.Services;
using Melville.WpfAppFramework.StartupBases;

namespace Melville.Pdf.LowLevelReader.CompositionRoot
{
    public class Startup: StartupBase
    {
        [STAThread]
        public static void Main(string[] arguments)
        {
            ApplicationRootImplementation.Run(new Startup());
        }

        protected override void RegisterWithIocContainer(IBindableIocService service)
        {
            service.AddLogging();
            service.RegisterHomeViewModel<MainDisplayViewModel>();
            RegisterMainWindow(service);
        }

        private static void RegisterMainWindow(IBindableIocService service)
        {
            service.Bind<Application>().ToSelf()
                .FixResult(i=>i.ShutdownMode=ShutdownMode.OnMainWindowClose)
                .AsSingleton();
            service.Bind<ICloseApp>().To<CloseWpfApp>();
            service.Bind<IOpenSaveFile>().To<OpenSaveFileAdapter>();
        }
    }
}