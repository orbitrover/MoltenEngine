﻿using System.Reflection;

namespace Molten;

public static class ReflectionHelper
{
    /// <summary>
    /// An empty <see cref="object"/> array. Useful when calling parameterless constructors.
    /// </summary>
    public static readonly object[] EmptyObjectArray = new object[0];

    /// <summary>Finds all types that derive from the provided class type.</summary>
    /// <typeparam name="T">The base type of other classes to search for.</typeparam>
    /// <returns></returns>
    public static List<Type> FindType<T>(bool includeAbstract = false)
    {
        Type bType = typeof(T);
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        List<Type> result = new List<Type>();
        foreach (Assembly assembly in assemblies)
        {
            IEnumerable<Type> types = assembly.DefinedTypes.Where(t => t.IsSubclassOf(bType) && (includeAbstract || !t.IsAbstract));
            result.AddRange(types);
        }

        return result;
    }

    /// <summary>Finds all types that derive from the provided class type.</summary>
    /// <typeparam name="T">The base type of other types to find.</typeparam>
    /// <returns></returns>
    public static IEnumerable<Type> FindTypeInParentAssembly<T>(bool includeAbstract = false)
    {
        Type bType = typeof(T);
        return FindType<T>(bType.Assembly, includeAbstract);
    }

    /// <summary>
    /// Finds all types that derive from <typeparamref name="T"/> within the given <see cref="Assembly"/>.
    /// </summary>
    /// <typeparam name="T">The base-type of other types to find.</typeparam>
    /// <param name="assembly">The assembly within which to find types.</param>
    /// <param name="includeAbstract">If true, abstract types will also be included in the result. Default value is false.</param>
    /// <returns></returns>
    public static IEnumerable<Type> FindType<T>(Assembly assembly, bool includeAbstract = false)
    {
        Type bType = typeof(T);

        return assembly.GetTypes().Where(t =>
        {
            return !t.IsGenericType
            && (includeAbstract || !t.IsAbstract)
            && (t.IsSubclassOf(bType) || (bType.IsAssignableFrom(t) && !t.IsInterface));
        });
    }

    public static IEnumerable<Type> FindTypesWithAttribute<T>(Assembly assembly, bool includeAbstract = false) where T : Attribute
    {
        Type bType = typeof(T);
        return assembly.GetTypes().Where(t => t.GetCustomAttribute<T>() != null && (includeAbstract || !t.IsAbstract));
    }

    /// <summary>Gets the name of a type. Includes its namespace and name only.</summary>
    /// <param name="type">The type of which to retrieve the name.</param>
    /// <returns></returns>
    public static string GetTypeName(Type type)
    {
        string typeName = "";

        typeName = type.Namespace + "." + type.Name;
        if (type.IsGenericType)
        {
            typeName += "<";
            Type[] generics = type.GetGenericArguments();
            for (int i = 0; i < generics.Length; i++)
            {
                if (i > 0)
                    typeName += ", " + GetTypeName(generics[i]);
                else
                    typeName += GetTypeName(generics[i]);
            }

            typeName += ">";
        }

        return typeName;
    }

    /// <summary>Produces a short string that can be used to retrieve the type via Type.GetType.</summary>
    /// <param name="type">The type to create the string for.</param>
    /// <returns></returns>
    public static string GetQualifiedTypeName(Type type)
    {
        string[] delims = new string[1] { ", " };

        return type + ", " + type.Assembly.FullName.Split(delims, StringSplitOptions.RemoveEmptyEntries)[0];
    }

    /// <summary>Returns an array containing all of the values in the specified enum type.</summary>
    /// <typeparam name="T">The enum type of which to return values.</typeparam>
    /// <returns></returns>
    public static T[] GetEnumValues<T>() where T : struct
    {
        Type t = typeof(T);
        if (!t.IsEnum)
            throw new Exception("The provided type was not of type Enum");

        return Enum.GetValues(t) as T[];
    }

    public static T GetLastEnumValue<T>() where T : struct, IConvertible
    {
        Type t = typeof(T);

        if (t.IsEnum == false)
            throw new InvalidOperationException("Type must be an enum");

        return Enum.GetValues(t).Cast<T>().Last();
    }

    public static T GetHighestEnumValue<T>() where T : struct, IConvertible
    {
        Type t = typeof(T);

        if (t.IsEnum == false)
            throw new InvalidOperationException("Type must be an enum");

        return Enum.GetValues(t).Cast<T>().Max();
    }
}
