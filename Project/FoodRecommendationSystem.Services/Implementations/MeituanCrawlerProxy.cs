using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.Services.Interfaces;

namespace FoodRecommendationSystem.Services.Implementations
{
    public class MeituanCrawlerProxy : ICrawlerService
    {
        private readonly HttpClient _httpClient;

        public MeituanCrawlerProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Merchant>> CrawlMerchantsAsync(string platform)
        {
            var response = await _httpClient.GetAsync($"api/{platform}/merchants");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<List<Merchant>>();
        }
    }
}
