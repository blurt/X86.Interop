using System;
using System.Runtime.InteropServices;

namespace Managed.X86.Interop
{
    public static class DllHelper
    {
        public static IntPtr GetOffset(string dllName, UInt32 offset)
        {
            Guard.ArgumentNotNullOrEmpty(nameof(dllName), dllName);
            return GetDllBaseAddress(dllName).AddOffset(offset);
        }
        
        public static T GetDelegateFromName<T>(string dllName, string function) where T : class
        {
            Guard.ArgumentNotNullOrEmpty(nameof(dllName), dllName);
            return GetDelegateFromName<T>(GetDllBaseAddress(dllName), function);
        }
                
        public static T GetDelegateFromName<T>(IntPtr module, string function) where T : class
        {
            if (module == IntPtr.Zero)
            {
                string errMsg = string.Format("Module handle pointer is zero. Failed to get function \"{0}\".", function);
                throw new ArgumentException(errMsg, nameof(module));
            }
            IntPtr fnAddress = Kernel32.GetProcAddress(module, function);
            if (fnAddress == IntPtr.Zero)
            {
                string errMsg = string.Format("Could not find function \"{0}\" at module 0x{1:x}", function, module);
                throw new ArgumentException(errMsg);
            }
            return GetDelegateFromPointer<T>(fnAddress);
        }
        
        public static T GetDelegateFromOrdinal<T>(string dllName, UInt16 ordinal) where T : class
        {
            Guard.ArgumentNotNullOrEmpty(nameof(dllName), dllName);
            IntPtr fnAddress = Kernel32.GetProcAddress(GetDllBaseAddress(dllName), ordinal);
            if (fnAddress == IntPtr.Zero)
            {
                throw new ArgumentException(string.Format("Could not find ordinal {0} in {1}", ordinal, dllName));
            }
            return GetDelegateFromPointer<T>(fnAddress);
        }

        public static T GetDelegateFromPointer<T>(IntPtr fnAddress) where T : class
        {
            try
            {
                return Marshal.GetDelegateForFunctionPointer(fnAddress, typeof(T)) as T;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Function at address \"{0}\" is not of type \"{1}\".",
                                              fnAddress, typeof(T));
                throw new ArgumentException(errMsg, ex);
            }
        }

        public static IntPtr GetDllBaseAddress(string dllName)
        {
            Guard.ArgumentNotNullOrEmpty(nameof(dllName), dllName);
            IntPtr moduleAddress = Kernel32.GetModuleHandle(dllName);
            if (moduleAddress == IntPtr.Zero)
            {
                moduleAddress = Kernel32.LoadLibrary(string.Format("{0}.dll", dllName));
                if (moduleAddress == IntPtr.Zero)
                    throw new InvalidOperationException(string.Format("Failed to load \"{0}\".", dllName));
            }
            return moduleAddress;
        }
    }
}
