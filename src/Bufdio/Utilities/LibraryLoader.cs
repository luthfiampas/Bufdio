using System;
using System.Runtime.InteropServices;
using Bufdio.Interop;

namespace Bufdio.Utilities;

internal sealed class LibraryLoader : IDisposable
{
    private const int RTLD_NOW = 2;
    private readonly IntPtr _handle;
    private bool _disposed;

    public LibraryLoader(string libraryName)
    {
        Ensure.NotNull(libraryName, nameof(libraryName));

        if (PlatformInfo.IsWindows)
        {
            _handle = Kernel32.LoadLibrary(libraryName);
        }
        else if (PlatformInfo.IsLinux)
        {
            _handle = Libdl2.dlopen(libraryName, RTLD_NOW);
        }
        else if (PlatformInfo.IsOSX)
        {
            _handle = Libdl.dlopen(libraryName, RTLD_NOW);
        }
        else
        {
            throw new NotSupportedException("Platform is not supported.");
        }

        Ensure.That<Exception>(_handle != IntPtr.Zero, $"Could not load native libary: {libraryName}.");
    }

    public TDelegate LoadFunc<TDelegate>(string name)
    {
        IntPtr ptr;

        if (PlatformInfo.IsWindows)
        {
            ptr = Kernel32.GetProcAddress(_handle, name);
        }
        else if (PlatformInfo.IsLinux)
        {
            ptr = Libdl2.dlsym(_handle, name);
        }
        else if (PlatformInfo.IsOSX)
        {
            ptr = Libdl.dlsym(_handle, name);
        }
        else
        {
            throw new NotSupportedException("Platform is not supported.");
        }

        Ensure.That<Exception>(ptr != IntPtr.Zero, $"Could not load function name: {name}.");

        return Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (PlatformInfo.IsWindows)
        {
            Kernel32.FreeLibrary(_handle);
        }
        else if (PlatformInfo.IsLinux)
        {
            Libdl2.dlclose(_handle);
        }
        else if (PlatformInfo.IsOSX)
        {
            Libdl.dlclose(_handle);
        }
        else
        {
            throw new NotSupportedException("Platform is not supported.");
        }

        _disposed = true;
    }
}
