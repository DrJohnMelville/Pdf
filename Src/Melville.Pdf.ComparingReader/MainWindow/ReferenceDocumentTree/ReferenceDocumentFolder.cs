using System;
using System.Collections.Generic;
using System.Linq;

namespace Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;

public class ReferenceDocumentFolder : ReferenceDocumentNode
{
    public override string ShortName { get; }
    public List<ReferenceDocumentNode> Children { get; }

    private ReferenceDocumentFolder(string shortName, List<ReferenceDocumentNode> children)
    {
        ShortName = shortName;
        Children = children;
    }
    public ReferenceDocumentFolder(string shortName): this(shortName, new())
    {
    }

    public void AddItem(ReferenceDocumentLeaf item, in Span<string> names)
    {
        if (names.Length == 0) 
            Children.Add(item);
        else
            PlaceInSubFolder(item, names);
    }

    private void PlaceInSubFolder(ReferenceDocumentLeaf item, Span<string> names) => 
        GetSubFolder(names[0]).AddItem(item, names[1..]);

    private  ReferenceDocumentFolder GetSubFolder(string name) => 
        SubfolderWithName(name) is { } ret ? ret : CreateNewChild(name);

    private ReferenceDocumentFolder? SubfolderWithName(string name) => 
        Children
            .OfType<ReferenceDocumentFolder>()
            .FirstOrDefault(i => i.ShortName.Equals(name) );

    private ReferenceDocumentFolder CreateNewChild(string name)
    {
        var child = new ReferenceDocumentFolder(name);
        Children.Add(child);
        return child;
    }

    public ReferenceDocumentFolder Collapse()
    {
        if (Children.Count == 1 && Children[0] is ReferenceDocumentFolder child)
        {
            return MergeParentIntoSingleChild(child);
        }
        
        CollapseSubTrees();
        return this;
    }

    private ReferenceDocumentFolder MergeParentIntoSingleChild(ReferenceDocumentFolder child) =>
        new ReferenceDocumentFolder($"{ShortName}.{child.ShortName}",
            child.Children).Collapse();

    private void CollapseSubTrees()
    {
        for (int i = 0; i < Children.Count; i++)
        {
            if (Children[i] is not ReferenceDocumentFolder innerChild) continue;
            Children[i] = innerChild.Collapse();
        }
    }
}