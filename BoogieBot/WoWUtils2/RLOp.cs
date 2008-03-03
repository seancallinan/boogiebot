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
	/// Realmlist OpCodes
	/// </summary>
	public enum RLOp
	{
		AUTH_LOGON_CHALLENGE = 0x00,
		AUTH_LOGON_PROOF = 0x01,
		AUTH_RECONNECT_CHALLENGE = 0x02,
		AUTH_RECONNECT_PROOF = 0x03,
		REALM_LIST = 0x10,
        SURVEY = 48,
	}
}
