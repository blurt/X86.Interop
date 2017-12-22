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
		public void LockPrefix() {
			writer.Write(new byte[] { 0xF0 });
		}
	}
}
