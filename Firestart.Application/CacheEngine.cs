using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace Firestar
{
    public class CacheEngine
    {
        private const int CACHE_MAX_LENGTH = 10000;

        private readonly IMemoryCache _memoryCache = null;

        private IList<string> _entries = null;

        private bool? _isActivated = true;
        public bool IsActivated
        {
            get
            {
                return _isActivated ?? false;
            }
        }

        public CacheEngine(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _entries = new List<string>(CACHE_MAX_LENGTH);
        }

        #region Utils

        /// <summary>
        /// Gets the cached object with a key.
        /// </summary>
        /// <returns><c>true</c>, if cache object was gotten, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="cacheObject">Cache object.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public bool GetCacheObject(string key, out IpAddress cacheObject)
        {
            if (!IsActivated)
            {
                cacheObject = null;

                return false;
            }

            return _memoryCache.TryGetValue(key.ToLower(), out cacheObject);
        }

        /// <summary>
        /// Sets a cached object with a key.
        /// </summary>
        /// <returns>The cache object.</returns>
        /// <param name="key">Key.</param>
        /// <param name="cacheObject">Cache object.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IpAddress SetCacheObject(string key, IpAddress cacheObject, PostEvictionDelegate callback = null)
        {
            if (!IsActivated)
            {
                return cacheObject;
            }

            key = key.ToLower();

            lock (_entries)
            {
                _entries.Add(key);
            }

            return _memoryCache.Set(key, cacheObject, GetMemoryCacheEntryOptions(callback));
        }

        /// <summary>
        /// Clears the cache by key.
        /// </summary>
        /// <param name="key">Key.</param>
        public void ClearCache(string key)
        {
            key = key.ToLower();

            lock (_entries)
            {
                _entries.Remove(key);
            }
            _memoryCache.Remove(key);
        }

        /// <summary>
        /// Clears the cache where the key was found.
        /// </summary>
        /// <param name="key">Key.</param>
        public void ClearCacheContains(string key)
        {
            var entriesToDelete = _entries.Where(e => e.Contains(key.ToLower()));

            foreach (string entry in entriesToDelete)
            {
                _memoryCache.Remove(entry);
            }

            _entries = _entries.Except(entriesToDelete).ToList();
        }

        /// <summary>
        /// Clears all cache.
        /// </summary>
        public void ClearAllCache()
        {
            foreach (string entry in _entries)
            {
                _memoryCache.Remove(entry);
            }

            lock (_entries)
            {
                _entries = new List<string>(CACHE_MAX_LENGTH);
            }
        }

        /// <summary>
        /// Gets all entries in the cache.
        /// </summary>
        /// <returns>The all entries.</returns>
        public IList<string> GetAllEntries()
        {
            return _entries;
        }

        /// <summary>
        /// Enables the cache.
        /// </summary>
        public void EnableCache()
        {
            _isActivated = true;
        }

        /// <summary>
        /// Disables the cache.
        /// </summary>
        public void DisableCache()
        {
            _isActivated = false;
        }

        /// <summary>
        /// Generate memory cache entry options.
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <returns>Memory cache entry options</returns>
        private MemoryCacheEntryOptions GetMemoryCacheEntryOptions(PostEvictionDelegate callback)
        {
            var options = new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromHours(2)
            };

            options.RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                lock (_entries)
                {
                    _entries.Remove(key.ToString());
                }
            });

            if (callback is not null)
            {
                options.RegisterPostEvictionCallback(callback);
            }

            return options;
        }

        #endregion
    }
}