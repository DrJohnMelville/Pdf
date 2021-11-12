using System;
using System.Collections.Generic;
using Melville.IOC.IocContainers;
using Melville.Pdf.ComparingReader.MainWindow;
using Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;
using Melville.WpfAppFramework.StartupBases;

namespace Melville.Pdf.ComparingReader.CompositionRoot
{
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
            service.RegisterHomeViewModel<MainWindowViewModel>();
            service.Bind<IList<ReferenceDocumentNode>>().ToMethod(ReferenceDocumentFactory.Create);
            service.Bind<ICommandLineSelection>().ToMethod(() => new CommandLineSelection(this.CommandLineParameters));
        }
    }
}