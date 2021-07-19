using System;
using Melville.IOC.IocContainers;
using Melville.Wpf.LowLevelReader.ViewModels;
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
            service.RegisterHomeViewModel<MainDisplay>();
        }
    }
}