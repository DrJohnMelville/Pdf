// Copyright (c) 2007-2016 CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

namespace CoreJ2K
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Util;

    /// <summary>
    /// Setup helper methods for initializing library.
    /// </summary>
    internal static class J2kSetup
    {
        /// <summary>
        /// Gets a single instance from the platform assembly implementing the <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">(Abstract) type for which implementation is requested.</typeparam>
        /// <returns>The single instance from the platform assembly implementing the <typeparamref name="T"/> type, 
        /// or null if no or more than one implementations are available.</returns>
        /// <remarks>It is implicitly assumed that implementation class has a public, parameter-less constructor.</remarks>
        internal static T GetSinglePlatformInstance<T>()
        {
            try
            {
                var assembly = GetCurrentAssembly();
                var type =
                    assembly.DefinedTypes.Single(
                        t => (t.IsSubclassOf(typeof(T)) || typeof(T).GetTypeInfo().IsAssignableFrom(t)) && !t.IsAbstract)
                        .AsType();

                var instance = (T)Activator.CreateInstance(type);

                return instance;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Gets the default classified instance from the platform assembly implementing the <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">(Abstract) type for which implementation is requested.</typeparam>
        /// <returns>The single instance from the platform assembly implementing the <typeparamref name="T"/> type that is classified as default, 
        /// or null if no or more than one default classified implementations are available.</returns>
        /// <remarks>It is implicitly assumed that all implementation classes has a public, parameter-less constructor.</remarks>
        internal static T GetDefaultPlatformInstance<T>() where T : IDefaultable
        {
            try
            {
                var assembly = GetCurrentAssembly();
                var types = GetConcreteTypes<T>(assembly);

                return GetDefaultOrSingleInstance<T>(types);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        private static Assembly GetCurrentAssembly()
        {
            return typeof(J2kSetup).GetTypeInfo().Assembly;
        }

        private static IEnumerable<Type> GetConcreteTypes<T>(Assembly assembly)
        {
            return assembly.DefinedTypes.Where(
                t =>
                    {
                        try
                        {
                            return (t.IsSubclassOf(typeof(T)) || typeof(T).GetTypeInfo().IsAssignableFrom(t))
                                   && !t.IsAbstract;
                        }
                        catch
                        {
                            return false;
                        }
                    }).Select(t => t.AsType());
        }

        private static T GetDefaultOrSingleInstance<T>(IEnumerable<Type> types) where T : IDefaultable
        {
            var instances = types.Select(
                t =>
                    {
                        try
                        {
                            return (T)Activator.CreateInstance(t);
                        }
                        catch
                        {
                            return default(T);
                        }
                    }).ToList();

            return instances.Count > 1 ? instances.Single(instance => instance.IsDefault) : instances.SingleOrDefault();
        }
    }
}
