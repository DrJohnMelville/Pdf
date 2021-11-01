using ArchitectureAnalyzer.Models;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.Models;

public class RecognizerFactoryTest
{
    private readonly RecognizerFactory fact = new(new());
    [Fact] 
    public void RepeatGivesSameRecorgnize() => 
        Assert.Equal(fact.Create("Foo"), fact.Create("Foo"));
    [Fact] 
    public void DifferentStringsAreDifferent() => 
        Assert.NotEqual(fact.Create("Foo1"), fact.Create("Foo"));
}