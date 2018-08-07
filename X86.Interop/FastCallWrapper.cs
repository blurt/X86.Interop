using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace X86.Interop
{
    /// <summary>
    /// Creates a native stub of X86 that can either: call a __fastcall function pointer (for .NET -> X86 calls), or wrap a .NET delegate in a __fastcall function pointer (for X86 -> .NET)
    /// </summary>
    public class FastCallWrapper<TDelegate> : ManagedX86Asm
    {
        private readonly TDelegate _delegate;
        private readonly IntPtr _fnPtr;
        private static readonly byte[] _template;
        private static readonly byte[] _callTemplate;

        static FastCallWrapper()
        {
            var methodInfo = typeof(TDelegate).GetMethod("Invoke");
            var numParameters = methodInfo.GetParameters().Count();
            switch (numParameters)
            {
                // TODO: to support other parameter sizes, we could check for size using Marshal.SizeOf() and use appropriate template.. 
                case 0:
                    _template = fastcall_void;
                    _callTemplate = call_fastcall_void;
                    break;
                case 1:
                    _template = fastcall_dword;
                    _callTemplate = call_fastcall_dword;
                    break;
                case 2:
                    _template = fastcall_dword_dword;
                    _callTemplate = call_fastcall_dword_dword;
                    break;
                case 3:
                    _template = fastcall_dword_dword_dword;
                    _callTemplate = call_fastcall_dword_dword_dword;
                    break;;
                default: throw new NotSupportedException($"Delegate of type {typeof(TDelegate)} is not yet supported.");
            }
        }

        /// <summary>
        /// Creates a stub of X86 asm that can be called with __fastcall convention, and invoke the delegate using __stdcall
        /// </summary>
        /// <param name="func">must be delegate type (not typesafe)</param>
        public FastCallWrapper(TDelegate func) : base(_template.Length)
        {
            //the CLR generates a native stub for the std call
            //it is automatically disposed when the delegate is garbage collected
            //.. so it is necessary to keep a reference to the delegate
            _delegate = func;
            _fnPtr = Marshal.GetFunctionPointerForDelegate(_delegate);
            Write(ReplaceStubAddress(_template, _fnPtr));
        }

        public FastCallWrapper(IntPtr pFunction) : base(_callTemplate.Length)
        {
            _fnPtr = pFunction;
            Write(ReplaceStubAddress(_callTemplate, _fnPtr));
            _delegate = Marshal.GetDelegateForFunctionPointer<TDelegate>(Address);
        }

        public IntPtr FuncPtr => _fnPtr;
        public TDelegate Delegate => _delegate;
        
        //__fastcall Wrapper() { __stdcall func(); };
        private static byte[] fastcall_void = new byte[]
        {
            0x55, //               PUSH EBP
            0x8B, 0xEC,//             MOV EBP,ESP
            0x83, 0xEC, 0x44,//          SUB ESP,44
            0x53,//               PUSH EBX
            0x56,  //             PUSH ESI
            0x57, //               PUSH EDI
            0xC7, 0x45, 0xFC, 0xF0, 0xF0, 0xF0, 0xF0, // MOV DWORD PTR SS:[EBP - 4], F0F0F0F0
            0xFF, 0x55, 0xFC, //          CALL DWORD PTR SS:[EBP-4]
            0x5F, //               POP EDI
            0x5E, //               POP ESI
            0x5B, // POP EBX
            0x8B, 0xE5, // MOV ESP,EBP
            0x5D,   //            POP EBP
            0xC3, // RETN
        };

        // __stdcall Wrapper() { __fastcall func(); }
        private static byte[] call_fastcall_void = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x44, 0x53, 0x56, 0x57, 0xC7, 0x45, 0xFC, 0xF0, 0xF0, 0xF0, 0xF0, 0xFF, 0x55, 0xFC, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC3
        };

        //__fastcall Wrapper(DWORD param)
        private static byte[] fastcall_dword = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x48, 0x53, 0x56, 0x57, 0x89, 0x4D, 0xFC, 0xC7, 0x45, 0xF8, 0xF0, 0xF0, 0xF0, 0xF0, 0x8B, 0x45, 0xFC, 0x50, 0xFF, 0x55, 0xF8, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC3
        };

        // __stdcall Wrapper(DWORD param) { func(param); }   // where func has a __fastcall convention
        private static byte[] call_fastcall_dword = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x44, 0x53, 0x56, 0x57, 0xC7, 0x45, 0xFC, 0xF0, 0xF0, 0xF0, 0xF0, 0x8B, 0x4D, 0x08, 0xFF, 0x55, 0xFC, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC2, 0x04, 0x00
        };

        //__fastcall Wrapper(DWORD param, DWORD param2)
        private static byte[] fastcall_dword_dword = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x4C, 0x53, 0x56, 0x57, 0x89, 0x55, 0xF8, 0x89, 0x4D, 0xFC, 0xC7, 0x45, 0xF4, 0xF0, 0xF0, 0xF0, 0xF0, 0x8B, 0x45, 0xF8, 0x50, 0x8B, 0x4D, 0xFC, 0x51, 0xFF, 0x55, 0xF4, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC3
        };

        private static byte[] call_fastcall_dword_dword = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x44, 0x53, 0x56, 0x57, 0xC7, 0x45, 0xFC, 0xF0, 0xF0, 0xF0, 0xF0, 0x8B, 0x55, 0x0C, 0x8B, 0x4D, 0x08, 0xFF, 0x55, 0xFC, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC2, 0x08, 0x00
        };

        //__fastcall Wrapper(DWORD param, DWORD param2, DWORD param3)
        private static byte[] fastcall_dword_dword_dword = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x4C, 0x53, 0x56, 0x57, 0x89, 0x55, 0xF8, 0x89, 0x4D, 0xFC, 0xC7, 0x45, 0xF4, 0xF0, 0xF0, 0xF0, 0xF0, 0x8B, 0x45, 0x08, 0x50, 0x8B, 0x4D, 0xF8, 0x51, 0x8B, 0x55, 0xFC, 0x52, 0xFF, 0x55, 0xF4, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC2, 0x04, 0x00
        };

        private static byte[] call_fastcall_dword_dword_dword = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x44, 0x53, 0x56, 0x57, 0xC7, 0x45, 0xFC, 0xF0, 0xF0, 0xF0, 0xF0, 0x8B, 0x45, 0x10, 0x50, 0x8B, 0x55, 0x0C, 0x8B, 0x4D, 0x08, 0xFF, 0x55, 0xFC, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC2, 0x0C, 0x00
        };

        // ref: https://blogs.msdn.microsoft.com/winsdk/2015/02/09/c-and-fastcall-how-to-make-them-work-together-without-ccli-shellcode/
        private static byte[] ReplaceStubAddress(byte[] stub, IntPtr address)
        {
            List<byte> newcode = new List<byte>();
            List<byte> currentcodequeued = new List<byte>();

            byte[] cBytes = new byte[] { 0xF0, 0xF0, 0xF0, 0xF0 };
            byte[] nBytes = BitConverter.GetBytes(address.ToInt32());
            int currentMatchNumber = 0; bool matched = false;

            //Loop through the code, find the matching address, replace it with the address from the delegate
            for (int i = 0; i < stub.Length; i++)
            {
                if (matched)
                {
                    newcode.Add(stub[i]);
                }
                else if (stub[i] == cBytes[currentMatchNumber])
                {
                    currentMatchNumber++;
                    if (currentMatchNumber == cBytes.Length)
                    {
                        //Add the real address instead of the fake
                        newcode.AddRange(nBytes);
                        currentcodequeued.Clear();
                        matched = true;

                    }
                    else
                    {
                        currentcodequeued.Add(stub[i]);
                    }
                }
                else
                {
                    if (currentcodequeued.Count > 0)
                    {
                        newcode.AddRange(currentcodequeued);
                        currentcodequeued.Clear();
                    }
                    newcode.Add(stub[i]);
                }
            }
            return newcode.ToArray();
        }
    }
}
