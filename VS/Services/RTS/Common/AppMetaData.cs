using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace RTS.Services.Common
{
	public static class AppMetaData
	{
		public const string DEV_VERSION = "0.0.0.0";

		private static string AttrSearch<TAttr>(Func<TAttr, string> prop)
		{
			return new Assembly[]
				{
					Assembly.GetEntryAssembly(),
					Assembly.GetCallingAssembly(),
					Assembly.GetExecutingAssembly()
				}
				.Concat(AppDomain.CurrentDomain.GetAssemblies())
				.Where(A => A != null)
				.SelectMany(A => A.GetCustomAttributes(typeof(TAttr), true).OfType<TAttr>())
				.Select(prop)
				.Where(S => !string.IsNullOrWhiteSpace(S))
				.FirstOrDefault();
		}

		private static object LockObject = new object();

		private static string _Version = null;
		public static string Version
		{
			get
			{
				lock ( LockObject )
				{
					if ( _Version == null )
						_Version = AttrSearch<AssemblyFileVersionAttribute>(A => A.Version)
							?? DEV_VERSION;
					return _Version;
				}
			}
		}

		private static string _Copyright = null;
		public static string Copyright
		{
			get
			{
				lock ( LockObject )
				{
					if ( _Copyright == null )
						_Copyright = AttrSearch<AssemblyCopyrightAttribute>(A => A.Copyright)
							?? "Copyright © Experient " + DateTime.Now.Year.ToString() + ". All Rights Reserved.";
					return _Copyright;
				}
			}
		}

		public static IEnumerable<string> StatusCodes
		{
			get
			{
				List<string> VersionParts = new string[]
				{
					Version
				}.ToList();

				string HostId = Regex.Replace(Environment.MachineName, "[^0-9]", "");
				if ( !string.IsNullOrEmpty(HostId) )
					VersionParts.Add("H" + HostId);

				return VersionParts;
			}
		}
	}
}
