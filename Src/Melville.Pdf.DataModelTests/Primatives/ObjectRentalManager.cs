using System.Linq;
using Melville.Linq;
using Melville.Parsing.ObjectRentals;
using Xunit;

namespace Melville.Pdf.DataModelTests.Primatives;

public class ObjectRentalManager
{
    private readonly ObjectPool<object> sut = new();

    [Fact]
    public void RentalCreatesObject()
    {
        var rented = sut.Rent();
        Assert.NotNull(rented);
        sut.Return(rented);
    }

    [Fact]
    public void ReuseReturnedObject()
    {
        var obj = sut.Rent();
        sut.Return(obj);
        Assert.Same(obj, sut.Rent());
        sut.Return(obj);
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
        sut.Return(obj1);
        sut.Return(obj2);
    }

    [Fact]
    public void DoNotStoreMoreThanTwentyItems()
    {
        var objs = Enumerable.Range(0, 20).Select(i => sut.Rent()).ToArray();
        var l1 = sut.Rent();
        var l2 = sut.Rent();
        var l3 = sut.Rent();
        objs.ForEach(i=>sut.Return(i));
        var last = sut.Rent();
        sut.Return(last);
        sut.Return(l1);
        sut.Return(l2);
        sut.Return(l3);
        Assert.Equal(last, sut.Rent());
        sut.Return(last);
    }
}