﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;

#if DEBUG
using log4net;
#endif

namespace X86.Interop
{
    public abstract partial class Structure : IStructure
    {
#if DEBUG
        private static readonly ILog Log = LogManager.GetLogger(typeof (Structure));
#endif

        private static readonly MemoryManager MemoryManager = new MemoryManager();

        /// <summary>
        /// If true, all exceptions such as <see cref="AccessViolationException"/>s will be thrown.
        /// By default, if marshalling fails, the default value is returned eg. 0, null
        /// </summary>
        public static bool ShouldThrowMarshalExceptions { get; set; }

        protected bool _isAllocated;
        protected readonly bool _isReference;

        protected Structure()
        {
            Allocate();
        }
        
        protected Structure(IntPtr baseAddress)
        {
            if (baseAddress == IntPtr.Zero)
            {
                string errMsg = string.Format("The base address of structure type \"{0}\" is IntPtr.Zero!", GetType());
                throw new ArgumentException(errMsg);
            }
            _isReference = true;
            BaseAddress = baseAddress;
        }

        ~Structure()
        {
            Dispose(false);
        }
        
        /// <summary>
        /// The structure's location in memory
        /// </summary>
        public IntPtr BaseAddress { get; set; }

        /// <summary>
        /// Returns the size of the structure in bytes
        /// </summary>
        public abstract int GetSize();

        public int GetDeepSize()
        {
            return GetDeepSizeInternal(new Dictionary<IntPtr, bool>() {{BaseAddress,true}});
        }

        private int GetDeepSizeInternal(IDictionary<IntPtr, bool> visitedStructs)
        {
            return GetSize()
                + VisitStructureProperties(visitedStructs)
                    .Sum(propertyInstance => propertyInstance.GetDeepSizeInternal(visitedStructs));
        }

        protected void Allocate()
        {
            if (BaseAddress != IntPtr.Zero)
            {
                throw new InvalidOperationException("Structure has already been allocated.");
            }

            int size = GetSize();
            // some structs have dynamic length. Once they've properly defined
            // their size it is their responsibility to call Allocate() if necessary
            if (size == 0)
                return;
            // allocate required amount of bytes
            BaseAddress = MemoryManager.Allocate(size);
            // set flag on success
            _isAllocated = true;
#if DEBUG
            Log.DebugFormat("{0} memory allocation at address 0x{3:x} for structure instance of type \"{1}\" size = {2} bytes.",
                            BaseAddress != IntPtr.Zero ? "Successful" : "Failed", GetType(), size, BaseAddress);
#endif
        }
        
        /// <summary>
        /// Frees the structure and all structures that it points to
        /// </summary>
        /// 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            //if (disposing)
            //{
            //    DisposeInternal(new Dictionary<IntPtr, bool>() { { BaseAddress, true } });
            //}
            if (BaseAddress != IntPtr.Zero && _isAllocated)
            {
                MemoryManager.Free(BaseAddress);
                BaseAddress = IntPtr.Zero;
            }
        }

        // recursive dispose? probably not the best idea (hence commenting it out)
        // but an interesting thought - for convenience-sake .. hmmm
        private void DisposeInternal(IDictionary<IntPtr, bool> visitedStructs)
        {
#if DEBUG
            Log.DebugFormat("Freeing structure instance of type \"{0}\" at address 0x{1:x} size = {2} bytes.",
                            GetType(), BaseAddress.ToHexString(), GetSize());
#endif
            foreach (var structProperty in VisitStructureProperties(visitedStructs))
            {
                structProperty.DisposeInternal(visitedStructs);
            }
            if (!HasVisited(this, visitedStructs) && BaseAddress != IntPtr.Zero)
                MemoryManager.Free(BaseAddress);
            BaseAddress = IntPtr.Zero;
        }

        protected IntPtr GetOffset(Int32 offset)
        {
            return BaseAddress.AddOffset(offset);
        }
        
