using System;
using System.Runtime.InteropServices;

namespace Managed.X86.Interop
{
    #region Enumerations

    [Flags]
    internal enum AccessRights : uint
    {
        /// <summary>
        /// Required to delete the object.
        /// </summary>
        Delete = 0x00010000,

        /// <summary>
        /// Required to read information in the security descriptor for the object, not including the information in the SACL.
        /// To read or write the SACL, you must request the AccessSystemSecurity access right.
        /// For more information, see SACL Access Right.
        /// </summary>
        ReadControl = 0x00020000,

        /// <summary>
        /// The right to use the object for synchronization.
        /// This enables a thread to wait until the object is in the signaled state.
        /// Required to wait for a process to terminate.
        /// </summary>
        Synchronize = 0x00100000,

        /// <summary>
        /// Required to modify the DACL in the security descriptor for the object.
        /// </summary>
        WriteDAC = 0x00040000,

        /// <summary>
        /// Required to change the owner in the security descriptor for the object.
        /// </summary>
        WriteOwner = 0x00080000,

        /// <summary>
        /// Required to create a process.
        /// </summary>
        ProcessCreateProcess = 0x0080,

        /// <summary>
        /// Required to create a thread.
        /// </summary>
        ProcessCreateThread = 0x0002,

        /// <summary>
        /// Required to duplicate a handle.
        /// </summary>
        ProcessDuplicateHandle = 0x0040,

        /// <summary>
        /// Required to retrieve certain information about a process, such as its token, exit code, and priority class.
        /// </summary>
        ProcessQueryInformation = 0x0400,

        /// <summary>
        /// Required to retrieve certain information about a process.
        /// A handle that has the ProcessQueryInformation access right is automatically granted ProcessQueryLimitedAccess.
        /// Windows Server 2003 and Windows XP/2000:  This access right is not supported.
        /// </summary>
        ProcessQueryLimitedInformation = 0x1000,

        /// <summary>
        /// Required to set certain information about a process, such as its priority class.
        /// </summary>
        ProcessSetInformation = 0x0200,

        /// <summary>
        /// Required to set memory limits.
        /// </summary>
        ProcessSetQuota = 0x0100,

        /// <summary>
        /// Required to suspend or resume a process.
        /// </summary>
        ProcessSuspendResume = 0x0800,

        /// <summary>
        /// Required to terminate a process.
        /// </summary>
        ProcessTerminate = 0x0001,

        /// <summary>
        /// Required to perform an operation on the address space of a process.
        /// </summary>
        ProcessVirtualMemoryOperation = 0x0008,

        /// <summary>
        /// Required to read memory in a process.
        /// </summary>
        ProcessVirtualMemoryRead = 0x0010,

        /// <summary>
        /// Required to write to memory in a process.
        /// </summary>
        ProcessVirtualMemoryWrite = 0x0020,
    }

    [Flags]
    internal enum CreationAttributes : uint
    {
        /// <summary>
        /// The thread runs immediately after creation.
        /// </summary>
        CreateRunning = 0x00000000,

        /// <summary>
        /// The thread is created in a suspended state, and does not run until the thread is resumed.
        /// </summary>
        CreateSuspended = 0x00000004,

        /// <summary>
        /// The stack size parameter specifies the initial reserve size of the stack.
        /// If this flag is not specified, the stack size specifies the commit size.
        /// Windows 2000:  The StackSizeParameterIsAReservation flag is not supported.
        /// </summary>
        StackSizeParameterIsAReservation = 0x00010000,
    }

    [Flags]
    internal enum MemoryAllocationType : uint
    {
        /// <summary>
        /// Allocates physical storage in memory or in the paging file on disk for the specified reserved memory pages.
        /// The function initializes the memory to zero.
        /// To reserve and commit pages in one step, use Commit | Reserve.
        /// Memory allocation fails if you attempt to commit a page that has not been reserved.
        /// The result is an invalid address.
        /// An attempt to commit a page that is already committed does not cause the allocation to fail.
        /// This means that you can commit pages without first determining the current commitment state of each page.
        /// </summary>
        Commit = 0x1000,

