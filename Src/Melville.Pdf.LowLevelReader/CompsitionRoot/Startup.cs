using System;
using Melville.IOC.IocContainers;
using Melville.WpfAppFramework.StartupBases;

namespace Melville.Wpf.LowLevelReader.CompsitionRoot
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
        }
    }
}