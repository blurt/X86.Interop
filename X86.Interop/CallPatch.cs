using System;

namespace Managed.X86.Interop
{
    /// <summary>
    /// Patches a location in memory with a CALL instruction to intercept execution.
    /// </summary>
    public class CallPatch : Patch
    {
        private readonly Action<X86Writer> _writeIntercept;
        private ManagedX86Asm _asm;

        /// <summary>
        /// Patches a location in memory with a CALL instruction to intercept execution.
        /// </summary>
        /// <param name="address">The address to write the CALL asm</param>
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
        public CallPatch(string dllName, UInt32 offset, Action<X86Writer> writeIntercept)
            : this(DllHelper.GetOffset(dllName, offset), writeIntercept)
        {
        }
        
        /// <summary>
        /// Asm is lazy-loaded, only written to memory once requested.
        /// </summary>
        public IX86Asm Target
        {
            get { return _asm ?? (_asm = X86Asm.Create(_writeIntercept)); }
        }

        protected override void Dispose(bool disposing)
        {
            _asm?.Dispose();
            _asm = null;
            base.Dispose();
        }
                
        public override string ToString()
        {
            return string.Format("{0} => Call to {1} @{2}", Address.ToHexString(), Target.GetType().Name, Target.Address.ToHexString());
        }
    }
}
