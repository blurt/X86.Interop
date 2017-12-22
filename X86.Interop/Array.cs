using System;
using System.Collections;
using System.Collections.Generic;

namespace X86.Interop
{
    /// <summary>
    /// An array stored contiguously in memory with indices of type T
    /// </summary>
    public class Array<T> : Structure, IArray<T> where T : Structure, new()
    {
        private static readonly Func<IntPtr, T> Constructor =
            ConstructorDelegate.Compile<Func<IntPtr, T>>(typeof(T));
        private int _length;
        public static int TSize = DetermineSize();

        public Array() { }
        public Array(IntPtr baseAddress) : base(baseAddress) { }
        
        public int Length
        {
            get { return _length; }
            set
            {
                _length = value;
                Allocate();
            }
        }

        public override int GetSize()
        {
            return TSize * Length * sizeof(byte);
        }
        
        private static int DetermineSize()
        {
            using (var inst = new T())
            {
                return inst.GetSize();
            }
        }

        public T this[int index]
        {
            get
            {
                CheckIndex(index);
                //Log.DebugFormat("Accessing index {0} -> base address is {1}", index, BaseAddress);
                //instantiate the type, passing the base address.
                return Constructor(GetOffset(index * TSize));
            }
            set
            {
                CheckIndex(index);

                //TODO: revise
                byte[] bytes = new byte[TSize];
                if (value == default(T))
                {
                    for (int i = 0; i < TSize; i++) bytes[i] = 0;
                    System.Runtime.InteropServices.Marshal.Copy(bytes, 0, GetOffset(index * TSize), TSize);
                    return;
                }
                // copy from unmanaged to managed
                System.Runtime.InteropServices.Marshal.Copy(value.BaseAddress, bytes, 0, TSize);
                // copy from managed to unmanaged
                System.Runtime.InteropServices.Marshal.Copy(bytes, 0, GetOffset(index * TSize), TSize);


                //Kernel32.CopyMemory(GetOffset(index * TSize), value.BaseAddress, (uint)TSize);
            }
        }

        //We'd need to define array width if we want accessors to multi dimensional array.
        //public T this[int x, int y]
        //{
        //    get
        //    {
        //        int index = x + (y * Width);
        //        return this[index];
        //    }
        //    set
        //    {
        //        int index = x + (y * Width);
        //        this[index] = value;
        //    }
        //}

        protected void CheckIndex(int index)
        {
            if (index >= Length || index < 0)
            {
                string errMsg = string.Format(
                    "The index {0} is out of range = [0,{1}]", index, Length);
                throw new IndexOutOfRangeException(errMsg);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i=0; i<Length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
