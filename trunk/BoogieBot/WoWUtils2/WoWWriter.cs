/*
 * Copyright ¸ 2005 Kele (fooleau@gmail.com)
 * This library is free software; you can redistribute it and/or 
 * modify it under the terms of the GNU Lesser General Public 
 * License version 2.1 as published by the Free Software Foundation
 * (the "LGPL").
 * This software is distributed on an "AS IS" basis, WITHOUT WARRANTY
 * OF ANY KIND, either express or implied.
 */
// created on 03/26/2004 at 15:46
using System;
using System.IO;
using System.Text;

namespace Foole.WoW
{
	public class WoWWriter : BinaryWriter
	{
		public WoWWriter(): base(new MemoryStream())
		{
		}

		public WoWWriter(OpCode Operation): base(new MemoryStream())
		{
			Write((UInt32)Operation);
		}

		public WoWWriter(Stream fs) : base(fs)
		{
		}

		public override void Write(string Text)
		{
			if (Text != null) Write(Encoding.Default.GetBytes(Text));
			Write((byte)0); // String terminator
		}

		public void Write(WoWWriter ww)
		{
			this.Write(ww.ToArray());
		}
		
		public byte[] ToArray()
		{
			return ((MemoryStream)BaseStream).ToArray();
		}

		public static implicit operator byte[] (WoWWriter ww)
		{
			return ww.ToArray();
		}
	}
}
