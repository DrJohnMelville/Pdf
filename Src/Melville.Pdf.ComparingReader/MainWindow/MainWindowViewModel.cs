

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.WpfViewerParts.LowLevelViewer;

namespace Melville.Pdf.ComparingReader.MainWindow
{
    public partial class MainWindowViewModel
    {
        public IList<ReferenceDocumentNode> Nodes { get; }
        [AutoNotify] private ReferenceDocumentNode? selectedNode;
        public LowLevelViewModel LowLevelModel { get; }

        private async void OnSelectedNodeChanged(ReferenceDocumentNode? newValue)
        {
            if (newValue is ReferenceDocumentLeaf leaf)
            {
                var stream = new MultiBufferStream();
                await leaf.Document.WritePdfAsync(stream);
                LowLevelModel.SetStream(stream.CreateReader());
            }
        }

        public MainWindowViewModel(
            IList<ReferenceDocumentNode> nodes, ICommandLineSelection commandLine, LowLevelViewModel lowLevelModel)
        {
            Nodes = nodes;
            LowLevelModel = lowLevelModel;
            SelectedNode = SearchRecusive(nodes, commandLine.CommandLineTag());
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
                ReferenceDocumentFolder tree => SearchRecusive(tree.Children, commandLineTag)
            };
        }
    }
    
}