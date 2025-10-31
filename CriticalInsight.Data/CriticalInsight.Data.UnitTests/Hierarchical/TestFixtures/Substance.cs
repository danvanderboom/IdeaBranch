using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;

public class Substance : ObservableObject
{
    private string _Name = string.Empty;
    public string Name
    {
        get => _Name;
        set => Set(nameof(Name), ref _Name, value);
    }

    private string _Description = string.Empty;
    public string Description
    {
        get => _Description;
        set => Set(nameof(Description), ref _Description, value);
    }

    public override string ToString() => $"Name = {Name}";
}