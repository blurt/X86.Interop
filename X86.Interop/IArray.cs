using System.Collections.Generic;

namespace X86.Interop
{
    public interface IArray<out T> : IEnumerable<T>
    {
        T this[int index] { get; }
        int Length { get; set; }
    }
}
