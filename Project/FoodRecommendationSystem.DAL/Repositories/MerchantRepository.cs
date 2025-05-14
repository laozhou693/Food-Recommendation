using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using MongoDB.Driver;

namespace FoodRecommendationSystem.DAL.Repositories
{
    public class MerchantRepository
    {
        private readonly IMongoCollection<Merchant> _collection;

        public MerchantRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Merchant>("merchants");
        }

        public async Task<List<Merchant>> GetNearbyMerchantsAsync(GeoLocation location, double radiusKm)
        {
            var filter = Builders<Merchant>.Filter.Near(
                m => m.Location,
                location.Longitude,
                location.Latitude,
                radiusKm * 1000
            );
            return await _collection.Find(filter).ToListAsync();
        }
    }
}