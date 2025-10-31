using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class TypeExtensions
{
    public static string GetFormattedName(this Type type, bool assemblyQualification = false)
    {
        if (!type.IsGenericType)
            return assemblyQualification
                ? (type.AssemblyQualifiedName ?? type.FullName ?? type.Name)
                : (type.FullName ?? type.Name);

        string typeBaseName = (type.Namespace != null ? type.Namespace + "." : string.Empty) + type.Name[..type.Name.IndexOf('`')];

        string FormatArg(Type t)
        {
            if (!t.IsGenericType)
                return assemblyQualification
                    ? (t.AssemblyQualifiedName ?? t.FullName ?? t.Name)
                    : (t.FullName ?? t.Name);
            return t.GetFormattedName(assemblyQualification);
        }

        var genericArguments = string.Join(", ", type.GetGenericArguments().Select(FormatArg));
        return $"{typeBaseName}<{genericArguments}>";
    }

    public static string GetShortFormattedName(this Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        string typeBaseName = type.Name[..type.Name.IndexOf('`')];

        string FormatArg(Type t)
        {
            return t.IsGenericType ? t.GetShortFormattedName() : t.Name;
        }

        var genericArguments = string.Join(", ", type.GetGenericArguments().Select(FormatArg));
        return $"{typeBaseName}<{genericArguments}>";
    }
}