        /// <summary>
        /// Reserves a range of the process's virtual address space without allocating any actual physical storage in memory or in the paging file on disk.
        /// You commit reserved pages using Commit.
        /// To reserve and commit pages in one step, use Commit | Reserve.
        /// </summary>
        Reserve = 0x2000,

        /// <summary>
        /// Indicates that data in the memory range specified is no longer of interest.
        /// The pages should not be read from or written to the paging file.
        /// However, the memory block will be used again later, so it should not be decommitted.
        /// This value cannot be used with any other value.
        /// Using this value does not guarantee that the range operated on will contain zeroes.
        /// If you want the range to contain zeroes, decommit the memory and then recommit it.
        /// When you use Reset, the protection value is ignored.
        /// However, you must still set the protection to a valid protection value, such as NoAccess.
        /// An error is returned if you use Reset and the range of memory is mapped to a file.
        /// A shared view is only acceptable if it is mapped to a paging file.
        /// </summary>
        Reset = 0x80000,

        /// <summary>
        ///  Allocates memory using large page support.
        ///  The size and alignment must be a multiple of the large-page minimum.
        ///  Windows XP/2000:   This flag is not supported.
        /// </summary>
        LargePages = 0x20000000,

        /// <summary>
        /// Reserves an address range that can be used to map Address Windowing Extensions (AWE) pages.
        /// This value must be used with Reserve and no other values.
        /// </summary>
        Physical = 0x400000,

        /// <summary>
        ///  Allocates memory at the highest possible address.
        /// </summary>
        TopDown = 0x100000
    }

    [Flags]
    internal enum MemoryFreeType : uint
    {
        /// <summary>
        /// Decommits the specified region of committed pages.
        /// After the operation, the pages are in the reserved state.
        /// The function does not fail if you attempt to decommit an uncommitted page.
        /// This means that you can decommit a range of pages without first determining their current commitment state.
        /// Do not use this value with Release.
        /// </summary>
        Decommit = 0x4000,

        /// <summary>
        /// Releases the specified region of pages.
        /// After the operation, the pages are in the free state.
        /// If you specify this value, the size must be zero, and address must point to the base address returned by the allocation function when the region is reserved.
        /// The function fails if either of these conditions is not met.
        /// If any pages in the region are committed currently, the function first decommits, and then releases them.
        /// The function does not fail if you attempt to release pages that are in different states, some reserved and some committed.
        /// This means that you can release a range of pages without first determining the current commitment state.
        /// Do not use this value with Decommit.
        /// </summary>
        Release = 0x8000,
    }

    [Flags]
    internal enum MemoryProtection : uint
    {
        /// <summary>
        /// Enables execute access to the committed region of pages.
        /// An attempt to read from or write to the committed region results in an access violation.
        /// </summary>
        Execute = 0x10,

        /// <summary>
        /// Enables execute, read-only, or copy-on-write access to the committed region of pages.
        /// An attempt to write to the committed region results in an access violation.
        /// Windows Server 2003 and Windows XP:  This attribute is not supported by the File Mapping functions until Windows XP with SP2 and Windows Server 2003 with SP1.
        /// </summary>
        ExecuteRead = 0x20,

        /// <summary>
        /// Enables execute, read-only, read/write, or copy-on-write access to the committed region of pages.
        /// Windows Server 2003 and Windows XP:  This attribute is not supported by the File Mapping functions until Windows XP with SP2 and Windows Server 2003 with SP1.
        /// </summary>
        ExecuteReadWrite = 0x40,

        /// <summary>
        /// Enables execute, read-only, or copy-on-write access to the committed region of image file code pages.
        /// This value is equivalent to ExecuteRead.
        /// This flag is not supported by the Memory Allocation functions.
        /// It is not supported by the File Mapping function until Windows Vista with SP1 and Windows Server 2008.
        /// </summary>
        ExecuteWriteCopy = 0x80,

