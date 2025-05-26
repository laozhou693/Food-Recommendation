using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace FoodRecommendationSystem.Services.Implementations
{
    /// <summary>
    /// 饿了么数据爬取服务
    /// </summary>
    public class ElementCrawlerProxy : ICrawlerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ElementCrawlerProxy> _logger;
        private readonly string _baseUrl = "https://www.ele.me";

        public ElementCrawlerProxy(HttpClient httpClient, ILogger<ElementCrawlerProxy> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // 设置基础URL
            _httpClient.BaseAddress = new Uri(_baseUrl);

            // 设置UA头
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }

        /// <summary>
        /// 获取平台名称
        /// </summary>
        public string PlatformName => "Eleme";

        /// <summary>
        /// 爬取商家数据
        /// </summary>
        public async Task<List<Merchant>> CrawlMerchantsAsync(string platform)
        {
            try
            {
                _logger.LogInformation($"开始爬取饿了么商家数据");
                var merchants = new List<Merchant>();

                // 武汉各区域地址
                var districts = new Dictionary<string, Region>
                {
                    { "武昌区", Region.WuchangDistrict },
                    { "汉口区", Region.HanKouDistrict },
                    { "汉阳区", Region.HanYangDistrict },
                    { "青山区", Region.QingshanDistrict },
                    { "洪山区", Region.HongShanDistrict },
                    { "江夏区", Region.JiangXiaDistrict },
                    { "江汉区", Region.JiangHanDistrict }
                };

                // 美食分类标签
                var foodCategories = new List<string> { "快餐便当", "小吃夜宵", "火锅", "川湘菜", "江浙菜", "香锅香辣" };

                foreach (var district in districts.Keys)
                {
                    foreach (var category in foodCategories)
                    {
                        try
                        {
                            // 构建API请求
                            var apiUrl = $"/restapi/shopping/v3/restaurants?offset=0&limit=20&keyword={district}+{category}&latitude=30.593099&longitude=114.305313";

                            var merchantsInCategory = await CrawlMerchantListApiAsync(apiUrl, districts[district]);
                            merchants.AddRange(merchantsInCategory);

                            // 避免请求过快
                            await Task.Delay(1500);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"爬取饿了么区域{district}分类{category}失败");
                        }
                    }
                }

                _logger.LogInformation($"饿了么商家数据爬取完成，共获取{merchants.Count}家商户");
                return merchants;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "爬取饿了么商家数据出错");
                throw;
            }
        }

        /// <summary>
        /// 爬取商家列表API
        /// </summary>
        private async Task<List<Merchant>> CrawlMerchantListApiAsync(string apiUrl, Region region)
        {
            var merchants = new List<Merchant>();
            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = JsonDocument.Parse(jsonContent);

            if (data.RootElement.TryGetProperty("items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    try
                    {
                        if (item.TryGetProperty("restaurant", out var restaurant))
                        {
                            // 解析商家基本信息
                            var platformId = restaurant.GetProperty("id").GetString();
                            var name = restaurant.GetProperty("name").GetString();
                            var rating = restaurant.GetProperty("rating").GetDouble();
                            var latitude = restaurant.GetProperty("latitude").GetDouble();
                            var longitude = restaurant.GetProperty("longitude").GetDouble();
                            var address = restaurant.GetProperty("address").GetString();

                            // 尝试解析评价数量
                            int reviewCount = 0;
                            if (restaurant.TryGetProperty("rating_count", out var ratingCountProp))
                            {
                                reviewCount = ratingCountProp.GetInt32();
                            }

                            // 尝试解析人均价格
                            decimal averagePrice = 0;
                            if (restaurant.TryGetProperty("average_price", out var averagePriceProp))
                            {
                                averagePrice = averagePriceProp.GetDecimal();
                            }

                            // 解析分类
                            var categories = new List<string>();
                            if (restaurant.TryGetProperty("flavors", out var flavorsProp))
                            {
                                foreach (var flavor in flavorsProp.EnumerateArray())
                                {
                                    categories.Add(flavor.GetProperty("name").GetString() ?? string.Empty);
                                }
                            }

                            // 构建商家对象
                            var merchant = new Merchant
                            {
                                Name = name ?? string.Empty,
                                Rating = rating,
                                ReviewCount = reviewCount,
                                Address = address ?? string.Empty,
                                AveragePrice = averagePrice,
                                Categories = categories,
                                Region = region,
                                LastUpdated = DateTime.Now,
                                Location = new GeoLocation
                                {
                                    Latitude = latitude,
                                    Longitude = longitude
                                },
                                PlatformInfos = new List<PlatformInfo>
                                {
                                    new PlatformInfo
                                    {
                                        PlatformName = "Eleme",
                                        PlatformMerchantId = platformId ?? string.Empty,
                                        PlatformUrl = $"{_baseUrl}/shop/{platformId}",
                                        PlatformRating = rating,
                                        PlatformReviewCount = reviewCount
                                    }
                                }
                            };

                            // 尝试解析商家图片
                            if (restaurant.TryGetProperty("image_path", out var imagePathProp))
                            {
                                var imagePath = imagePathProp.GetString();
                                if (!string.IsNullOrEmpty(imagePath))
                                {
                                    merchant.ImageUrls = new List<string>
                                    {
                                        $"https://fuss10.elemecdn.com/{imagePath.Substring(0, 1)}/{imagePath.Substring(1, 1)}/{imagePath.Substring(2)}.jpg"
                                    };
                                }
                            }

                            // 爬取商家详细信息
                            if (platformId != null)
                            {
                                await EnrichMerchantDetailsAsync(merchant, platformId);
                            }
                            else
                            {
                                // 取决于你的业务逻辑，可能有不同的处理方式
                                _logger.LogWarning("跳过商家详情更新: platformId 为 null");
                                // 或者
                                // throw new ArgumentNullException(nameof(platformId), "丰富商家详情需要有效的平台ID");
                            }

                            merchants.Add(merchant);

                            // 避免请求过快
                            await Task.Delay(500);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "解析饿了么商家数据失败");
                    }
                }
            }

            return merchants;
        }

        /// <summary>
        /// 爬取商家详细信息
        /// </summary>
        private async Task EnrichMerchantDetailsAsync(Merchant merchant, string platformId)
        {
            try
            {
                // 获取菜单信息API
                var menuUrl = $"/restapi/shopping/v2/menu?restaurant_id={platformId}";
                var menuResponse = await _httpClient.GetAsync(menuUrl);
                menuResponse.EnsureSuccessStatusCode();

                var menuJson = await menuResponse.Content.ReadAsStringAsync();
                var menuData = JsonDocument.Parse(menuJson);

                // 解析菜品
                var dishes = new List<Dish>();

                foreach (var category in menuData.RootElement.EnumerateArray())
                {
                    if (category.TryGetProperty("foods", out var foods))
                    {
                        foreach (var food in foods.EnumerateArray())
                        {
                            try
                            {
                                var dishName = food.GetProperty("name").GetString();
                                var description = string.Empty;
                                if (food.TryGetProperty("description", out var descProp))
                                {
                                    description = descProp.GetString();
                                }

                                var price = food.GetProperty("price").GetDecimal() / 100; // 转换为元
                                var originalPrice = price;

                                // 检查是否有原始价格
                                if (food.TryGetProperty("original_price", out var origPriceProp))
                                {
                                    var origPrice = origPriceProp.GetDecimal() / 100;
                                    if (origPrice > 0 && origPrice != price)
                                    {
                                        originalPrice = origPrice;
                                    }
                                }

                                // 获取月销量
                                int monthlySales = 0;
                                if (food.TryGetProperty("month_sales", out var salesProp))
                                {
                                    monthlySales = salesProp.GetInt32();
                                }

                                // 获取评分
                                double rating = 0;
                                if (food.TryGetProperty("rating", out var ratingProp))
                                {
                                    rating = ratingProp.GetDouble();
                                }

                                // 获取图片
                                var imageUrls = new List<string>();
                                if (food.TryGetProperty("image_path", out var imgProp))
                                {
                                    var imagePath = imgProp.GetString();
                                    if (!string.IsNullOrEmpty(imagePath))
                                    {
                                        imageUrls.Add($"https://fuss10.elemecdn.com/{imagePath.Substring(0, 1)}/{imagePath.Substring(1, 1)}/{imagePath.Substring(2)}.jpg");
                                    }
                                }

                                // 获取分类
                                var dishCategories = new List<string>();
                                if (category.TryGetProperty("name", out var catNameProp))
                                {
                                    dishCategories.Add(catNameProp.GetString() ?? string.Empty);
                                }

                                var dish = new Dish
                                {
                                    Name = dishName ?? string.Empty,
                                    Description = description ?? string.Empty,
                                    MerchantId = merchant.Id,
                                    Categories = dishCategories,
                                    Rating = rating,
                                    MonthlySales = monthlySales,
                                    ImageUrls = imageUrls,
                                    LastUpdated = DateTime.Now,
                                    Prices = new List<PriceComparison>
                                    {
                                        new PriceComparison
                                        {
                                            PlatformName = "Eleme",
                                            Price = price,
                                            OriginalPrice = originalPrice,
                                            HasDiscount = price < originalPrice,
                                            LastUpdated = DateTime.Now,
                                            PlatformUrl = $"{_baseUrl}/shop/{platformId}"
                                        }
                                    }
                                };

                                dishes.Add(dish);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "解析饿了么菜品数据失败");
                            }
                        }
                    }
                }

                merchant.Dishes = dishes;

                // 获取联系电话和营业时间
                var detailUrl = $"/restapi/shopping/restaurant/{platformId}";
                var detailResponse = await _httpClient.GetAsync(detailUrl);

                if (detailResponse.IsSuccessStatusCode)
                {
                    var detailJson = await detailResponse.Content.ReadAsStringAsync();
                    var detailData = JsonDocument.Parse(detailJson);

                    if (detailData.RootElement.TryGetProperty("phone", out var phoneProp))
                    {
                        merchant.PhoneNumber = phoneProp.GetString() ?? string.Empty;
                    }

                    if (detailData.RootElement.TryGetProperty("opening_hours", out var hoursProp))
                    {
                        var hours = new List<string>();
                        foreach (var hour in hoursProp.EnumerateArray())
                        {
                            hours.Add(hour.GetString() ?? string.Empty);
                        }
                        merchant.BusinessHours = string.Join(", ", hours);
                    }

                    if (detailData.RootElement.TryGetProperty("description", out var descProp))
                    {
                        merchant.Description = descProp.GetString() ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"获取饿了么商家ID:{platformId}详情失败");
            }
        }
    }
}