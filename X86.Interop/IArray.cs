using System.Collections.Generic;

namespace D2XF.Game.Structures
{
    public interface IArray<out T> : IEnumerable<T>
    {
        T this[int index] { get; }
        int Length { get; set; }
    }
}
