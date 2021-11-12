using System.Collections.Generic;
using Melville.Pdf.ReferenceDocuments.Infrastructure;

namespace Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;

public static class ReferenceDocumentFactory
{
    public static IList<ReferenceDocumentNode> Create()
    {
        var root = new ReferenceDocumentFolder("Root");
        foreach (var generator in GeneratorFactory.AllGenerators)
        {
            var node = new ReferenceDocumentLeaf(generator);
            root.AddItem(node, (generator.GetType().FullName??"").Split('.')[..^1]);
        }
        return root.Collapse().Children;
    }
}