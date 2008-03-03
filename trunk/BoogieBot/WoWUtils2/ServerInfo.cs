/*
 * Copyright ¸ 2005 Kele (fooleau@gmail.com)
 * This library is free software; you can redistribute it and/or 
 * modify it under the terms of the GNU Lesser General Public 
 * License version 2.1 as published by the Free Software Foundation
 * (the "LGPL").
 * This software is distributed on an "AS IS" basis, WITHOUT WARRANTY
 * OF ANY KIND, either express or implied.
 */

using System;

namespace Foole.WoW
{
	/// <summary>
	/// Server information for a realmlist
	/// </summary>
	public class ServerInfo
	{
		private string mName;
		private string mAddress;
		
		public ServerInfo(WoWReader wr)
		{
			wr.ReadUInt32(); // normal/pvp/rp
			wr.ReadByte(); // Colour
			mName = wr.ReadString();
			mAddress = wr.ReadString();
			wr.ReadSingle(); // Population
			wr.ReadByte(); // Char count
			wr.ReadByte(); // ?
			wr.ReadByte(); // ?
		}
		
		public ServerInfo(string Name, string Address)
		{
			mName = Name;
			mAddress = Address;
		}

		public void Write(WoWWriter ww)
		{
			ww.Write(1); // 0=normal, 1=pvp, 6=RP
			ww.Write((byte)0); // Colour. 0=brown 1=red, 2=grey/disabled
			ww.Write(mName);
			ww.Write(mAddress);
			ww.Write(0.0f); // Population (0.5 = low, 1=medium, 2=high)
			ww.Write((byte)0); // Number of characters
			ww.Write((byte)1); // If this is > 1 the server does not appear in the list
			ww.Write((byte)0); // Timezone?
		}

		public void Write(WoWWriter ww, string Username)
		{
			// TODO: Get the number of characters for Username
			ww.Write(1); // 0=normal, 1=pvp, 6=RP
			ww.Write((byte)0); // Colour. 0=brown 1=red, 2=grey/disabled
			ww.Write(mName);
			ww.Write(mAddress);
			ww.Write(0.0f); // Population (0.5 = low, 1=medium, 2=high)
			ww.Write((byte)1); // Number of characters
			ww.Write((byte)1); // If this is > 1 the server does not appear in the list
			ww.Write((byte)0); // Timezone?
		}
	}
}
