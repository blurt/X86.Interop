using System;

namespace X86.Interop
{
    /// <summary>
    /// Patches a location in memory with a CALL instruction to intercept execution.
    /// </summary>
    public class CallPatch : Patch
    {
        private readonly Action<X86Writer> _writeIntercept;
        private IntPtr _callTarget;
        private ManagedX86Asm _asm;

        /// <summary>
        /// Patches a location in memory with a CALL instruction to intercept execution.
        /// </summary>
        /// <param name="address">The address to patch</param>
        /// <param name="callTarget">The target address to CALL</param>
        public CallPatch(IntPtr address, IntPtr callTarget)
            : base(address)
        {
            _callTarget = callTarget;
            _writeAsm = writer => writer.Call(callTarget);
        }

        public CallPatch(IntPtr address, IX86Asm asm) : base(address)
        {
            _callTarget = asm.Address;
            _writeAsm = writer => writer.Call(asm.Address);
        }

        /// <summary>
        /// Patches a location in memory with a CALL instruction to intercept execution.
        /// </summary>
        /// <param name="address">The address to patch</param>
        /// <param name="writeIntercept">A delegate that writes the intercept asm, which will be CALLed</param>
        public CallPatch(IntPtr address, Action<X86Writer> writeIntercept)
            : base(address)
        {
            Guard.ArgumentNotNull(nameof(writeIntercept), writeIntercept);
            _writeIntercept = writeIntercept;
            _writeAsm = writer => writer.Call(Target.Address);
        }

        /// <summary>
        /// Patches a location in memory with a CALL instruction to intercept execution.
        /// </summary>
        /// <param name="dllName">The name of the native dll loaded in memory</param>
        /// <param name="offset">The offset from the base address of the dll</param>
        /// <param name="writeIntercept">A delegate that writes the intercept asm, which will be CALLed</param>
        public CallPatch(string dllName, Int32 offset, Action<X86Writer> writeIntercept)
            : this(DllHelper.GetOffset(dllName, offset), writeIntercept)
        {
        }
        
        /// <summary>
        /// Asm is lazy-loaded, only written to memory once requested.
        /// </summary>
        public IX86Asm Target
        {
            get
            {
                return _callTarget != IntPtr.Zero ? X86Asm.At(_callTarget)
                                                  : _asm ?? (_asm = X86Asm.Create(_writeIntercept));
            }
        }

        protected override void Dispose(bool disposing)
        {
            _asm?.Dispose();
            _asm = null;
            base.Dispose(disposing);
        }
                
        public override string ToString()
        {
            return string.Format("{0} => Call to {1} @{2}", Address.ToHexString(), Target.GetType().Name, Target.Address.ToHexString());
        }
    }
}
