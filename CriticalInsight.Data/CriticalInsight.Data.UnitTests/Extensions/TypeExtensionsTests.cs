using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CriticalInsight.Data.UnitTests.Extensions;

[TestFixture]
public class TypeExtensionsTests
{
    [Test]
    public void GetFormattedName_HandlesGenericTypes()
    {
        var t = typeof(Dictionary<string, List<int>>);
        var name = t.GetFormattedName();

        Assert.That(name, Does.Contain("Dictionary"));
        Assert.That(name, Does.Contain("String"));
        Assert.That(name, Does.Contain("List"));
        Assert.That(name, Does.Contain("Int32"));
    }

    [Test]
    public void GetShortFormattedName_HandlesGenericTypes()
    {
        var t = typeof(Dictionary<string, List<int>>);
        var shortName = t.GetShortFormattedName();

        Assert.That(shortName, Does.Contain("Dictionary"));
        Assert.That(shortName, Does.Contain("String"));
        Assert.That(shortName, Does.Contain("List"));
        Assert.That(shortName, Does.Contain("Int32"));
        Assert.That(shortName.StartsWith("System"), Is.False);
    }
}


