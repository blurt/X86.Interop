using System;
using System.Collections;
using System.Collections.Generic;

#if DEBUG
using log4net;
#endif

namespace X86.Interop
{
    /// <summary>
    /// An array of pointers, where each pointer points to a structure of type T
    /// </summary>
    public class PointerArray<T> : Array<Pointer>, IArray<T> where T : Structure, new()
    {
        private static readonly Func<IntPtr, T> Constructor =
            ConstructorDelegate.Compile<Func<IntPtr, T>>(typeof(T));

#if DEBUG
        private static readonly ILog Log = LogManager.GetLogger(typeof (PointerArray<T>));
#endif
        public PointerArray(IntPtr baseAddress) : base(baseAddress) { } 
        public PointerArray() {}

        public new T this[int index]
        {
            get
            {
                CheckIndex(index);
                return base[index].Value == IntPtr.Zero ? null : Constructor(base[index].Value);
            }
            set
            {
                CheckIndex(index);
                base[index].Value = value == null ? IntPtr.Zero : value.BaseAddress;
            }
        }

        //protected override Structure CopyShallow()
        //{
        //    return new PointerArray<T>() { Length = Length };
        //}

        //        protected override void CopyPublicProperties(Structure copy, IDictionary<IntPtr, IList<Structure>> processedItems, int depth)
        //        {
        //#if DEBUG
        //            Log.DebugFormat("{0}PointerArray of size {1}", new string('\t', depth), Length);
        //#endif
        //            var pointerArrayCopy = (PointerArray<T>) copy;
        //            for (int i = 0; i < Length; i++)
        //            {
        //#if DEBUG
        //                Log.DebugFormat("{0}Processed {1}", new string('\t', depth), i);
        //#endif
        //                T item = this[i];
        //                pointerArrayCopy[i] = item == null ? null : (T)CopyPropertyStruct(item, processedItems, depth);
        //            }
        //        }

        public new IEnumerator<T> GetEnumerator()
        {
#if DEBUG
            Log.Debug("Enumerating pointer array..");
#endif
            for (int i = 0; i < Length; i++)
            {
                var ret = this[i];
                if (ret != null)
                    yield return ret;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
