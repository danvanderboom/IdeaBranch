using System.Collections.Generic;
using NUnit.Framework;

namespace CriticalInsight.Data.UnitTests.Extensions;

[TestFixture]
public class ICollectionExtensionsTests
{
    [Test]
    public void IsValidIndex_Boundaries()
    {
        var list = new List<int> { 1, 2, 3 } as ICollection<int>;

        Assert.That(list.IsValidIndex(0), Is.True);
        Assert.That(list.IsValidIndex(2), Is.True);
        Assert.That(list.IsValidIndex(3), Is.False);
    }
}


