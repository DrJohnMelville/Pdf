using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Melville.INPC;
using Melville.MVVM.Wpf.Bindings;
using Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.ComparingReader.MainWindow;

public partial class MainWindowViewModel
{
    public IList<ReferenceDocumentNode> Nodes { get; }
    [AutoNotify] private ReferenceDocumentNode? selectedNode;
    public IPasswordSource PasswordBox { get; }
    public IMultiRenderer Renderer { get; }

    public MainWindowViewModel(
        IList<ReferenceDocumentNode> nodes, ICommandLineSelection commandLine, 
        IMultiRenderer renderer, IPasswordSource passwordBox)
    {
        Nodes = nodes;
        Renderer = renderer;
        PasswordBox = passwordBox;
        SelectedNode = SearchRecusive(nodes, commandLine.CommandLineTag());
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
}

public static class MainWindowConverters
{
    public static readonly IValueConverter PasswordTypeConverter =
        LambdaConverter.Create((PasswordType pt) => (int)pt, i => (PasswordType)i);
}