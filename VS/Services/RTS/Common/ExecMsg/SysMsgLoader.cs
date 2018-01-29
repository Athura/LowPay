using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace RTS.Service.Common
{
	/// <summary>
	/// Loads textual content for ExecMsgs based on
	/// a MessageID (see SysMsgCode).
	/// </summary>
	public static class SysMsgLoader
	{
		#region Cache
		private static object _CacheLock = new object();
		private static Dictionary<int, string> _Cache = null;
		private static Dictionary<int, string> Cache
		{
			get
			{
				lock ( _CacheLock )
				{
					if ( _Cache != null )
						return _Cache;

					_Cache = new Dictionary<int, string>();

					// SUENA: 2010-09-28 16:29 - Simplified Cache loading: just pull the message
					// text from the Description attribute on each SysMsgCode value.
					foreach ( FieldInfo FI in typeof(SysMsgCode).GetFields() )
					{
						SysMsgCode Code = default(SysMsgCode);
						if ( Enum.TryParse<SysMsgCode>(FI.Name, out Code) )
						{
							DescriptionAttribute DA = FI.GetAttribute<DescriptionAttribute>();
							string Desc = ((DA != null) && (DA.Description != null))
								? DA.Description.Trim()
								: ("Missing message text for SysMsgCode \"" + FI.Name + "\".");
							_Cache[(int)Code] = Desc;
						}
					}

					return _Cache;
				}
			}
		}
		#endregion

		#region GetMessageContent
		/// <summary>
		/// Gets the message text for a given Message ID.
		/// </summary>
		public static string GetMessageContent(int messageID)
		{
			// SUENA: 2010-09-28 16:30 - Removed multi-culture code, since there was nowhere
			// to store translations, anyway (YAGNI).
			string Msg = string.Empty;
			Cache.TryGetValue(messageID, out Msg);
			return Msg;
		}
		/// <summary>
		/// Gets the message text for a given Message ID.
		/// </summary>
		public static string GetMessageContent(SysMsgCode messageID)
		{
			return GetMessageContent((int)messageID);
		}
		#endregion
	}
}
