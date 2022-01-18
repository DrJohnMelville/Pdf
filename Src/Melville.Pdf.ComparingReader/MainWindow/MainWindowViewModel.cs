using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using ABI.System.Collections.Generic;
using Melville.INPC;
using Melville.MVVM.Wpf.Bindings;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.RootWindows;
using Melville.MVVM.Wpf.ViewFrames;
using Melville.Parsing.Streams;
using Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.ComparingReader.REPLs;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.ComparingReader.MainWindow;

public partial class MainWindowViewModel
{
    private readonly ICommandLineSelection commandLine;
    public IList<ReferenceDocumentNode> Nodes { get; }
    [AutoNotify] private ReferenceDocumentNode? selectedNode;
    public IPasswordSource PasswordBox { get; }
    public IMultiRenderer Renderer { get; }

    public MainWindowViewModel(
        IList<ReferenceDocumentNode> nodes, ICommandLineSelection commandLine, 
        IMultiRenderer renderer, IPasswordSource passwordBox)
    {
        this.commandLine = commandLine;
        Nodes = nodes;
        Renderer = renderer;
        PasswordBox = passwordBox;
    }

    public void ShowInitial()
    {
        SelectedNode ??= SearchRecusive(Nodes, commandLine.CommandLineTag());
    }

    private async void OnSelectedNodeChanged(ReferenceDocumentNode? newValue)
    {
        if (newValue is ReferenceDocumentLeaf leaf)
        {
            Renderer.SetTarget(await leaf.GetDocument());
        }
    }

    private ReferenceDocumentLeaf? SearchRecusive(IList<ReferenceDocumentNode> nodes, string? commandLineTag)
    {
        if (commandLineTag == null) return null;
        return nodes.Select(i => SearchRecusive(i, commandLineTag)).Where(i => i != null).FirstOrDefault();
    }

    private ReferenceDocumentLeaf? SearchRecusive(ReferenceDocumentNode node, string commandLineTag)
    {
        return node switch
        {
            ReferenceDocumentLeaf leaf => leaf.ShortName.Equals(commandLineTag) ? leaf : null,
            ReferenceDocumentFolder tree => SearchRecusive(tree.Children, commandLineTag),
            _=> throw new InvalidProgramException("Unknown Tree member")
        };
    }

    public void ShowPdfRepl([FromServices] ReplViewModel model,
        [FromServices] Func<object, IRootNavigationWindow> factory) =>
        factory(model).Show();

    public async Task LoadFile([FromServices] IOpenSaveFile fileSource)
    {
        var file = fileSource.GetLoadFile(null, "PDF", "Portable Document Format(*.pdf)|*.pdf", "Load File");
        if (file is null) return;
        var src = new MultiBufferStream();
        await using (var fileStr = await file.OpenRead())
        {
            await fileStr.CopyToAsync(src);
        }
        Renderer.SetTarget(src);
    }
}

public static class MainWindowConverters
{
    public static readonly IValueConverter PasswordTypeConverter =
        LambdaConverter.Create((PasswordType pt) => (int)pt, i => (PasswordType)i);
}