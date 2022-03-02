using Melville.Pdf.Wpf.Controls;
using Xunit;

namespace Melville.Pdf.WpfToolTests.ComparingReader.Renderers;

public class PageSelectorViewModelTest
{
    private readonly PageSelectorViewModel sut = new(){};

    [Fact]
    public void InitialValues()
    {
        Assert.Equal(1, sut.Page);
        Assert.Equal(1, sut.MaxPage);
    }

    [Fact]
    public void SetMaxPage()
    {
        sut.MaxPage = 10;
        Assert.Equal(10, sut.MaxPage);
    }

    [Theory]
    [InlineData(5,5)]
    [InlineData(-1,1)]
    [InlineData(0,1)]
    [InlineData(1,1)]
    [InlineData(9,9)]
    [InlineData(10,10)]
    [InlineData(11,10)]
    public void TrySetValue(int attempt, int result)
    {
        sut.MaxPage = 10;
        sut.Page = attempt;
        Assert.Equal(result, sut.Page);
        
    }

    [Fact]
    public void ResettingSizeResetsPage()
    {
        sut.MaxPage = 10;
        sut.Page = 5;
        sut.MaxPage = 20;
        Assert.Equal(1, sut.Page);
    }

    [Fact]
    public void IncrementPage()
    {
        sut.MaxPage = 10;
        sut.Increment();
        Assert.Equal(2, sut.Page);
        sut.Increment();
        Assert.Equal(3, sut.Page);
    }
    [Fact]
    public void GoToEnd()
    {
        sut.MaxPage = 10;
        sut.ToEnd();
        Assert.Equal(10, sut.Page);
        sut.Increment();
        Assert.Equal(10, sut.Page);
    }

    [Fact]
    public void Decrement()
    {
        sut.MaxPage = 10;
        sut.Decrement();
        Assert.Equal(1, sut.Page);
        sut.ToEnd();
        sut.Decrement();
        Assert.Equal(9, sut.Page);
        
    }    
    [Fact]
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