        [HandleProcessCorruptedStateExceptions]
        protected Byte[] ReadBytes(Int32 offset, Int32 count)
        {
            Byte[] buffer = new Byte[count];
            try
            {
                Marshal.Copy(GetOffset(offset), buffer, 0, count);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("ReadBytes failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
            }
            return buffer;
        }

        protected void WriteBytes(Int32 offset, Byte[] value)
        {
            try
            {
                Marshal.Copy(value, 0, GetOffset(offset), value.Length);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("WriteBytes failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        protected byte ReadByte(Int32 offset)
        {
            try
            {
                return Marshal.ReadByte(GetOffset(offset));
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("ReadByte failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
                return 0;
            }
        }

        protected void WriteByte(Int32 offset, Byte value)
        {
            try
            {
                Marshal.WriteByte(GetOffset(offset), value);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("WriteByte failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        protected Int16 ReadInt16(Int32 offset)
        {
            try
            {
                return Marshal.ReadInt16(GetOffset(offset));
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("ReadInt16 failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
                return 0;
            }
        }

        protected void WriteInt16(Int32 offset, Int16 value)
        {
            try
            {
                Marshal.WriteInt16(GetOffset(offset), value);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("WriteInt16 failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        protected UInt16 ReadUInt16(Int32 offset)
        {
            try
            {
                return BitConverter.ToUInt16(ReadBytes(offset, sizeof (UInt16)), 0);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("ReadUInt16 failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
                return 0;
            }
        }

        protected UInt16[] ReadUInt16Array(Int32 offset, int length)
        {
            UInt16[] ret = new UInt16[length];
            for (int i = 0; i < length; i++)
                ret[i] = ReadUInt16(offset + (2 * i));
            return ret;
        }

        protected void WriteUInt16(Int32 offset, UInt16 value)
        {
            WriteBytes(offset, BitConverter.GetBytes(value));
        }

        protected void WriteUInt16Array(Int32 offset, UInt16[] value, int length)
        {
            for (int i = 0; i < length; i++)
                WriteUInt16(offset + (2 * i), value[i]);
        }

        [HandleProcessCorruptedStateExceptions]
        protected Int32 ReadInt32(Int32 offset)
        {
            try
            {
                return Marshal.ReadInt32(GetOffset(offset));
            }
            catch (Exception ex)
            {
#if DEBUG
                string errMsg = string.Format("ReadInt32 failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
                return 0;
            }
        }

        protected void WriteInt32(Int32 offset, Int32 value)
        {
            try
            {
                Marshal.WriteInt32(GetOffset(offset), value);
            }
            catch (Exception ex)
            {
#if DEBUG
                string errMsg = string.Format("WriteInt32 failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
            }
        }

        protected UInt32[] ReadUInt32Array(Int32 offset, int length)
        {
            UInt32[] ret = new UInt32[length];
            for (int i = 0; i < length; i++)
                ret[i] = ReadUInt32(offset + (4 * i));
            return ret;
        }

        [HandleProcessCorruptedStateExceptions]
        protected UInt32 ReadUInt32(Int32 offset)
        {
            return BitConverter.ToUInt32(ReadBytes(offset, sizeof(UInt32)), 0);
        }

        protected void WriteUInt32(Int32 offset, UInt32 value)
        {
            WriteBytes(offset, BitConverter.GetBytes(value));
        }

        protected void WriteUInt32Array(Int32 offset, UInt32[] value, int length)
        {
            for (int i = 0; i < length; i++)
                WriteUInt32(offset + (4 * i), value[i]);
        }

        [HandleProcessCorruptedStateExceptions]
        protected Int64 ReadInt64(Int32 offset)
        {
            try
            {
                return Marshal.ReadInt64(GetOffset(offset));
            }
            catch (Exception ex)
            {
#if DEBUG
                string errMsg = string.Format("ReadInt64 failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
                return 0;
            }
        }

        protected void WriteInt64(Int32 offset, Int64 value)
        {
            try
            {
                Marshal.WriteInt64(GetOffset(offset), value);
            }
            catch (Exception ex)
            {
#if DEBUG
                string errMsg = string.Format("WriteInt64 failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
            }
        }

        protected UInt64 ReadUInt64(Int32 offset)
        {
            return BitConverter.ToUInt64(ReadBytes(offset, sizeof(UInt64)), 0);
        }

        protected void WriteUInt64(Int32 offset, UInt64 value)
        {
            WriteBytes(offset, BitConverter.GetBytes(value));
        }

        [HandleProcessCorruptedStateExceptions]
        protected IntPtr ReadIntPtr(Int32 offset)
        {
            try
            {
                return Marshal.ReadIntPtr(GetOffset(offset));
            }
            catch (Exception ex)
            {
#if DEBUG
                string errMsg = string.Format("ReadIntPtr failed at offset {0:x8} @ {1}", offset, BaseAddress.AddOffset(offset).ToHexString());
                Log.Error(errMsg, ex);
#endif
                return IntPtr.Zero;
            }
        }

        protected bool TryReadIntPtr(Int32 offset, out IntPtr ptr)
        {
            try
            {
                ptr = ReadIntPtr(offset);
                return ptr != IntPtr.Zero;
            }
            catch (AccessViolationException)
            {
                ptr = IntPtr.Zero;
                return false;
            }
        }

        protected void WriteIntPtr(Int32 offset, IntPtr value)
        {
            try
            {
                Marshal.WriteIntPtr(GetOffset(offset), value);
            }
            catch (Exception ex)
            {
#if DEBUG
                string errMsg = string.Format("WriteIntPtr failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
            }
        }

        protected void WriteStructPointer(Int32 offset, object value)
        {
            var structure = value as IStructure;
            if (structure == null)
                throw new ArgumentException("Value does not implement IStructure, cannot write pointer.");
            WriteIntPtr(offset, value == null ? IntPtr.Zero : structure.BaseAddress);
        }


        protected void WriteStructPointer(Int32 offset, IStructure structure)
        {
            WriteIntPtr(offset, structure == null ? IntPtr.Zero : structure.BaseAddress);
        }

        protected String ReadAnsiString(Int32 offset)
        {
            try
            {
                string ret = Marshal.PtrToStringAnsi(GetOffset(offset));
                return ret == string.Empty ? null : ret;
            }
            catch (Exception ex)
            {
#if DEBUG
                string errMsg = string.Format("ReadAnsiString failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
                return null;
            }
        }

        protected void WriteAnsiString(Int32 offset, String value)
        {
            WriteBytes(offset, Encoding.ASCII.GetBytes(value));
        }

        protected string ReadUnicodeString(Int32 offset)
        {
            try
            {
                return Marshal.PtrToStringUni(GetOffset(offset));
            }
            catch (Exception ex)
            {
#if DEBUG
                string errMsg = string.Format("ReadUnicodeString failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
                Log.Error(errMsg, ex);
#endif
                if (ShouldThrowMarshalExceptions) throw;
                return null;
            }
        }

        protected void WriteUnicodeString(Int32 offset, String value)
        {
            WriteBytes(offset, Encoding.Unicode.GetBytes(value));
        }

        private bool AreSame(Type a, Type b)
        {
            if (a == b)
                return true;
            return a.IsSubclassOf(b) || b.IsSubclassOf(a);
        }

        protected IEnumerable<Structure> VisitStructureProperties(IDictionary<IntPtr, bool> visitedStructs)
        {
            PropertyInfo indexerProp = null;
            foreach (var p in GetType().GetProperties())
            {
                if (p.PropertyType.GetInterfaces().Any(I => I == typeof(IStructure)))
                {
                    if (p.GetIndexParameters().Length > 0)
                    {
                        indexerProp = p;
                        continue;
                    }
#if NET40
                    var propInstance = p.GetValue(this, null) as Structure;
#else
                    var propInstance = p.GetValue(this) as Structure;
#endif
                    if (propInstance == null || HasVisited(propInstance, visitedStructs))
                        continue;
                    yield return propInstance;
                }
            }
            if (indexerProp == null)
                yield break;
            var len = GetPropertyValue<int>(this,"Length");
            for (int i = 0; i < len; i++)
            {
                var item = indexerProp.GetValue(this, new object[] { i }) as Structure;
                if (item == null || HasVisited(item, visitedStructs))
                    continue;
                yield return item;
            }
        }

        private bool HasVisited(Structure structure, IDictionary<IntPtr, bool> visitedStructs)
        {
            if (structure.BaseAddress == IntPtr.Zero || visitedStructs.ContainsKey(structure.BaseAddress))
            {
                return true;
            }
            visitedStructs.Add(structure.BaseAddress, true);
            return false;
        }

        //TODO: move out
        public static T GetPropertyValue<T>(object source, string property)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sourceType = source.GetType();
            var sourceProperties = sourceType.GetProperties();
            var properties = sourceProperties
                .Where(s => s.Name.Equals(property));
            if (!properties.Any())
            {
                sourceProperties = sourceType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
                properties = sourceProperties.Where(s => s.Name.Equals(property));
            }

            if (properties.Any())
            {
                var propertyValue = properties
                    .Select(s => s.GetValue(source, null))
                    .FirstOrDefault();

                return propertyValue != null ? (T)propertyValue : default(T);
            }

            return default(T);
        }
    }
}
