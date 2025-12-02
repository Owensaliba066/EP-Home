using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DataAccess.Repositories
{
    public class ItemsInMemoryRepository : IItemsRepository
    {
        private const string CacheKey = "BulkImportItems";
        private readonly IMemoryCache _cache;

        public ItemsInMemoryRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<List<IItemValidating>> GetAsync()
        {
            if (_cache.TryGetValue(CacheKey, out List<IItemValidating> items))
            {
                return Task.FromResult(items);
            }

            return Task.FromResult(new List<IItemValidating>());
        }

        public Task SaveAsync(List<IItemValidating> items)
        {
            _cache.Set(
                CacheKey,
                items,
                new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(20)
                });

            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            _cache.Remove(CacheKey);
            return Task.CompletedTask;
        }
    }
}
