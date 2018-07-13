using System;

#if DEBUG
using log4net;
#endif

namespace X86.Interop
{
    /// <summary>
    /// Represents managed X86 assembly at a particular location in memory.
    /// </summary>
    public class ManagedX86Asm : X86Asm, IDisposable
    {
#if DEBUG
        private static readonly ILog Log = LogManager.GetLogger(typeof(ManagedX86Asm));
#endif
        public ManagedX86Asm(int size) : base(System.Runtime.InteropServices.Marshal.AllocHGlobal(size))
        {
            Size = size;
#if DEBUG
            Log.DebugFormat("Allocated {1} bytes at 0x{0:x8}.", Address, Size);
#endif
            SetMemoryProtection(MemoryProtection.ExecuteReadWrite, size);
        }

        public int Size { get; private set; }

        ~ManagedX86Asm()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Address != IntPtr.Zero)
            {
#if DEBUG
                Log.DebugFormat("Freeing managed asm memory block at 0x{0:x8}.", Address);
#endif
                System.Runtime.InteropServices.Marshal.FreeHGlobal(Address);
                Address = IntPtr.Zero;
            }
        }

        //Address = Kernel32.VirtualAlloc(
        //        IntPtr.Zero,
        //        new UIntPtr((uint)asm.Length),
        //        MemoryAllocationType.Commit | MemoryAllocationType.Reserve,
        //        MemoryProtection.ExecuteReadWrite);
        //Kernel32.VirtualFree(Address, 0, MemoryFreeType.Release);
    }
}
