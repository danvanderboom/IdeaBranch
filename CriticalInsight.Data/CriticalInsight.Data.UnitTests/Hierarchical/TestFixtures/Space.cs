using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CriticalInsight.Data.Hierarchical;

namespace CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;

public class Space : ObservableObject
{
    private string _Name = string.Empty;
    public string Name
    {
        get => _Name;
        set => Set(nameof(Name), ref _Name, value);
    }

    private double _SquareFeet = double.NaN;
    public double SquareFeet
    {
        get => _SquareFeet;
        set => Set(nameof(SquareFeet), ref _SquareFeet, value);
    }

    public override string ToString() => $"Name = {Name}, Square Feet = {SquareFeet.ToString("f2")}";
}