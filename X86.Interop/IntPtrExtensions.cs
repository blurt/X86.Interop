using System;

namespace X86.Interop
{
    public static class IntPtrExtensions
    {
        public static string ToHexString(this IntPtr address)
        {
            return Environment.Is64BitProcess ? string.Format("0x{0:x8}", address.ToInt32())
                                              : string.Format("0x{0:x16}", address.ToInt64());
        }

        public static IntPtr AddOffset(this IntPtr baseAddress, int offset)
        {
            return Environment.Is64BitProcess ? (IntPtr)(baseAddress.ToInt64() + offset)
                                              : (IntPtr)(baseAddress.ToInt32() + offset);
        }
    }
}
