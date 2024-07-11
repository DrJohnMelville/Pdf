using Melville.Parsing.ObjectRentals;
using Xunit;

namespace Melville.Pdf.DataModelTests.Primatives;

public class ObjectRentalManager
{
    private readonly ObjectPool<object> sut = new();

    [Fact]
    public void RentalCreatesObject()
    {
        Assert.NotNull(sut.Rent());
    }

    [Fact]
    public void ReuseReturnedObject()
    {
        var obj = sut.Rent();
        sut.Return(obj);
        Assert.Same(obj, sut.Rent());
    }
    [Fact]
    public void ReturnUniqueObjects()
    {
        var obj1 = sut.Rent();
        var obj2 = sut.Rent();
        sut.Return(obj1);
        sut.Return(obj2);
        Assert.Same(obj2, sut.Rent());
        Assert.Same(obj1, sut.Rent());
    }

    [Fact]
    public void DoNotStoreMoreThanTwentyItems()
    {
        for (int i = 0; i < 20; i++)
        {
            sut.Return(new object());
        }
        var last = sut.Rent();
        sut.Return(last);
        sut.Return(new object());
        sut.Return(new object());
        sut.Return(new object());
        Assert.Equal(last, sut.Rent());
        
    }
}