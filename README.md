# X86.Interop
A .NET library to:
- write X86 asm to memory
- patch native dlls at runtime, intercept execution
- create wrappers for complex unmanaged structures

Supports net40 and net45
P/Invokes kernel32


# Writing to memory

```
// Write a no op instruction at memory address 0xFFFF8000
X86Asm.At(0xFFFF8000).Write(writer => writer.Nop());
// or use raw bytes
X86Asm.At(0xFFFF8000).Write(new byte[] { 0x90 });


// Write a new block of asm, managed by our application
var asm = X86Asm.Create(writer =>
{
	writer.Add32(X86Register32.EAX, 1);
	writer.Cmp32(X86Register32.EAX, 1);
});
Console.WriteLine($"Asm block allocated at {asm.Address}");
asm.Dispose(); // free asm block

```

# Patching native code

```
// Patch a location relative to a native dll:
var patch = new CallPatch("myNativeDll.dll", 0x4FF2,
	writer =>
	{
		// See Marshal.GetFunctionPointerForDelegate to acquire a pointer to a .NET delegate
		writer.PushAd(); //push all registers to stack
		// TODO: write intercept asm .. don't mess up the stack!
		writer.PopAd(); // restore registers
	});

// or specify exact address:
var patch = new CallPatch(0x12341234, writer => {})

patch.Install();   // installs the patch
patch.Uninstall(); // swaps back original asm
```


# Marshalling complex structures

Traditional marshalling requires you to marshal over the entire unmanaged structure. There are also limitations with marshalling pointers to nested structures.
Here we can create c# wrappers for the unmanaged structures. The class is opaque -- values are marshalled over as needed.

```

public class MyUnmanagedStructure : X86.Interop.Structure
{
	// creates an instance of the structure, which will be allocated and managed by the base class
	public MyUnmanagedStructure() : base() { }

	// reference an unmanaged structure at a particular location in memory
	public MyUnmanagedStructure(IntPtr baseAddress) : base(baseAddress) { }

	// WORD at 0x00
	public UInt16 My16BitInteger
	{
		get { return ReadUInt16(0x00); } // read 16 bit integer at offset 0x00
		set { WriteUInt16(0x00, value); }
	}

	// DWORD at 0x02 -- pointer to NestedStructure
	// a nice pattern emerges to dynamically create a reference to a nested unmanaged structure
	public NestedStructure NestedStruct
	{
		get { return TryReadIntPtr(0x02, out IntPtr nestedStructPtr) ? new NestedStruct(nestedStructPtr) : null; }
		set { WriteStructPointer(Offset_pArray, value); }
	}

	// DWORD at 0x06 -- pointer to an array of structures stored contiguously in memory
	public Array<OtherStruct> Items
	{
		get { return TryReadIntPtr(0x06, out IntPtr arrayPtr) ? new Array<OtherStruct>(arrayPtr) { Length = ItemsCount } : null; }
		set { WriteStructPointer(Offset_pArray, value); }
	}

	// DWORD at 0x10 -- length of above array
	public UInt32 ItemsCount
	{
		get { return ReadUInt32(0x10); } // read 16 bit integer at offset 0x00
		set { WriteUInt32(0x10, value); }
	}

	// also have type PointerArray<T> for an array-of-pointers that point to structures of type T

	public override int GetSize() { return 0x14 * sizeof(byte); }
}
```