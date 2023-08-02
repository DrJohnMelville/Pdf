using System;
using Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;
using Melville.Pdf.ReferenceDocuments.Infrastructure;
using Moq;
using Xunit;

namespace Melville.Pdf.WpfToolTests.ComparingReader.MainWindow;

public class ConstructTreeTest
{
    public ReferenceDocumentFolder sut = new ReferenceDocumentFolder("Root");

    private static ReferenceDocumentLeaf NewLeaf() => new(Mock.Of<IPdfGenerator>());
    [Fact]
    public void AddItem()
    {
        sut.AddItem(NewLeaf(), Array.Empty<string>());
        Assert.Single(sut.Children);
    }

    [Fact]
    public void PutInSubFolder()
    {
        sut.AddItem(NewLeaf(), new []{"Hello", "World"});
        Assert.Single(sut.Children);
        var p1 = (ReferenceDocumentFolder)sut.Children[0];
        Assert.Single(p1.Children);
        var p2 = (ReferenceDocumentFolder)p1.Children[0];
        Assert.Single(p2.Children);
        Assert.True(p2.Children[0] is ReferenceDocumentLeaf);
    }
    
    [Fact]
    public void PutTwoItemsInSubFolder()
    {
        sut.AddItem(NewLeaf(), new []{"Hello", "World"});
        sut.AddItem(NewLeaf(), new []{"Hello", "World"});
        Assert.Single(sut.Children);
        var p1 = (ReferenceDocumentFolder)sut.Children[0];
        Assert.Single(p1.Children);
        var p2 = (ReferenceDocumentFolder)p1.Children[0];
        Assert.Equal(2, p2.Children.Count);
        Assert.True(p2.Children[0] is ReferenceDocumentLeaf);
        Assert.True(p2.Children[1] is ReferenceDocumentLeaf);
    }

    [Fact]
    public void CollapseNames()
    {
        sut.AddItem(NewLeaf(), new []{"Hello", "World"});
        sut.AddItem(NewLeaf(), new []{"Hello", "World"});
        var p2 = sut.Collapse();
        Assert.Equal("Root.Hello.World", p2.ShortName);
        
        Assert.Equal(2, p2.Children.Count);
        Assert.True(p2.Children[0] is ReferenceDocumentLeaf);
        Assert.True(p2.Children[1] is ReferenceDocumentLeaf);
    }
}