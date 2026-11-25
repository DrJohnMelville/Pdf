using Melville.Pdf.DataModelTests.Asserts;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.WpfToolTests.ComparingReader.Renderers;

public class PageSelectorViewModelTest
{
    private readonly PageSelectorViewModel sut = new(){};

    [Test]
    public void InitialValues()
    {
        Assert.Equal(1, sut.Page);
        Assert.Equal(1, sut.MaxPage);
    }

    [Test]
    public void SetMaxPage()
    {
        sut.MaxPage = 10;
        Assert.Equal(10, sut.MaxPage);
    }

    [Test]
    [Arguments(5,5)]
    [Arguments(-1,1)]
    [Arguments(0,1)]
    [Arguments(1,1)]
    [Arguments(9,9)]
    [Arguments(10,10)]
    [Arguments(11,10)]
    public void TrySetValue(int attempt, int result)
    {
        sut.MaxPage = 10;
        sut.Page = attempt;
        Assert.Equal(result, sut.Page);
        
    }

    [Test]
    public void ResettingSizeResetsPage()
    {
        sut.MaxPage = 10;
        sut.Page = 5;
        sut.MaxPage = 20;
        Assert.Equal(1, sut.Page);
    }

    [Test]
    public void IncrementPage()
    {
        sut.MaxPage = 10;
        sut.Increment();
        Assert.Equal(2, sut.Page);
        sut.Increment();
        Assert.Equal(3, sut.Page);
    }
    [Test]
    public void GoToEnd()
    {
        sut.MaxPage = 10;
        sut.ToEnd();
        Assert.Equal(10, sut.Page);
        sut.Increment();
        Assert.Equal(10, sut.Page);
    }

    [Test]
    public void Decrement()
    {
        sut.MaxPage = 10;
        sut.Decrement();
        Assert.Equal(1, sut.Page);
        sut.ToEnd();
        sut.Decrement();
        Assert.Equal(9, sut.Page);
        
    }    
    [Test]
    public void ToStart()
    {
        sut.MaxPage = 10;
        Assert.Equal(1, sut.Page);
        sut.ToEnd();
        sut.Decrement();
        Assert.Equal(9, sut.Page);
        sut.ToStart();
        Assert.Equal(1, sut.Page);
    }
}