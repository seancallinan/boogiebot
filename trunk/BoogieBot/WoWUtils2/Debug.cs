/*
 * Copyright ¸ 2005 Kele (fooleau@gmail.com)
 * This library is free software; you can redistribute it and/or 
 * modify it under the terms of the GNU Lesser General Public 
 * License version 2.1 as published by the Free Software Foundation
 * (the "LGPL").
 * This software is distributed on an "AS IS" basis, WITHOUT WARRANTY
 * OF ANY KIND, either express or implied.
 */
// created on 03/23/2004 at 13:35
using System;
using System.IO;

// Version 1.01 - Can now write to a TextWriter
namespace Foole.Utils
{
	public class Debug
	{
	
		public static void DumpBuffer(byte[] buffer)
		{
            // If no logtype was specified, we default it to NetworkComms
			DumpBuffer(buffer, BoogieBot.Common.LogType.NeworkComms);
		}

        public static void DumpBuffer(byte[] buffer, BoogieBot.Common.LogType lt)
        {
            //DumpBuffer(buffer, Length, Console.Out);
            StringWriter sw = new StringWriter();
            DumpBuffer(buffer, buffer.Length, sw);
            BoogieBot.Common.BoogieCore.Log(lt, "{0}", sw.ToString());
        }

		public static void DumpBuffer(byte[] buffer, int Length, TextWriter tw)
		{
			int Count;
			int j = 0;
			lock(tw)
			{
				while ( true ) 
				{
					Count = Length - j;
					if (Count > 16) 
						Count = 16;
					else if (Count < 1)
						break;
	
					tw.Write ("{0:X8} - ", j);
	
					for (int i = 0; i < 16; ++i)
					{
						if ( i < Count )
							tw.Write ("{0:X2} ", buffer[j + i]);
						else
							tw.Write ("   ");
					}
					//  Print the printable characters, or a period if unprintable.
					for (int i = 0; i < Count; ++i)
					{
						byte Current = buffer[j + i];
						if ((Current < 32) || (Current > 126))
							tw.Write (".");
						else
							tw.Write ((char) Current);
					}
					tw.Write (Environment.NewLine);
					j += 16;
				}
			}
		}
	}
}
