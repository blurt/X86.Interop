using System;
namespace X86.Interop
{
    public interface IPatch : IX86Asm, IDisposable
    {
        bool IsInstalled { get; }
        void Install();
        void Uninstall();
    }
}