        /// <summary>
        /// Disables all access to the committed region of pages.
        /// An attempt to read from, write to, or execute the committed region results in an access violation exception, called a general protection (GP) fault.
        /// This flag is not supported by the File Mapping functions.
        /// </summary>
        NoAccess = 0x01,

        /// <summary>
        /// Enables read-only or copy-on-write access to the committed region of pages.
        /// An attempt to write to the committed region results in an access violation.
        /// If the system differentiates between read-only access and execute access, an attempt to execute code in the committed region results in an access violation.
        /// </summary>
        ReadOnly = 0x02,

        /// <summary>
        /// Enables read-only, read/write, or copy-on-write access to the committed region of pages.
        /// </summary>
        ReadWrite = 0x04,

        /// <summary>
        /// Enables read-only or copy-on-write access to the committed region of pages.
        /// This value is equivalent to ReadOnly.
        /// This flag is not supported by the Memory Allocation functions.
        /// </summary>
        WriteCopy = 0x08,

        /// <summary>
        /// Pages in the region become guard pages.
        /// Any attempt to access a guard page causes the system to raise a page violation exception and turn off the guard page status.
        /// Guard pages thus act as a one-time access alarm.
        /// When an access attempt leads the system to turn off guard page status, the underlying page protection takes over.
        /// If a guard page exception occurs during a system service, the service typically returns a failure status indicator.
        /// This value cannot be used with NoAccess.
        /// This flag is not supported by the File Mapping functions.
        /// </summary>
        Guard = 0x100,

        /// <summary>
        /// Does not allow caching of the committed regions of pages in the CPU cache.
        /// The hardware attributes for the physical memory should be specified as "no cache".
        /// This is not recommended for general usage.
        /// It is useful for device drivers, for example, mapping a video frame buffer with no caching.
        /// This value cannot be used with NoAccess.
        /// This flag is not supported by the File Mapping functions.
        /// </summary>
        NoCache = 0x200,

        /// <summary>
        /// Enables write-combined memory accesses.
        /// When enabled, the processor caches memory write requests to optimize performance.
        /// Thus, if two requests are made to write to the same memory address, only the more recent write may occur.
        /// Note that the Guard and NoCache flags cannot be specified with WriteCombine.
        /// If an attempt is made to do so, the invalid page protection NT error code is returned.
        /// This flag is not supported by the File Mapping functions.
        /// </summary>
        WriteCombine = 0x400,
    }

    #endregion

    internal static class Kernel32
    {
        #region Imports

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenProcess(AccessRights dwDesiredAccess, Boolean bInheritHandle, Int32 dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Boolean CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern IntPtr GetProcAddress(IntPtr module, String procedureName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr module, UInt16 ordinal);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(String module);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAlloc(
                IntPtr lpAddress,
                UIntPtr dwSize,
                MemoryAllocationType flAllocationType,
                MemoryProtection flProtect);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool VirtualFree(
            IntPtr lpAddress,
            uint dwSize,
            MemoryFreeType flFreeType);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int VirtualProtect(IntPtr lpAddress, int dwSize, MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, UInt32 dwSize, MemoryAllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Boolean VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UInt32 dwSize, MemoryFreeType dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Boolean WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, Byte[] buffer, UInt32 size, out UInt32 lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Boolean ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, Byte[] buffer, UInt32 size, out UInt32 lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, CreationAttributes dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Boolean GetExitCodeThread(IntPtr hThread, out IntPtr lpExitCode);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        internal static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        [DllImport("kernel32.dll")]
        internal static extern void RtlZeroMemory(IntPtr dst, int length);

        [DllImport("kernel32.dll", EntryPoint = "RtlFillMemory", SetLastError = false)]
        internal static extern void FillMemory(IntPtr destination, uint length, byte fill);

        //[DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        //public static extern IntPtr MemSet(IntPtr dest, int c, int count);

        #endregion
    }
}
