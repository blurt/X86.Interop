using System;

namespace X86.Interop
{
    public class Pointer : Structure
    {
        public Pointer(IntPtr baseAddress) : base(baseAddress) {}
        public Pointer() {}

        public IntPtr Value
        {
            get { return TryReadIntPtr(0, out IntPtr ptr) ? ptr : IntPtr.Zero; }
            set { WriteIntPtr(0, value);}
        }

        public override int GetSize()
        {
            return 4 * sizeof (byte);
        }
    }
}
