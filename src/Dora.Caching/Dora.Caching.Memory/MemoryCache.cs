﻿using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Dora.Caching.Memory
{
  public class MemoryCache : Cache
  {
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _options;
    public MemoryCache(string name, IMemoryCache cache, MemoryCacheEntryOptions options) : base(name)
    {
      Guard.ArgumentNotNull(cache, nameof(cache));
      Guard.ArgumentNotNull(options, nameof(options));
      _cache = cache;
      _options = options;
    }

    protected override Task  RemoveCoreAsync(string key)
    {
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
      _cache.Remove(key);
      return Task.CompletedTask;
    }

    protected override Task SetCoreAsync(string key, object value, CacheEntryOptions options)
    {
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
      MemoryCacheEntryOptions options2 = options!= null ?new MemoryCacheEntryOptions
      {
        AbsoluteExpiration = options.AbsoluteExpiration,
        AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
        SlidingExpiration = options.SlidingExpiration,
        Priority = (Microsoft.Extensions.Caching.Memory.CacheItemPriority)(int)options.Priority
      }:_options;
      _cache.Set(key, value, options2);
      return Task.CompletedTask;
    }

    protected override Task<CacheValue> GetCoreAsync(string key, Type valueType)
    {
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
      CacheValue cacheValue = _cache.TryGetValue(key, out object value)
        ? new CacheValue { Exists = true, Value = value }
        : CacheValue.NonExistent;
      return Task.FromResult(cacheValue);
    }
  }
}
