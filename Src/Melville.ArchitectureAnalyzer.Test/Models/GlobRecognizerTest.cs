using ArchitectureAnalyzer.Models;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.Models;

public class GlobRecognizerTest
{
    [Theory]
    [InlineData("Foo", "Foo", "Bar")]
    [InlineData("Foo*", "Foo", "Bar")]
    [InlineData("Foo?", "Foos", "Foo")]
    [InlineData("Foo*", "Foo", "Foart")]
    [InlineData("Foo+", "Fooslkdgashkd;hl", "Foo")]
    [InlineData("Foo*", "Fooggle", "Foart")]
    [InlineData("Foo", "Foo", "Foopostfix")]
    [InlineData("Foo", "Foo", "prefixFoo")]
    [InlineData("Foo*", "Foo<int>", "prefixFoo")]
    public void TryGlob(string source, string succeeds, string fails)
    {
        var sut = new GlobRecognizer(source);
        Assert.True(sut.Matches(succeeds));
        Assert.False(sut.Matches(fails));
            
    }
}