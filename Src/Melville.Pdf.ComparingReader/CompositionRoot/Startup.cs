using System;
using System.Collections.Generic;
using System.Windows;
using Melville.IOC.IocContainers;
using Melville.MVVM.Wpf.RootWindows;
using Melville.Pdf.ComparingReader.MainWindow;
using Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.ComparingReader.Renderers.PageFlippers;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;
using Melville.Pdf.ComparingReader.Viewers.LowLevel;
using Melville.Pdf.ComparingReader.Viewers.SkiaViewer;
using Melville.Pdf.ComparingReader.Viewers.SystemViewers;
using Melville.Pdf.ComparingReader.Viewers.WindowsViewer;
using Melville.Pdf.ComparingReader.Viewers.WpfViewers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
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
            RegisterRepl(service);
        }

        private void RegisterRepl(IBindableIocService service)
        {
            service.Bind<Func<object, IRootNavigationWindow>>().ToConstant(o =>
            {
                var navWindow = new NavigationWindow();
                navWindow.NavigateTo(o);
                return new RootNavigationWindow(navWindow);
            });
            
        }

        private static void RegisterRenderers(IBindableIocService service)
        {
            var selector = new PageSelectorViewModel();
            service.Bind<IPageSelector>().ToConstant(selector);
            BindImageRenderer(service, "Reference", new WindowsImageRenderer(selector));
            BindImageRenderer(service, "WPF", new WpfDrawingGroupRenderer());
            BindImageRenderer(service, "Skia", new SkiaRenderer());
            service.Bind<IRenderer>().To<LowLevelRenderer>();
            service.Bind<IRenderer>().To<SystemRenderViewModel>();
            service.Bind<IMultiRenderer>().To<TabMultiRendererViewModel>().AsSingleton();
        }

        private static void BindImageRenderer(
            IBindableIocService service, string name, IImageRenderer renderer) =>
            service.Bind<IRenderer>().To<ImageViewerViewModel>().WithParameters(renderer, name);

        private void RegisterRootWindows(IBindableIocService service)
        {
           service.Bind<IList<ReferenceDocumentNode>>().ToMethod(ReferenceDocumentFactory.Create);
            service.Bind<ICommandLineSelection>().ToMethod(() => new CommandLineSelection(this.CommandLineParameters));
            service.Bind<IPasswordSource>().To<PasswordBox>().AsSingleton();
            service.Bind<Window>().And<IRootNavigationWindow>().To<RootNavigationWindow>().AsSingleton();
        }
    }
}