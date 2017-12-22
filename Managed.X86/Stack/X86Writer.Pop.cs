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

namespace Managed.X86 {
	partial class X86Writer {
		public void Pop32(X86Register32 reg) {
			writer.Write(new byte[] { 0x8F });
			reg_emit32(0, reg);
		}
		public void Pop32(X86Address addr) {
			writer.Write(new byte[] { 0x8F });
			addr.Emit(writer, (X86Register8)0);
		}
        public void PopAd()
        {
            writer.Write(new byte[] { 0x60 });
        }
    }
}
