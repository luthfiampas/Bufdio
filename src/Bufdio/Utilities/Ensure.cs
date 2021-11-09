using System;
using System.Diagnostics;

namespace Bufdio.Utilities;

[DebuggerStepThrough]
internal static class Ensure
{
    public static void That<TException>(bool condition, string message = "") where TException : Exception
    {
        if (!condition)
        {
            throw (TException)Activator.CreateInstance(typeof(TException), message);
        }
    }

    public static void NotNull<T>(T argument, string name)
    {
        if (string.IsNullOrEmpty(name?.Trim()))
        {
            name = nameof(argument);
        }

        if (argument == null)
        {
            throw new ArgumentNullException(name);
        }
    }

    public static void NotNull(object argument, string name)
    {
        NotNull<object>(argument, name);
    }
}
