using System;
using System.Runtime.InteropServices;

namespace X86.Interop
{
    /// <summary>
    /// Manages memory allocation and deallocation for unmanaged objects, such as Structures.
    /// </summary>
    internal class MemoryManager
    {
        public IntPtr Allocate(int size)
        {
            IntPtr blockPtr = Marshal.AllocHGlobal(size);
            //TODO: needed?
            //Kernel32.FillMemory(blockPtr, (uint)size, 0);
            return blockPtr;
        }

        public void Free(IntPtr blockPtr)
        {
            Marshal.FreeHGlobal(blockPtr);
        }
    }
}
