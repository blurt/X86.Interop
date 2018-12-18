using System.Collections.Generic;

namespace X86.Interop
{
    /// <summary>
    /// An array stored contiguously in memory with indices of type T
    /// </summary>
    public interface IArray<out T> : IEnumerable<T>
    {
        T this[int index] { get; }
        int Length { get; set; }
    }
}
