using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;

namespace FoodRecommendationSystem.Services.Interfaces
{
    public interface ICrawlerService
    {
        Task<List<Merchant>> CrawlMerchantsAsync(string platform);
    }
}