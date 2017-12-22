using System;

namespace D2XF.Game.Structures
{
    public interface IStructure : IDisposable
    {
        /// <summary>
        /// The location in memory of the structure
        /// </summary>
        IntPtr BaseAddress { get; }


        /// <summary>
        /// Returns the size of the structure in bytes
        /// </summary>
        /// <returns></returns>
        //int GetSize();


        /// <summary>
        /// Returns the sum of the size of the structure in bytes and all structures that it points to
        /// </summary>
        /// <returns></returns>
        //int GetDeepSize();

        /// <summary>
        /// Copies the structure and all structure properties to a new location in memory
        /// </summary>
        /// <returns></returns>
        //Structure Copy();
    }
}
