using System;

namespace D2XF.Game.Structures
{
    public class Pointer : Structure
    {
        public Pointer(IntPtr baseAddress) : base(baseAddress) {}
        public Pointer() {}

        public IntPtr Value
        {
            get { return ReadIntPtr(0); }
            set { WriteIntPtr(0, value);}
        }

        public override int GetSize()
        {
            return 4 * sizeof (byte);
        }
    }
}
