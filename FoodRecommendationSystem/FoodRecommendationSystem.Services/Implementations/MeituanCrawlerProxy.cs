using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.Services.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace FoodRecommendationSystem.Services.Implementations
{
    /// <summary>
    /// 美团数据爬取服务
    /// </summary>
    public class MeituanCrawlerProxy : ICrawlerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MeituanCrawlerProxy> _logger;
        private readonly string _baseUrl = "https://wh.meituan.com";

        public MeituanCrawlerProxy(HttpClient httpClient, ILogger<MeituanCrawlerProxy> logger)
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
        public string PlatformName => "Meituan";

        /// <summary>
        /// 爬取商家数据
        /// </summary>
        public async Task<List<Merchant>> CrawlMerchantsAsync(string platform)
        {
            try
            {
                _logger.LogInformation($"开始爬取美团商家数据");
                var merchants = new List<Merchant>();

                // 武汉各区域ID
                var districtIds = new Dictionary<string, Region>
                {
                    { "1574", Region.WuchangDistrict },   // 武昌区
                    { "1575", Region.HanKouDistrict },    // 汉口区
                    { "1576", Region.HanYangDistrict },   // 汉阳区
                    { "1577", Region.QingshanDistrict },  // 青山区
                    { "1578", Region.HongShanDistrict },  // 洪山区
                    { "28478", Region.JiangXiaDistrict }, // 江夏区
                    { "6549", Region.JiangHanDistrict }   // 江汉区
                };

                // 美食分类ID
                var foodCategoryIds = new List<string> { "1", "17", "11", "28", "24", "20" };

                foreach (var districtId in districtIds.Keys)
                {
                    foreach (var categoryId in foodCategoryIds)
                    {
                        // 构建爬取URL
                        var url = $"/meishi/{districtId}/c{categoryId}/";

                        try
                        {
                            var merchantsInCategory = await CrawlMerchantListPageAsync(url, districtIds[districtId]);
                            merchants.AddRange(merchantsInCategory);

                            // 避免请求过快
                            await Task.Delay(1000);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"爬取美团区域{districtId}分类{categoryId}失败");
                        }
                    }
                }

                _logger.LogInformation($"美团商家数据爬取完成，共获取{merchants.Count}家商户");
                return merchants;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "爬取美团商家数据出错");
                throw;
            }
        }

        /// <summary>
        /// 爬取商家列表页面
        /// </summary>
        private async Task<List<Merchant>> CrawlMerchantListPageAsync(string url, Region region)
        {
            var merchants = new List<Merchant>();
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // 获取商家列表节点
            var merchantNodes = doc.DocumentNode.SelectNodes("//div[@class='list-item']");
            if (merchantNodes == null)
                return merchants;

            foreach (var node in merchantNodes)
            {
                try
                {
                    // 解析商家信息
                    var titleNode = node.SelectSingleNode(".//div[@class='item-title']/h4");
                    var name = titleNode?.InnerText.Trim();

                    var scoreNode = node.SelectSingleNode(".//div[@class='item-eval-info clearfix']/span");
                    var scoreText = scoreNode?.InnerText.Trim();
                    double rating = 0;
                    if (scoreText != null && double.TryParse(scoreText, out var parsedScore))
                    {
                        rating = parsedScore;
                    }

                    var reviewNode = node.SelectSingleNode(".//div[@class='item-eval-info clearfix']/span[2]");
                    var reviewText = reviewNode?.InnerText.Replace("条点评", "").Trim();
                    int reviewCount = 0;
                    if (reviewText != null && int.TryParse(reviewText, out var parsedReviewCount))
                    {
                        reviewCount = parsedReviewCount;
                    }

                    var priceNode = node.SelectSingleNode(".//div[@class='item-price']/span");
                    var priceText = priceNode?.InnerText.Replace("￥", "").Trim();
                    decimal averagePrice = 0;
                    if (priceText != null && decimal.TryParse(priceText, out var parsedPrice))
                    {
                        averagePrice = parsedPrice;
                    }

                    var addressNode = node.SelectSingleNode(".//div[@class='item-site-info']");
                    var address = addressNode?.InnerText.Trim();

                    var categoryNodes = node.SelectNodes(".//div[@class='item-tags']/span");
                    var categories = new List<string>();
                    if (categoryNodes != null)
                    {
                        foreach (var catNode in categoryNodes)
                        {
                            categories.Add(catNode.InnerText.Trim());
                        }
                    }

                    var linkNode = node.SelectSingleNode(".//a[@class='link']");
                    var detailUrl = linkNode?.GetAttributeValue("href", "");
                    var platformId = "";

                    // 从URL中解析平台ID
                    if (!string.IsNullOrEmpty(detailUrl))
                    {
                        var match = Regex.Match(detailUrl, @"/(\d+)\.html");
                        if (match.Success)
                        {
                            platformId = match.Groups[1].Value;
                        }
                    }

                    // 构建商家对象
                    var merchant = new Merchant
                    {
                        Name = name??string.Empty,
                        Rating = rating,
                        ReviewCount = reviewCount,
                        Address = address??string.Empty,
                        AveragePrice = averagePrice,
                        Categories = categories,
                        Region = region,
                        LastUpdated = DateTime.Now,
                        PlatformInfos = new List<PlatformInfo>
                        {
                            new PlatformInfo
                            {
                                PlatformName = "Meituan",
                                PlatformMerchantId = platformId,
                                PlatformUrl = detailUrl?.StartsWith("http") == true
                                                ? detailUrl
                                                 : _baseUrl + (detailUrl ?? string.Empty),
                                PlatformRating = rating,
                                PlatformReviewCount = reviewCount
                            }
                        }
                    };

                    // 如果有详情URL，爬取详细信息
                    if (!string.IsNullOrEmpty(detailUrl))
                    {
                        try
                        {
                            await EnrichMerchantDetailsAsync(merchant, detailUrl);

                            // 避免请求过快
                            await Task.Delay(500);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"爬取商家{name}详情失败");
                        }
                    }

                    merchants.Add(merchant);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "解析商家节点失败");
                }
            }

            return merchants;
        }

        /// <summary>
        /// 爬取商家详细信息
        /// </summary>
        private async Task EnrichMerchantDetailsAsync(Merchant merchant, string detailUrl)
        {
            if (string.IsNullOrEmpty(detailUrl))
                return;

            // 构建完整URL
            var url = detailUrl.StartsWith("http") ? detailUrl : _baseUrl + detailUrl;

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // 获取商家描述
            var descNode = doc.DocumentNode.SelectSingleNode("//div[@class='merchant-content']");
            if (descNode != null)
            {
                merchant.Description = descNode.InnerText.Trim();
            }

            // 获取营业时间
            var hoursNode = doc.DocumentNode.SelectSingleNode("//div[@class='merchant-info']/div[@class='info-item']/span[contains(text(), '营业时间')]");
            if (hoursNode != null)
            {
                merchant.BusinessHours = hoursNode.NextSibling.InnerText.Trim();
            }

            // 获取电话
            var phoneNode = doc.DocumentNode.SelectSingleNode("//div[@class='merchant-info']/div[@class='info-item']/span[contains(text(), '电话')]");
            if (phoneNode != null)
            {
                merchant.PhoneNumber = phoneNode.NextSibling.InnerText.Trim();
            }

            // 获取地理位置
            var scriptNodes = doc.DocumentNode.SelectNodes("//script");
            if (scriptNodes != null)
            {
                foreach (var scriptNode in scriptNodes)
                {
                    var scriptContent = scriptNode.InnerText;
                    if (scriptContent.Contains("address") && scriptContent.Contains("latitude"))
                    {
                        var latMatch = Regex.Match(scriptContent, @"latitude:\s*([\d\.]+)");
                        var lngMatch = Regex.Match(scriptContent, @"longitude:\s*([\d\.]+)");

                        if (latMatch.Success && lngMatch.Success)
                        {
                            merchant.Location = new GeoLocation
                            {
                                Latitude = double.Parse(latMatch.Groups[1].Value),
                                Longitude = double.Parse(lngMatch.Groups[1].Value)
                            };
                            break;
                        }
                    }
                }
            }

            // 获取菜品信息
            var dishNodes = doc.DocumentNode.SelectNodes("//div[@class='foodlist']/div[@class='foodlist-item']");
            if (dishNodes != null)
            {
                merchant.Dishes = new List<Dish>();

                foreach (var dishNode in dishNodes)
                {
                    try
                    {
                        var dishName = dishNode.SelectSingleNode(".//div[@class='foodlist-name']")?.InnerText.Trim();
                        var dishPrice = dishNode.SelectSingleNode(".//div[@class='foodlist-price']")?.InnerText.Replace("¥", "").Trim();
                        var dishSales = dishNode.SelectSingleNode(".//div[@class='foodlist-sale']")?.InnerText.Replace("月售", "").Replace("份", "").Trim();
                        var dishImgSrc = dishNode.SelectSingleNode(".//img")?.GetAttributeValue("src", "");

                        decimal price = 0;
                        if (dishPrice != null && decimal.TryParse(dishPrice, out var parsedPrice))
                        {
                            price = parsedPrice;
                        }

                        int sales = 0;
                        if (dishSales != null && int.TryParse(dishSales, out var parsedSales))
                        {
                            sales = parsedSales;
                        }

                        var dish = new Dish
                        {
                            Name = dishName??string.Empty,
                            MerchantId = merchant.Id,
                            MonthlySales = sales,
                            LastUpdated = DateTime.Now,
                            Prices = new List<PriceComparison>
                            {
                                new PriceComparison
                                {
                                    PlatformName = "Meituan",
                                    Price = price,
                                    LastUpdated = DateTime.Now,
                                    PlatformUrl = url,
                                    HasDiscount = false,
                                    OriginalPrice = price
                                }
                            }
                        };

                        if (!string.IsNullOrEmpty(dishImgSrc))
                        {
                            dish.ImageUrls = new List<string> { dishImgSrc };
                        }

                        merchant.Dishes.Add(dish);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "解析菜品节点失败");
                    }
                }
            }

            // 获取商家图片
            var imgNodes = doc.DocumentNode.SelectNodes("//div[@class='photos']/div/img");
            if (imgNodes != null)
            {
                merchant.ImageUrls = new List<string>();

                foreach (var imgNode in imgNodes)
                {
                    var imgSrc = imgNode.GetAttributeValue("src", "");
                    if (!string.IsNullOrEmpty(imgSrc))
                    {
                        merchant.ImageUrls.Add(imgSrc);
                    }
                }
            }
        }
    }
}