﻿#if DEBUG
using log4net;
#endif
using System;

namespace X86.Interop
{
    /// <summary>
    /// Patches a location in memory. See <see cref="Install()"/> <seealso cref="Uninstall()"/>
    /// </summary>
    public class Patch : IPatch
    {
#if DEBUG
        private static readonly ILog Log = LogManager.GetLogger(typeof(Patch));
#endif

        protected Action<X86Writer> _writeAsm;
        private readonly byte[] _rawBytes;
        private byte[] _oldBytes;


        public Patch(IntPtr address, byte[] rawBytes)
            : this(address)
        {
            Guard.ArgumentNotNull(nameof(rawBytes), rawBytes);
            _rawBytes = rawBytes;
        }

        public Patch(string dllName, Int32 offset, byte[] rawBytes)
            : this(DllHelper.GetOffset(dllName, offset))
        {
            Guard.ArgumentNotNullOrEmpty(nameof(dllName), dllName);
            Guard.ArgumentNotNull(nameof(rawBytes), rawBytes);
            _rawBytes = rawBytes;
        }

        public Patch(IntPtr address, Action<X86Writer> writeAsm)
            : this(address)
        {
            Guard.ArgumentNotNull(nameof(writeAsm), writeAsm);
            _writeAsm = writeAsm;
        }

        public Patch(string dllName, Int32 offset, Action<X86Writer> writeAsm)
            : this(DllHelper.GetOffset(dllName, offset))
        {
            Guard.ArgumentNotNullOrEmpty(nameof(dllName), dllName);
            Guard.ArgumentNotNull(nameof(writeAsm), writeAsm);
            _writeAsm = writeAsm;
        }

        protected Patch(IntPtr address)
        {
            Address = address;
        }

        ~Patch()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsInstalled)
                Uninstall();
        }
        
        public IntPtr Address { get; private set; }
        public bool IsInstalled { get; private set; }
        public byte[] PatchedBytes => _oldBytes;


        public void Install()
        {
            if (_oldBytes != null)
            {
                return;
            }

            byte[] bytes = _rawBytes ?? X86Asm.GetAsmBytes(Address, _writeAsm);
            if (bytes.Length == 0)
            {
                return;
            }

#if DEBUG
            Log.DebugFormat("Saving {0} bytes to {1}.", bytes.Length, Address.ToHexString());
#endif
            _oldBytes = new byte[bytes.Length];
            System.Runtime.InteropServices.Marshal.Copy(Address, _oldBytes, 0, bytes.Length);

            X86Asm.At(Address).Write(bytes);
            IsInstalled = true;
        }
        
        public void Uninstall()
        {
            if (_oldBytes == null)
            {
                throw new InvalidOperationException("Patch has not been installed.");
            }

            System.Runtime.InteropServices.Marshal.Copy(_oldBytes, 0, Address, _oldBytes.Length);
            _oldBytes = null;
            IsInstalled = false;
        }
    }
}
