using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace RTS.Service.Business
{
	public static class ApplicationCache
	{
		#region | CtxCacheKey |
		private static string CtxCacheKey(string cacheKey)
		{
			cacheKey += "__RTS__";
			return cacheKey;
		}
		#endregion

		#region | CacheManagerGet |
		public static X CacheManagerGet<X>(string cacheKey, Func<X> genFunc, Func<X, TimeSpan> ttlFunc,
			Func<X, bool> stillValidFunc = null)
		{
			cacheKey = CtxCacheKey(cacheKey);
			MemoryCache MC = MemoryCache.Default;
			object O = MC.Get(cacheKey);
			X Cached = (O is X) ? (X)O : default(X);
			if ( (O != null) && (Cached != null) && ((stillValidFunc == null) || stillValidFunc.Invoke(Cached)) )
				return Cached;
			Cached = genFunc.Invoke();
			TimeSpan TTL = ttlFunc.Invoke(Cached);
			object ToWrite = Cached;
			if ( TTL == TimeSpan.MaxValue )
				MC.Add(cacheKey, ToWrite, MemoryCache.InfiniteAbsoluteExpiration);
			else if ( TTL > TimeSpan.Zero )
				MC.Add(cacheKey, ToWrite, new DateTimeOffset(DateTime.UtcNow + TTL, TimeSpan.Zero));
			return Cached;
		}
		public static X CacheManagerGet<X>(string cacheKey, Func<X> genFunc, TimeSpan ttl,
			Func<X, bool> stillValidFunc = null)
		{
			X Cached = CacheManagerGet<X>(cacheKey, genFunc, V => ttl, stillValidFunc);
			return Cached;
		}
		public static X CacheManagerGet<X>(string cacheKey)
		{
			cacheKey = CtxCacheKey(cacheKey);
			MemoryCache MC = MemoryCache.Default;
			object O = MC.Get(cacheKey);
			X Cached = (O is X) ? (X)O : default(X);
			return Cached;
		}
		#endregion

		#region | CacheManagerSet |
		public static void CacheManagerSet(string cacheKey, object obj, TimeSpan ttl)
		{
			cacheKey = CtxCacheKey(cacheKey);
			MemoryCache MC = MemoryCache.Default;
			if ( ttl == TimeSpan.MaxValue )
				MC.Add(cacheKey, obj, MemoryCache.InfiniteAbsoluteExpiration);
			else if ( ttl > TimeSpan.Zero )
				MC.Add(cacheKey, obj, new DateTimeOffset(DateTime.UtcNow + ttl, TimeSpan.Zero));
			else
				MC.Remove(cacheKey);
		}
		#endregion

		#region | CacheManagerRemoveAll |
		/// <summary>
		/// Removes all the cache items.
		/// </summary>
		public static void CacheManagerRemoveAll()
		{
			MemoryCache MC = MemoryCache.Default;
			foreach ( KeyValuePair<string, object> Entry in MC.ToList() )
				MC.Remove(Entry.Key);
		}
		#endregion

		#region | CacheManagerRemove |
		public static void CacheManagerRemove(string cacheKey)
		{
			cacheKey = CtxCacheKey(cacheKey);
			MemoryCache MC = MemoryCache.Default;
			MC.Remove(cacheKey);
		}
		#endregion
	}
}
