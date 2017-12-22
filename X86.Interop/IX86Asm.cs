using System;

namespace Managed.X86.Interop
{
    /// <summary>
    /// X86 asm at a particular location in memory.
    /// To reference x86 asm already in memory <see cref="X86Asm.At(IntPtr)"/>. To create managed X86 asm: <see cref="X86Asm.Create(Action{X86Writer})"/>
    /// </summary>
    public interface IX86Asm
    {
        IntPtr Address { get; }
    }
}
