using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
#if DEBUG
using log4net;
#endif
using Managed.X86.Interop;

namespace D2XF.Game.Structures
{
    public abstract partial class Structure : IStructure
    {
#if DEBUG
        private static readonly ILog Log = LogManager.GetLogger(typeof (Structure));
#endif

        private static readonly MemoryManager MemoryManager = new MemoryManager();

        private bool _isAllocated;

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
        // but an interesting thought - for convenience-sake
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

        protected IntPtr GetOffset(UInt32 offset)
        {
            return BaseAddress.AddOffset(offset);
        }

        // TODO: revise
        protected IntPtr GetOffset(Int32 offset)
        {
            return (IntPtr)GetOffset(offset);
        }

        protected Byte[] ReadBytes(UInt32 offset, Int32 count)
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

        protected void WriteBytes(UInt32 offset, Byte[] value)
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
                throw ex;
            }
        } 

        protected Byte ReadByte(UInt32 offset)
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
                throw ex;
            }
        }

        protected void WriteByte(UInt32 offset, Byte value)
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
                throw ex;
            }
        }

        protected Int16 ReadInt16(UInt32 offset)
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
                throw ex;
            }
        }

        protected void WriteInt16(UInt32 offset, Int16 value)
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
                throw ex;
            }
        }

        protected UInt16 ReadUInt16(UInt32 offset)
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
                throw ex;
            }
        }

        protected UInt16[] ReadUInt16Array(UInt32 offset, int length)
        {
            UInt16[] ret = new UInt16[length];
            for (uint i = 0; i < length; i++)
                ret[i] = ReadUInt16(offset + (2 * i));
            return ret;
        }

        protected void WriteUInt16(UInt32 offset, UInt16 value)
        {
            WriteBytes(offset, BitConverter.GetBytes(value));
        }

        protected void WriteUInt16Array(UInt32 offset, UInt16[] value, int length)
        {
            for (uint i = 0; i < length; i++)
                WriteUInt16(offset + (2 * i), value[i]);
        }

        protected Int32 ReadInt32(UInt32 offset)
        {
            try
            {
                return Marshal.ReadInt32(GetOffset(offset));
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("ReadInt32 failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                throw ex;
            }
        }

        protected void WriteInt32(UInt32 offset, Int32 value)
        {
            try
            {
                Marshal.WriteInt32(GetOffset(offset), value);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("WriteInt32 failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                throw ex;
            }
        }

        protected UInt32[] ReadUInt32Array(UInt32 offset, int length)
        {
            UInt32[] ret = new UInt32[length];
            for (uint i = 0; i < length; i++)
                ret[i] = ReadUInt32(offset + (4 * i));
            return ret;
        }

        protected UInt32 ReadUInt32(UInt32 offset)
        {
            return BitConverter.ToUInt32(ReadBytes(offset, sizeof(UInt32)), 0);
        }

        protected void WriteUInt32(UInt32 offset, UInt32 value)
        {
            WriteBytes(offset, BitConverter.GetBytes(value));
        }

        protected void WriteUInt32Array(UInt32 offset, UInt32[] value, int length)
        {
            for (uint i = 0; i < length; i++)
                WriteUInt32(offset + (4 * i), value[i]);
        }

        protected Int64 ReadInt64(UInt32 offset)
        {
            try
            {
                return Marshal.ReadInt64(GetOffset(offset));
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("ReadInt64 failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                throw ex;
            }
        }

        protected void WriteInt64(UInt32 offset, Int64 value)
        {
            try
            {
                Marshal.WriteInt64(GetOffset(offset), value);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("WriteInt64 failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                throw ex;
            }
        }

        protected UInt64 ReadUInt64(UInt32 offset)
        {
            return BitConverter.ToUInt64(ReadBytes(offset, sizeof(UInt64)), 0);
        }

        protected void WriteUInt64(UInt32 offset, UInt64 value)
        {
            WriteBytes(offset, BitConverter.GetBytes(value));
        }

        protected IntPtr ReadIntPtr(UInt32 offset)
        {
            try
            {
                return Marshal.ReadIntPtr(GetOffset(offset));
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("ReadIntPtr failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                throw ex;
            }
        }

        protected bool TryReadIntPtr(UInt32 offset, out IntPtr ptr)
        {
            ptr = ReadIntPtr(offset);
            return ptr != IntPtr.Zero;
        }

        protected void WriteIntPtr(UInt32 offset, IntPtr value)
        {
            try
            {
                Marshal.WriteIntPtr(GetOffset(offset), value);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("WriteIntPtr failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                throw ex;
            }
        }

        protected void WriteStructPointer(UInt32 offset, object value)
        {
            var structure = value as IStructure;
            if (structure == null)
                throw new ArgumentException("Value does not implement IStructure, cannot write pointer.");
            WriteIntPtr(offset, value == null ? IntPtr.Zero : structure.BaseAddress);
        }


        protected void WriteStructPointer(UInt32 offset, IStructure structure)
        {
            WriteIntPtr(offset, structure == null ? IntPtr.Zero : structure.BaseAddress);
        }

            protected String ReadAnsiString(UInt32 offset)
        {
            try
            {
                string ret = Marshal.PtrToStringAnsi(GetOffset(offset));
                return ret == string.Empty ? null : ret;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("ReadAnsiString failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                throw ex;
            }
        }

        protected void WriteAnsiString(UInt32 offset, String value)
        {
            WriteBytes(offset, Encoding.ASCII.GetBytes(value));
        }

        protected String ReadUnicodeString(UInt32 offset)
        {
            try
            {
                return Marshal.PtrToStringUni(GetOffset(offset));
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("ReadUnicodeString failed at offset {0}", BaseAddress.AddOffset(offset).ToHexString());
#if DEBUG
                Log.Error(errMsg, ex);
#endif
                throw ex;
            }
        }

        protected void WriteUnicodeString(UInt32 offset, String value)
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
                    var propInstance = p.GetValue(this) as Structure;
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
