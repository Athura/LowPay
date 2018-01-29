using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;

namespace Lowpay.Framework.Business
{
	/// <summary>
	/// Extension methods for extracting IP address information from web requests,
	/// and testing for certain conditions.  These are in the Common layer instead
	/// of the Framework.UI.Web layer because the Logging framework has some
	/// dependency on these methods.
	/// </summary>
	public static class IPAddressExtensions
	{
		#region IsRFC1918/Loopback
		/// <summary>
		/// Determines if this IPv4 address is an RFC-1918 private local network
		/// address, i.e. originated within the same private network as the local
		/// machine.
		/// </summary>
		/// <param name="addr">IP Address to inspect.</param>
		/// <returns>True if this is a private (LAN) address.  False if it is an
		/// internet address.</returns>
		public static bool IsRFC1918(this IPAddress addr)
		{
			byte[] AddrBytes = addr.GetAddressBytes();

			// Allow the 10.*.*.* RFC-1918 local address block.
			if ( AddrBytes[0] == 10 )
				return true;

			// Allow the 192.168.*.* RFC-1918 address block.
			if ( (AddrBytes[0] == 192) && (AddrBytes[1] == 168) )
				return true;

			// Allow the 172.16.*.*-172.31.*.* RFC-1918 address block;
			// since the mask is half of one of the octets, this is
			// a bit more ugly-looking.
			if ( (AddrBytes[0] == 172) && ((AddrBytes[1] & 240) == 16) )
				return true;

			return false;
		}
		/// <summary>
		/// Determines if this is a "loopback" address, i.e. 127.0.0.1 / localhost.
		/// </summary>
		/// <param name="addr">IP Address to inspect.</param>
		/// <returns>True if this is a local address.  False if it is the address
		/// of any other machine.</returns>
		public static bool IsLoopback(this IPAddress addr)
		{
			return IPAddress.IsLoopback(addr);
		}
		/// <summary>
		/// Determines if this is either a "loopback" or a "private" address.
		/// Addresses that meet this criterion can be considered "internal" computers,
		/// while addresses that do not are "external."
		/// </summary>
		/// <param name="addr">IP Address to inspect.</param>
		/// <returns>True if this is either a local or private address, false if
		/// it is an internet address.</returns>
		public static bool IsRFC1918OrLoopback(this IPAddress addr)
		{
			return IPAddress.IsLoopback(addr) || addr.IsRFC1918();
		}
		#endregion

		#region TrueRemoteAddress
		private static IPAddress TrueRemoteAddress(string rawAddr, string xffHeader)
		{
			// If the address is not an RFC1918 address or a loopback address, then the connection
			// originated outside of the local network, and its header information cannot be trusted.
			// We have to assume this is the "true" remote address.
			IPAddress SrcAddr = IPAddress.Parse(rawAddr);
			if ( !SrcAddr.IsRFC1918OrLoopback() )
				return SrcAddr;

			// Check for an X-Forwarded-For HTTP header (from EventXL TI73120).  If present, then the
			// connection was forwarded through a local proxy, and we need to get the true remote
			// address from the remote proxy.  Forwarding through multiple proxies will cause this to
			// contain a stack of addresses (most recent last), through which we will trace the connection.
			if ( string.IsNullOrWhiteSpace(xffHeader) )
				return SrcAddr;
			Stack<string> XFF = new Stack<string>();
			foreach ( string X in xffHeader.Split(',') )
				XFF.Push(X);
			while ( XFF.Count > 0 )
			{
				// Get the next most recent address from the proxy stack.
				string X = XFF.Pop();

				// Try to parse the address.  If the header is mangled or we can't parse it for
				// any reason, return the last address we successfully parsed.
				try { SrcAddr = IPAddress.Parse(X.Trim()); }
				catch { return SrcAddr; }

				// If the address is within the same local network as the most recent hop, it
				// must be proceded by an unbroken chain of LAN addresses, and thus still within
				// our local network, and trustworthy, so we can follow the next hop (i.e. trace
				// to another reverse proxy within our LAN).  If the address is NOT within our
				// LAN, we cannot trust any X-Forwarded-For headers it provided, and thus we can
				// trace no further.
				if ( !SrcAddr.IsRFC1918OrLoopback() )
					return SrcAddr;
			}

			// When we run out of proxies to trace, return our best guess address.
			return SrcAddr;
		}
		/// <summary>
		/// Attempts to determine the true IP address of the remote host (like UserHostAddress),
		/// even if the connection was forwarded through a reverse proxy.  Reverse proxies such
		/// as our F5 hide the originating IP (the connection appears to be from the F5 machine
		/// instead), but they add an X-Forwarded-For HTTP header to hold the old address.  Note
		/// that any UA or proxy can insert, and forge, these HTTP headers, so we only trust
		/// values that appear to originate from a proxy within the local network.
		/// </summary>
		/// <param name="req">HttpRequest to inspect.</param>
		/// <returns>The address from which traffic came, ignoring a reverse proxy within our
		/// network.</returns>
		public static IPAddress TrueRemoteAddress(this HttpRequest req)
		{
			return TrueRemoteAddress(req.UserHostAddress, req.ServerVariables["HTTP_X_FORWARDED_FOR"]);
		}
		/// <summary>
		/// Attempts to determine the true IP address of the remote host (like UserHostAddress),
		/// even if the connection was forwarded through a reverse proxy.  Reverse proxies such
		/// as our F5 hide the originating IP (the connection appears to be from the F5 machine
		/// instead), but they add an X-Forwarded-For HTTP header to hold the old address.  Note
		/// that any UA or proxy can insert, and forge, these HTTP headers, so we only trust
		/// values that appear to originate from a proxy within the local network.
		/// </summary>
		/// <param name="req">HttpRequest to inspect.</param>
		/// <returns>The address from which traffic came, ignoring a reverse proxy within our
		/// network.</returns>
		public static IPAddress TrueRemoteAddress(this HttpRequestBase req)
		{
			return TrueRemoteAddress(req.UserHostAddress, req.ServerVariables["HTTP_X_FORWARDED_FOR"]);
		}
		#endregion
	}
}
