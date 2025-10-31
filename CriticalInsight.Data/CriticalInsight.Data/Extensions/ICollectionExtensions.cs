using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ICollectionExtensions
{
    public static bool IsValidIndex<T>(this ICollection<T> list, int index) => list.Count > index;
}