using System;
using System.Collections.Generic;
using System.Linq;

namespace Sqlist.NET.Utilities
{
    public static class Check
    {
        public static void Instantiable(Type type)
        {
            if (type.IsClass || type.IsAbstract)
                throw new InvalidOperationException($"The type {type.Name} must be an instantiable class.");
        }

        public static void NotNull<T>(T prm, string prmName)
        {
            if (prm == null)
                throw new ArgumentNullException(prmName);
        }

        public static void NotNullOrEmpty(string prm, string prmName)
        {
            if (string.IsNullOrEmpty(prm))
                throw new ArgumentException($"The argument {prmName} can neither be null or empty.");
        }

        public static void NotEmpty<T>(IEnumerable<T> prm, string prmName)
        {
            if (prm.Count() == 0)
                throw new ArgumentException($"The argument {prmName} cannot be empty.");
        }
    }
}
