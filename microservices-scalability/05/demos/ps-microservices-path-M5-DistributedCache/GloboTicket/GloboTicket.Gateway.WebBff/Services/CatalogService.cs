using GloboTicket.Gateway.Shared.Event;
using GloboTicket.Gateway.WebBff.Extensions;
using GloboTicket.Gateway.WebBff.Models;
using GloboTicket.Gateway.WebBff.Url;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GloboTicket.Gateway.WebBff.Services
{
    public class CatalogService: ICatalogService
    {
        private readonly HttpClient client;
        private IDistributedCache cache;

        public CatalogService(HttpClient client, IDistributedCache distCache)
        {
            this.client = client;
            this.cache = distCache;
        }

        private async Task<T> CacheGetOrCreateAsync<T>(string url) {

            T data;
            var encodedData = await cache.GetAsync(url);

            if (encodedData != null)
            {
                data = JsonSerializer.Deserialize<T>(encodedData);
                Console.WriteLine($"CACHE: Data from cache with key: {url}");
            }
            else
            {
                var response = await client.GetAsync(url);
                data = await response.ReadContentAs<T>();

                byte[] dataByteArray = JsonSerializer.SerializeToUtf8Bytes(data);

                var distCacheOptions = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(20))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(100));

                await cache.SetAsync(url, dataByteArray, distCacheOptions);
                Console.WriteLine($"API Call: Data from {url}");
            }

            return data;
        }

        public async Task<List<EventDto>> GetAllEvents()
        {
            return await CacheGetOrCreateAsync<List<EventDto>>(EventCatalogOperations.GetAllEvents());
        }

        public async Task<List<EventDto>> GetEventsPerCategory(Guid categoryId)
        {
            return await CacheGetOrCreateAsync<List<EventDto>>(EventCatalogOperations.GetEventsPerCategory(categoryId));
        }

        public async Task<EventDto> GetEventById(Guid eventId)
        {
            return await CacheGetOrCreateAsync<EventDto>(EventCatalogOperations.GetEventById(eventId));
        }

        public async Task<List<CategoryDto>> GetAllCategories()
        {
            return await CacheGetOrCreateAsync<List<CategoryDto>>(EventCatalogOperations.GetAllcategories());
        }

    
    }
}
