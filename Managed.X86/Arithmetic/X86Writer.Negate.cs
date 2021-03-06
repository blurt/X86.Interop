/*
 * (c) 2008 The Managed.X86 Project
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Alex Lyman (<mailto:mail.alex.lyman@gmail.com>)
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace X86.Interop {
	partial class X86Writer {
		public void Negate8(X86Register8 dest) {
			writer.Write(new byte[] { 0xF6 });
			reg_emit8((X86Register8)3, dest);
		}
		public void Negate16(X86Register16 dest) {
			writer.Write(new byte[] { 0x66, 0xF7 });
			reg_emit16((X86Register16)3, dest);
		}
		public void Negate32(X86Register32 dest) {
			writer.Write(new byte[] { 0xF7 });
			reg_emit32((X86Register32)3, dest);
		}

		public void Negate8(X86Address dest) {
			writer.Write(new byte[] { 0xF6 });
			dest.Emit(writer, (X86Register8)3);
		}
		public void Negate16(X86Address dest) {
			writer.Write(new byte[] { 0x66, 0xF7 });
			dest.Emit(writer, (X86Register8)3);
		}
		public void Negate32(X86Address dest) {
			writer.Write(new byte[] { 0xF7 });
			dest.Emit(writer, (X86Register8)3);
		}
	}
}
