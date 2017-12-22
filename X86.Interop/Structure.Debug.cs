using Managed.X86.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace D2XF.Game.Structures
{
    public abstract partial class Structure
    {
        /// <summary>
        /// Returns a deep copy of the structure (currently not used)
        /// </summary>
        //public Structure Copy()
        //{
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    var ret = CopyDeep(new Dictionary<IntPtr, IList<Structure>>(), 0);
        //    stopwatch.Stop();
        //    Log.DebugFormat("Successfully copied structure of type \"{0}\". Total duration = {1}ms",
        //        GetType(), stopwatch.ElapsedMilliseconds);
        //    return ret;
        //}

        //private Structure CopyDeep(IDictionary<IntPtr, IList<Structure>> processedItems, int depth)
        //{
        //    var ret = CopyShallow();

        //    Log.DebugFormat("{0}Copied object type \"{1}\" from {2} to {3}.",
        //                    new string('\t', depth), GetType(), BaseAddress.ToHexString(), ret.BaseAddress.ToHexString());

        //    //in order to avoid cycles (=>stackoverflows) we add the
        //    //instance to our processed structs list
        //    if (processedItems.ContainsKey(BaseAddress))
        //        processedItems[BaseAddress].Add(ret);
        //    else
        //        processedItems.Add(BaseAddress, new List<Structure>() { ret });

        //    //copy property values from this instance to ret
        //    CopyPublicProperties(ret, processedItems, depth);
        //    return ret;
        //}

        //protected virtual void CopyPublicProperties(Structure copy, IDictionary<IntPtr, IList<Structure>> processedItems, int depth)
        //{
        //    //reflect on each property in order to
        //    //recursively copy properties with type Structure
        //    Type structType = GetType();
        //    PropertyInfo indexerProp = null;
        //    foreach (var p in structType.GetProperties())
        //    {
        //        //copy structure property
        //        if (p.PropertyType.IsSubclassOf(typeof(Structure)))
        //        {
        //            if (p.GetIndexParameters().Length > 0)
        //            {
        //                indexerProp = p;
        //                continue;
        //            }

        //            var propInstance = p.GetValue(this) as Structure;
        //            if (propInstance == null)
        //                continue;
        //            Log.DebugFormat("{0}Copying property \"{1}\"", new string('\t', depth), p.Name);
        //            p.SetValue(copy, CopyPropertyStruct(propInstance, processedItems, depth));
        //        }
        //    }
        //    if (indexerProp == null)
        //        return;
        //    var len = GetPropertyValue<int>(this, "Length");
        //    Log.DebugFormat("{0}Copying indexer for object type \"{1}\". Length={2}", new string('\t', depth), structType, len);
        //    if (len > 0)
        //    {
        //        var indexerType = indexerProp.GetValue(this, new object[] { 0 }).GetType();
        //        bool deepCopy = indexerType.IsSubclassOf(typeof(Structure));
        //        for (int i = 0; i < len; i++)
        //        {
        //            var item = indexerProp.GetValue(this, new object[] { i });
        //            indexerProp.SetValue(copy,
        //                deepCopy ? CopyPropertyStruct(item as Structure, processedItems, depth) : item,
        //                new object[] { i });
        //        }
        //    }
        //    Log.DebugFormat("{0}Done copying indexer \"{1}\"", new string('\t', depth), indexerProp.Name);
        //}

        //protected virtual Structure CopyShallow()
        //{
        //    throw new NotImplementedException();
        //    int size = GetSize();
        //    Type structType = GetType();

        //    //allocate required space for the structure
        //    IntPtr copybuffer = MemoryManager.Allocate(size);
        //    //Log.DebugFormat("{0} memory allocation at address {3} for structure instance of type \"{1}\" size = {2} bytes.",
        //    //                copybuffer == IntPtr.Zero ? "Successful" : "Failed", GetType(), size, copybuffer);
        //    //copy over the structure data
        //    //TODO
        //    //Kernel32.CopyMemory(copybuffer, BaseAddress, (uint)size);
        //    //instantiate a new structure with base address pointing to copybuffer
        //    object[] parameter = new object[1];
        //    parameter[0] = copybuffer;
        //    object instance =
        //       Activator.CreateInstance(structType, parameter);
        //    return instance as Structure;
        //}

        //protected Structure CopyPropertyStruct(Structure instance, IDictionary<IntPtr, IList<Structure>> processedItems, int depth)
        //{
        //    var instanceType = instance.GetType();
        //    if (processedItems.ContainsKey(instance.BaseAddress))
        //    {
        //        var processedInstance =
        //            processedItems[instance.BaseAddress].FirstOrDefault(s => AreSame(s.GetType(), instanceType));
        //        if (processedInstance != null)
        //        {
        //            Log.DebugFormat("{0}Already copied type \"{1}\" from address 0x{2}",
        //                            new string('\t', depth), instance.GetType(), instance.BaseAddress.ToHexString());
        //            return processedInstance;
        //        }
        //    }
        //    return instance.CopyDeep(processedItems, ++depth);
        //}

        //public void ValidateCopy(Structure copy, IList<IntPtr> processedItems, int depth)
        //{
        //    processedItems.Add(BaseAddress);
        //    if (GetType() != copy.GetType())
        //        throw new ArgumentException("Structures were not of same type.");
        //    Type structType = GetType();
        //    foreach (var p in structType.GetProperties())
        //    {
        //        if (p.Name == "BaseAddress")
        //            continue;
        //        if (p.PropertyType.IsSubclassOf(typeof(Structure)))
        //        {
        //            if (p.GetIndexParameters().Length > 0)
        //            {
        //                var len = GetPropertyValue<int>(this,"Length");
        //                var len2 = GetPropertyValue<int>(copy, "Length");
        //                if (len != len2)
        //                {
        //                    Log.ErrorFormat("{0}!!!!!! INDEXER LENGTH ARE NOT EQUAL", new string('\t', depth));
        //                }
        //                for (int i = 0; i < len; i++)
        //                {
        //                    var indexInstance = p.GetValue(this, new object[] { i }) as Structure;
        //                    var indexCopy = p.GetValue(copy, new object[] { i }) as Structure;
        //                    CompareStructs(indexInstance, indexCopy, processedItems, p, depth);
        //                }
        //                continue;
        //            }
        //            Log.DebugFormat("{0}[STRUCTURE] checking property \"{1}\" on type \"{2}\"", new string('\t', depth), p.Name, structType);
        //            var propInstance = p.GetValue(this) as Structure;
        //            var propCopy = p.GetValue(copy) as Structure;
        //            CompareStructs(propInstance, propCopy, processedItems, p, depth);

        //        }
        //        else
        //        {
        //            Log.DebugFormat("{0}[VALUE] checking property \"{1}\" on type \"{2}\"", new string('\t', depth), p.Name, structType);
        //            try
        //            {
        //                if (p.GetIndexParameters().Length > 0)
        //                {
        //                    var len = GetPropertyValue<int>(this,"Length");
        //                    var len2 = GetPropertyValue<int>(copy,"Length");
        //                    if (len != len2)
        //                    {
        //                        Log.ErrorFormat("{0}!!!!!! INDEXER LENGTH ARE NOT EQUAL", new string('\t', depth));
        //                    }
        //                    for (int i = 0; i < len; i++)
        //                    {
        //                        var indexInstance = p.GetValue(this, new object[] { i });
        //                        var indexCopy = p.GetValue(copy, new object[] { i });
        //                        if (!CompareObjects(indexInstance, indexCopy))
        //                        {
        //                            Log.ErrorFormat("{0}Type: {1} | PropertyName: {2} | ExpectedValue: {3} | Value: {4} | InstanceAddr={5} | CopyAddr={6}",
        //                                            new string('\t', depth), GetType(), p.Name, indexInstance, indexCopy, BaseAddress, copy.BaseAddress);
        //                        }
        //                    }
        //                    continue;
        //                }

        //                object selfValue = structType.GetProperty(p.Name).GetValue(this, null);
        //                object toValue = structType.GetProperty(p.Name).GetValue(copy, null);


        //                if (!CompareObjects(selfValue, toValue))
        //                {
        //                    Log.ErrorFormat("{0}Type: {1} | PropertyName: {2} | ExpectedValue: {3} | Value: {4} | InstanceAddr={5} | CopyAddr={6}",
        //                                    new string('\t', depth), GetType(), p.Name, selfValue, toValue, BaseAddress, copy.BaseAddress);
        //                }
        //            }
        //            catch (Exception) { Log.ErrorFormat("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!Failed checking property \"{0}\" on type \"{1}\"", p.Name, structType); }
        //        }
        //    }
        //    Log.DebugFormat("+++Validated {0}++++", structType);
        //}

        //private bool CompareObjects(object selfValue, object toValue)
        //{
        //    if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        //private void CompareStructs(Structure propInstance, Structure propCopy, IList<IntPtr> processedItems, PropertyInfo p, int depth)
        //{
        //    if (propInstance == null && propCopy != null)
        //        Log.ErrorFormat("Structures are different.\nType:{0}\nProperty Name:{1}\nExpected Value:null\nValue:{2}", GetType(), p.Name, p.GetValue(propCopy));
        //    if (propInstance != null && propCopy == null)
        //        Log.ErrorFormat("Structures are different.\nType:{0}\nProperty Name:{1}\nExpected Value:{2}\nValue:null", GetType(), p.Name, p.GetValue(this));
        //    if (propInstance == null)
        //        return;
        //    if (processedItems.Contains(propInstance.BaseAddress))
        //        return;

        //    Log.DebugFormat("Validating \"{0}\"", p.Name);
        //    propInstance.ValidateCopy(propCopy, processedItems, ++depth);
        //    Log.DebugFormat("Validated \"{0}\"", p.Name);
        //}

        //public static void CopyIndexer(PropertyInfo indexerProperty, object source, object dest)
        //{
        //    var len = GetPropertyValue<int>(source,"Length");
        //    for (int i = 0; i < len; i++)
        //    {
        //        var item = indexerProperty.GetValue(source, new object[] { i });
        //        indexerProperty.SetValue(dest, item, new object[] { i });
        //    }
        //}

    }
}
