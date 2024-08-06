using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Threading.Tasks;
using System.Windows.Data;
using Melville.FileSystem;
using Melville.INPC;
using Melville.MVVM.Wpf.Bindings;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.RootWindows;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.ComparingReader.REPLs;
using Melville.Pdf.ComparingReader.Viewers.LowLevel;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.ComparingReader.MainWindow;

public partial class MainWindowViewModel
{
    public IList<ReferenceDocumentNode> Nodes { get; }
    [AutoNotify] private ReferenceDocumentNode? selectedNode;
    public IPasswordSource PasswordBox { get; }
    public IMultiRenderer Renderer { get; }

    public MainWindowViewModel(
        IList<ReferenceDocumentNode> nodes, 
        IMultiRenderer renderer, IPasswordSource passwordBox)
    {
        Nodes = nodes;
        Renderer = renderer;
        PasswordBox = passwordBox;
    }

    public async void ShowInitial([FromServices]ICommandLineSelection commandLine)
    {
        if (commandLine.CommandLineTag() is {} tag) 
            SelectedNode = SearchRecursive(Nodes, tag);
        if (commandLine.CmdLineFile() is { } file)
            await LoadFileAsync(file);
    }

    private async void OnSelectedNodeChanged(ReferenceDocumentNode? newValue)
    {
        if (newValue is ReferenceDocumentLeaf leaf)
        {
            Renderer.SetTarget(await leaf.GetDocumentAsync());
        }
    }

    private ReferenceDocumentLeaf? SearchRecursive(IList<ReferenceDocumentNode> nodes, string? commandLineTag)
    {
        if (commandLineTag == null) return null;
        return nodes.Select(i => SearchRecursive(i, commandLineTag)).FirstOrDefault(i => i != null);
    }

    private ReferenceDocumentLeaf? SearchRecursive(ReferenceDocumentNode node, string commandLineTag)
    {
        return node switch
        {
            ReferenceDocumentLeaf leaf => leaf.ShortName.Equals(commandLineTag) ? leaf : null,
            ReferenceDocumentFolder tree => SearchRecursive(tree.Children, commandLineTag),
            _=> throw new InvalidProgramException("Unknown Tree member")
        };
    }

    public async void ShowPdfRepl([FromServices] ReplViewModelFactory modelFactory,
        [FromServices] Func<object, IRootNavigationWindow> windowFactory, IReplStreamPicker? picker = null)
    {
        windowFactory(await modelFactory.CreateAsync(picker?.GetReference())).Show();
    }

    public async Task LoadFileAsync([FromServices] IOpenSaveFile fileSource)
    {
        var file = fileSource.GetLoadFile(null, "PDF", "Portable Document Format(*.pdf)|*.pdf", "Load File");
        if (file is null) return;
        await LoadFileAsync(file);
    }

    private async Task LoadFileAsync(IFile file) => Renderer.SetTarget(MultiplexSourceFactory.Create(await file.OpenRead()));
}

public static class MainWindowConverters
{
    public static readonly IValueConverter PasswordTypeConverter =
        LambdaConverter.Create((PasswordType pt) => (int)pt, i => (PasswordType)i);
}