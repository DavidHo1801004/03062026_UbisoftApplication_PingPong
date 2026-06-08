using System;
using System.Linq;
using System.Reflection;

namespace Developer.Utilities
{
    /// <summary>
    /// Provides helper methods for common reflection-based queries.
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Retrieves all non-abstract subclasses of the specified base type
        /// across all assemblies loaded in the current <see cref="AppDomain"/>.
        /// </summary>
        /// <typeparam name="T"> The base type whose non-abstract subclasses are queried. </typeparam>
        /// <returns>
        /// An array of <see cref="Type"/> instances representing all non-abstract subclasses
        /// assignable to <typeparamref name="T"/>.
        /// </returns>
        /// <remarks>
        /// This method safely handles partially loaded assemblies by recovering types
        /// from <see cref="ReflectionTypeLoadException"/>.
        /// </remarks>
        public static Type[] GetNonAbstractSubclassesOf<T>()
        {
            Type baseType = typeof(T);

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany((assembly) =>
                {
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        types = ex.Types.Where(t => t != null).ToArray();
                    }
                    return types;
                })
                .Where((t) =>
                    t != null &&
                    baseType.IsAssignableFrom(t) &&
                    t.IsClass &&
                    !t.IsAbstract)
                .ToArray();
        }
    }
}
