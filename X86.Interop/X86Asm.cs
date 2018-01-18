#if DEBUG
using log4net;
#endif
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace X86.Interop
{
    /// <summary>
    /// X86 assembly at a particular location in memory.
    /// See static factory methods: <see cref="X86Asm.At(IntPtr)"/>, <see cref="X86Asm.At(string, UInt32)"/> <see cref="X86Asm.Create(Action{X86Writer})"/>
    /// </summary>
    public class X86Asm : IX86Asm
    {
#if DEBUG
        private static readonly ILog Log = LogManager.GetLogger(typeof(X86Asm));
#endif
        
        protected X86Asm(IntPtr address)
        {
            Address = address;
        }

        public IntPtr Address { get; protected set; }

        public int Write(Action<X86Writer> writeAction)
        {
            byte[] asm = GetAsmBytes(Address, writeAction);
            return Write(asm);
        }

        public int Write(byte[] bytes)
        {
#if DEBUG
            Log.DebugFormat("Copying {0} bytes to {1}.", bytes.Length, Address.ToHexString());
#endif
            Marshal.Copy(bytes, 0, Address, bytes.Length);
            return bytes.Length;
        }

        //public void Execute()
        //{
        //  hmmm
        //}

        public static X86Asm At(IntPtr address)
        {
            return new X86Asm(address);
        }

        public static X86Asm At(string dllName, Int32 offset)
        {
            return new X86Asm(DllHelper.GetOffset(dllName, offset));
        }

        public static ManagedX86Asm Create(Action<X86Writer> writeAction)
        {
            byte[] asm = GetAsmBytes(IntPtr.Zero, writeAction);
            var managedAsm = new ManagedX86Asm(asm.Length);
            managedAsm.Write(writeAction);
            return managedAsm;
        }
        
        internal static byte[] GetAsmBytes(IntPtr address, Action<X86Writer> writeAction)
        {
            //write asm to memory stream
            MemoryStream stream = new MemoryStream();
            X86Writer writer = new X86Writer(stream, address);
            writeAction.Invoke(writer);
            return stream.ToArray();
        }
    }
}
