using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace X86.Interop
{
    /// <summary>
    /// Wraps a delegate or function pointer in a method with __fastcall declaration
    /// NOTE: The delegate type is expected to have std calling convention, and can have between 0 and 3 parameters which must all must be sizeof(DWORD)
    /// </summary>
    public class FastCallWrapper<TDelegate> : ManagedX86Asm
    {
        private readonly TDelegate _delegate;
        private static readonly byte[] _template;
        private static readonly byte[] _callTemplate;

        static FastCallWrapper()
        {
            var methodInfo = typeof(TDelegate).GetMethod("Invoke");
            var numParameters = methodInfo.GetParameters().Count();
            switch (numParameters)
            {
                // TODO: to support other parameter sizes, we could check for primitive type and use appropriate template..
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

        public FastCallWrapper(TDelegate func) : base(_template.Length)
        {
            //the CLR generates a native stub for the std call
            //it is automatically disposed when the delegate is garbage collected
            //.. so it is necessary to keep a reference to the delegate
            _delegate = func;
            var delegatePtr = Marshal.GetFunctionPointerForDelegate(_delegate);
            Write(ReplaceStubAddress(_template, delegatePtr));
        }

        public FastCallWrapper(IntPtr pFunction) : base(_callTemplate.Length)
        {
            Write(ReplaceStubAddress(_callTemplate, pFunction));
            _delegate = Marshal.GetDelegateForFunctionPointer<TDelegate>(Address);
        }

        public TDelegate Delegate => _delegate;
        
        //__fastcall Wrapper();
        public static byte[] fastcall_void = new byte[]
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

        public static byte[] call_fastcall_void = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x44, 0x53, 0x56, 0x57, 0xC7, 0x45, 0xFC, 0xF0, 0xF0, 0xF0, 0xF0, 0xFF, 0x55, 0xFC, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC3
        };

        //__fastcall Wrapper(DWORD param)
        public static byte[] fastcall_dword = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x48, 0x53, 0x56, 0x57, 0x89, 0x4D, 0xFC, 0xC7, 0x45, 0xF8, 0xF0, 0xF0, 0xF0, 0xF0, 0x8B, 0x45, 0xFC, 0x50, 0xFF, 0x55, 0xF8, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC3
        };

        public static byte[] call_fastcall_dword = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x44, 0x53, 0x56, 0x57, 0xC7, 0x45, 0xFC, 0xF0, 0xF0, 0xF0, 0xF0, 0x8B, 0x4D, 0x08, 0xFF, 0x55, 0xFC, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC2, 0x04, 0x00
        };

        //__fastcall Wrapper(DWORD param, DWORD param2)
        public static byte[] fastcall_dword_dword = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x4C, 0x53, 0x56, 0x57, 0x89, 0x55, 0xF8, 0x89, 0x4D, 0xFC, 0xC7, 0x45, 0xF4, 0xF0, 0xF0, 0xF0, 0xF0, 0x8B, 0x45, 0xF8, 0x50, 0x8B, 0x4D, 0xFC, 0x51, 0xFF, 0x55, 0xF4, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC3
        };
        
        public static byte[] call_fastcall_dword_dword = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x44, 0x53, 0x56, 0x57, 0xC7, 0x45, 0xFC, 0xF0, 0xF0, 0xF0, 0xF0, 0x8B, 0x55, 0x0C, 0x8B, 0x4D, 0x08, 0xFF, 0x55, 0xFC, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC2, 0x08, 0x00
        };

        //__fastcall Wrapper(DWORD param, DWORD param2, DWORD param3)
        public static byte[] fastcall_dword_dword_dword = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x4C, 0x53, 0x56, 0x57, 0x89, 0x55, 0xF8, 0x89, 0x4D, 0xFC, 0xC7, 0x45, 0xF4, 0xF0, 0xF0, 0xF0, 0xF0, 0x8B, 0x45, 0x08, 0x50, 0x8B, 0x4D, 0xF8, 0x51, 0x8B, 0x55, 0xFC, 0x52, 0xFF, 0x55, 0xF4, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC2, 0x04, 0x00
        };
        
        public static byte[] call_fastcall_dword_dword_dword = new byte[]
        {
            0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x44, 0x53, 0x56, 0x57, 0xC7, 0x45, 0xFC, 0xF0, 0xF0, 0xF0, 0xF0, 0x8B, 0x45, 0x10, 0x50, 0x8B, 0x55, 0x0C, 0x8B, 0x4D, 0x08, 0xFF, 0x55, 0xFC, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC2, 0x0C, 0x00
        };

        // ref: https://blogs.msdn.microsoft.com/winsdk/2015/02/09/c-and-fastcall-how-to-make-them-work-together-without-ccli-shellcode/
        public static byte[] ReplaceStubAddress(byte[] stub, IntPtr address)
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
