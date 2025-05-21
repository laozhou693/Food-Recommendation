using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace FoodRecommendationSystem.DAL.Repositories
{
    /// <summary>
    /// 菜品数据访问仓储
    /// </summary>
    public class DishRepository
    {
        private readonly IMongoCollection<Dish> _collection;

        public DishRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Dish>("dishes");

            // 创建商家ID索引
            var indexModel = new CreateIndexModel<Dish>("{ MerchantId: 1 }");
            _collection.Indexes.CreateOne(indexModel);

            // 创建名称文本索引用于搜索
            var textIndexModel = new CreateIndexModel<Dish>("{ Name: 'text', Description: 'text' }");
            _collection.Indexes.CreateOne(textIndexModel);
        }

        /// <summary>
        /// 根据ID获取菜品
        /// </summary>
        public async Task<Dish> GetByIdAsync(string id)
        {
            return await _collection.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根据商家ID获取菜品列表
        /// </summary>
        public async Task<List<Dish>> GetByMerchantIdAsync(string merchantId)
        {
            return await _collection.Find(d => d.MerchantId == merchantId).ToListAsync();
        }

        /// <summary>
        /// 搜索菜品
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <param name="categories">菜品分类列表</param>
        /// <param name="minRating">最低评分</param>
        /// <param name="maxPrice">最高价格</param>
        /// <param name="tags">菜品标签</param>
        /// <returns>符合条件的菜品列表</returns>
        public async Task<List<Dish>> SearchAsync(string? keyword = null, List<string>? categories = null, double? minRating = null, decimal? maxPrice = null, List<string>? tags = null)
        {
            var builder = Builders<Dish>.Filter;
            var filter = builder.Empty;

            // 关键词搜索
            if (!string.IsNullOrEmpty(keyword))
            {
                if (keyword.Contains(" "))
                {
                    // 使用文本索引进行搜索
                    filter = builder.Text(keyword);
                }
                else
                {
                    // 对于单个词使用正则表达式搜索，性能更好
                    filter = builder.Regex(d => d.Name, new BsonRegularExpression(keyword, "i")) |
                             builder.Regex(d => d.Description, new BsonRegularExpression(keyword, "i"));
                }
            }

            // 按分类筛选
            if (categories != null && categories.Count > 0)
            {
                filter = filter & builder.AnyIn(d => d.Categories, categories);
            }

            // 按评分筛选
            if (minRating.HasValue)
            {
                filter = filter & builder.Gte(d => d.Rating, minRating.Value);
            }

            // 按价格筛选(如果有价格信息)
            if (maxPrice.HasValue)
            {
                filter = filter & builder.ElemMatch(d => d.Prices,
                    p => p.Price <= maxPrice.Value);
            }

            // 按标签筛选
            if (tags != null && tags.Count > 0)
            {
                filter = filter & builder.AnyIn(d => d.Tags, tags);
            }

            // 排序：先按评分，后按销量
            var sort = Builders<Dish>.Sort
                .Descending(d => d.Rating)
                .Descending(d => d.MonthlySales);

            return await _collection.Find(filter)
                .Sort(sort)
                .Limit(100)  // 限制返回数量
                .ToListAsync();
        }

        /// <summary>
        /// 创建新菜品
        /// </summary>
        public async Task CreateAsync(Dish dish)
        {
            if (string.IsNullOrEmpty(dish.Id))
            {
                dish.Id = ObjectId.GenerateNewId().ToString();
            }
            dish.LastUpdated = DateTime.Now;
            await _collection.InsertOneAsync(dish);
        }

        /// <summary>
        /// 批量创建菜品
        /// </summary>
        public async Task CreateManyAsync(List<Dish> dishes)
        {
            if (dishes == null || dishes.Count == 0)
                return;

            // 确保所有菜品都有ID和更新时间
            foreach (var dish in dishes)
            {
                if (string.IsNullOrEmpty(dish.Id))
                {
                    dish.Id = ObjectId.GenerateNewId().ToString();
                }
                dish.LastUpdated = DateTime.Now;
            }

            await _collection.InsertManyAsync(dishes);
        }

        /// <summary>
        /// 更新菜品
        /// </summary>
        public async Task UpdateAsync(string id, Dish dish)
        {
            dish.LastUpdated = DateTime.Now;
            await _collection.ReplaceOneAsync(d => d.Id == id, dish);
        }

        /// <summary>
        /// 更新菜品价格比较信息
        /// </summary>
        public async Task UpdatePriceComparisonAsync(string dishId, PriceComparison priceComparison)
        {
            // 查找菜品
            var dish = await GetByIdAsync(dishId);
            if (dish == null) return;

            // 检查是否存在该平台的价格信息
            var existingPriceIndex = dish.Prices.FindIndex(p => p.PlatformName == priceComparison.PlatformName);

            if (existingPriceIndex >= 0)
            {
                // 更新现有价格信息
                dish.Prices[existingPriceIndex] = priceComparison;
            }
            else
            {
                // 添加新价格信息
                dish.Prices.Add(priceComparison);
            }

            // 更新菜品
            await UpdateAsync(dishId, dish);
        }

        /// <summary>
        /// 获取特色菜品
        /// </summary>
        public async Task<List<Dish>> GetSignatureDishesAsync(int limit = 20)
        {
            return await _collection.Find(d => d.IsSignatureDish)
                .Sort(Builders<Dish>.Sort.Descending(d => d.Rating))
                .Limit(limit)
                .ToListAsync();
        }

        /// <summary>
        /// 删除菜品
        /// </summary>
        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(d => d.Id == id);
        }

        /// <summary>
        /// 根据商家ID删除所有菜品
        /// </summary>
        public async Task DeleteByMerchantIdAsync(string merchantId)
        {
            await _collection.DeleteManyAsync(d => d.MerchantId == merchantId);
        }
    }
}