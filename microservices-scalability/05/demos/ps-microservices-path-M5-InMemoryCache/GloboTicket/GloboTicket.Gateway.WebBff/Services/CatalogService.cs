using GloboTicket.Gateway.Shared.Event;
using GloboTicket.Gateway.WebBff.Extensions;
using GloboTicket.Gateway.WebBff.Models;
using GloboTicket.Gateway.WebBff.Url;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GloboTicket.Gateway.WebBff.Services
{
    public class CatalogService: ICatalogService
    {
        private readonly HttpClient client;
        private IMemoryCache cache;

        public CatalogService(HttpClient client, IMemoryCache memoryCache)
        {
            this.client = client;
            this.cache = memoryCache;
        }

        public async Task<List<EventDto>> GetAllEvents()
        {
            return await CacheGetOrCreateAsync<List<EventDto>>(EventCatalogOperations.GetAllEvents());
        }

        public async Task<T> CacheGetOrCreateAsync<T>(string url)
        {
            var cacheEntry = await
                cache.GetOrCreateAsync(url, async(entry) =>
                {
                    entry.SlidingExpiration = TimeSpan.FromSeconds(3);

                    var response = await client.GetAsync(url);
                    Console.WriteLine($"API Call: Data from {url}");

                    return await response.ReadContentAs<T>();
                });

            //other things you can do
            //var cacheEntry = cache.Get<T?>(url);
            //cache.Remove(url);
            //cache.Compact(.33);  

            return cacheEntry;
        }

        private async Task<T> SimpleCacheGetOrCreateAsync<T>(string url)
        {
            T data;

            if (!cache.TryGetValue(url, out data))
            {
                var response = await client.GetAsync(url);
                data = await response.ReadContentAs<T>();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(10))
                    .SetPriority(CacheItemPriority.NeverRemove)
                    //.RegisterPostEvictionCallback
                    .SetSlidingExpiration(TimeSpan.FromSeconds(3));

                cache.Set(url, data, cacheEntryOptions);


                Console.WriteLine($"API Call: Data from {url}");

            }
            else
            {
                Console.WriteLine($"CACHE: Data from cache with key: {url}");
            }

            return data;
        }

        public async Task<List<EventDto>> GetEventsPerCategory(Guid categoryId)
        {
            //var response = await client.GetAsync(EventCatalogOperations.GetEventsPerCategory(categoryId));
            //return await response.ReadContentAs<List<EventDto>>();

            return await CacheGetOrCreateAsync<List<EventDto>>(EventCatalogOperations.GetEventsPerCategory(categoryId));
        }

        public async Task<EventDto> GetEventById(Guid eventId)
        {
            //var response = await client.GetAsync(EventCatalogOperations.GetEventById(eventId));
            //return await response.ReadContentAs<EventDto>();

            return await CacheGetOrCreateAsync<EventDto>(EventCatalogOperations.GetEventById(eventId));
        }

        public async Task<List<CategoryDto>> GetAllCategories()
        {
            //var response = await client.GetAsync(EventCatalogOperations.GetAllcategories());
            //return await response.ReadContentAs<List<CategoryDto>>();

            return await CacheGetOrCreateAsync<List<CategoryDto>>(EventCatalogOperations.GetAllcategories());
        }

    
    }
}
