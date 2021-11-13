using System;
using System.Collections.Generic;
using System.Windows;
using Melville.IOC.IocContainers;
using Melville.MVVM.Wpf.RootWindows;
using Melville.Pdf.ComparingReader.MainWindow;
using Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.ComparingReader.Viewers.LowLevel;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.WpfViewerParts.PasswordDialogs.PasswordDialogs;
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
            service.AddLogging();
            service.RegisterHomeViewModel<MainWindowViewModel>();
            RegisterRenderers(service);
            RegisterRootWindows(service);
        }

        private static void RegisterRenderers(IBindableIocService service)
        {
            service.Bind<IRenderer>().To<LowLevelRenderer>();
            service.Bind<IMultiRenderer>().To<TabMultiRendererViewModel>();
        }

        private void RegisterRootWindows(IBindableIocService service)
        {
            service.Bind<IList<ReferenceDocumentNode>>().ToMethod(ReferenceDocumentFactory.Create);
            service.Bind<ICommandLineSelection>().ToMethod(() => new CommandLineSelection(this.CommandLineParameters));
            service.Bind<IPasswordSource>().To<PasswordQuery>();
            service.Bind<Window>().And<IRootNavigationWindow>().To<RootNavigationWindow>().AsSingleton();
        }
    }
}