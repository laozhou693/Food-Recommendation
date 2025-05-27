using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace FoodRecommendationSystem.Services.Implementations
{
    /// <summary>
    /// 大众点评数据爬取服务
    /// </summary>
    public class DianpingCrawlerProxy : ICrawlerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DianpingCrawlerProxy> _logger;
        private readonly string _baseUrl = "https://www.dianping.com";

        public DianpingCrawlerProxy(HttpClient httpClient, ILogger<DianpingCrawlerProxy> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // 设置基础URL
            _httpClient.BaseAddress = new Uri(_baseUrl);

            // 设置UA头
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            // 设置Referer头(避免被封)
            _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.dianping.com/wuhan");

            // 设置Cookie(可能需要登录Cookie才能访问某些页面)
            _httpClient.DefaultRequestHeaders.Add("Cookie", "_lxsdk_cuid=xxxxxxxx; _hc.v=xxxxxxxx;");
        }

        /// <summary>
        /// 获取平台名称
        /// </summary>
        public string PlatformName => "Dianping";

        /// <summary>
        /// 爬取商家数据
        /// </summary>
        public async Task<List<Merchant>> CrawlMerchantsAsync(string platform)
        {
            try
            {
                _logger.LogInformation($"开始爬取大众点评商家数据");
                var merchants = new List<Merchant>();

                // 武汉各区域ID映射
                var districts = new Dictionary<string, Region>
                {
                    { "r1577", Region.WuchangDistrict }, // 武昌区
                    { "r1578", Region.HanKouDistrict },  // 汉口区
                    { "r1579", Region.HanYangDistrict }, // 汉阳区
                    { "r1581", Region.QingshanDistrict },// 青山区
                    { "r1580", Region.HongShanDistrict },// 洪山区
                    { "r1582", Region.JiangXiaDistrict },// 江夏区
                    { "r1583", Region.JiangHanDistrict } // 江汉区
                };

                // 美食分类ID映射
                var categories = new Dictionary<string, string>
                {
                    { "c11", "全部美食" },
                    { "c17", "火锅" },
                    { "c30", "小吃" },
                    { "c34", "湘菜" },
                    { "c55", "江浙菜" }
                };

                foreach (var district in districts.Keys)
                {
                    foreach (var category in categories.Keys)
                    {
                        try
                        {
                            // 构建URL
                            var url = $"/wuhan/{district}/{category}";

                            // 爬取商家列表页面
                            var merchantsInCategory = await CrawlMerchantListPageAsync(url, districts[district], categories[category]);
                            merchants.AddRange(merchantsInCategory);

                            // 避免请求过快
                            await Task.Delay(2000);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"爬取大众点评区域{district}分类{category}失败");
                        }
                    }
                }

                _logger.LogInformation($"大众点评商家数据爬取完成，共获取{merchants.Count}家商户");
                return merchants;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "爬取大众点评商家数据出错");
                throw;
            }
        }

        /// <summary>
        /// 爬取商家列表页面
        /// </summary>
        private async Task<List<Merchant>> CrawlMerchantListPageAsync(string url, Region region, string category)
        {
            var merchants = new List<Merchant>();

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // 商家列表节点
                var merchantNodes = doc.DocumentNode.SelectNodes("//div[@id='shop-all-list']/ul/li");
                if (merchantNodes == null)
                    return merchants;

                foreach (var node in merchantNodes)
                {
                    try
                    {
                        // 解析商家名称
                        var nameNode = node.SelectSingleNode(".//div[@class='tit']/a");
                        var name = nameNode?.GetAttributeValue("title", "").Trim();
                        var detailUrl = nameNode?.GetAttributeValue("href", "");

                        // 解析商家评分
                        var ratingNode = node.SelectSingleNode(".//div[@class='comment']/span");
                        var ratingText = ratingNode?.InnerText.Trim();
                        double rating = 0;
                        if (!string.IsNullOrEmpty(ratingText))
                        {
                            double.TryParse(ratingText, out rating);
                        }

                        // 解析评论数量
                        var reviewNode = node.SelectSingleNode(".//div[@class='comment']/a[@class='review-num']");
                        var reviewText = reviewNode?.InnerText.Replace("条点评", "").Trim();
                        int reviewCount = 0;
                        if (!string.IsNullOrEmpty(reviewText))
                        {
                            int.TryParse(reviewText, out reviewCount);
                        }

                        // 解析人均价格
                        var priceNode = node.SelectSingleNode(".//div[@class='comment']/a[@class='mean-price']");
                        var priceText = priceNode?.InnerText.Replace("¥", "").Replace("人均", "").Trim();
                        decimal averagePrice = 0;
                        if (!string.IsNullOrEmpty(priceText))
                        {
                            decimal.TryParse(priceText, out averagePrice);
                        }

                        // 解析地址
                        var addressNode = node.SelectSingleNode(".//div[@class='tag-addr']/span[@class='addr']");
                        var address = addressNode?.InnerText.Trim();

                        // 解析商家分类
                        var categoryNodes = node.SelectNodes(".//div[@class='tag-addr']/span[not(@class)]");
                        var categories = new List<string>();

                        if (categoryNodes != null)
                        {
                            foreach (var catNode in categoryNodes)
                            {
                                categories.Add(catNode.InnerText.Trim());
                            }
                        }

                        // 如果没有分类，添加当前页面分类
                        if (categories.Count == 0 && !string.IsNullOrEmpty(category))
                        {
                            categories.Add(category);
                        }

                        // 解析商家图片URL
                        var imgNode = node.SelectSingleNode(".//div[@class='pic']/a/img");
                        var imgUrl = imgNode?.GetAttributeValue("src", "");
                        var imageUrls = new List<string>();
                        if (!string.IsNullOrEmpty(imgUrl))
                        {
                            imageUrls.Add(imgUrl);
                        }

                        // 解析商家ID
                        var platformId = string.Empty;
                        if (!string.IsNullOrEmpty(detailUrl))
                        {
                            var match = Regex.Match(detailUrl, @"/shop/(\d+)");
                            if (match.Success)
                            {
                                platformId = match.Groups[1].Value;
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
                            ImageUrls = imageUrls,
                            LastUpdated = DateTime.Now,
                            PlatformInfos = new List<PlatformInfo>
                            {
                                new PlatformInfo
                                {
                                    PlatformName = "Dianping",
                                    PlatformMerchantId = platformId,
                                    PlatformUrl = detailUrl ?? string.Empty,
                                    PlatformRating = rating,
                                    PlatformReviewCount = reviewCount
                                }
                            }
                        };

                        // 如果有详情URL，爬取商家详情
                        if (!string.IsNullOrEmpty(detailUrl))
                        {
                            try
                            {
                                await EnrichMerchantDetailsAsync(merchant, detailUrl);

                                // 避免请求过快
                                await Task.Delay(1500);
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
                        _logger.LogWarning(ex, "解析大众点评商家节点失败");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"爬取大众点评页面{url}失败");
            }

            return merchants;
        }

        /// <summary>
        /// 爬取商家详细信息
        /// </summary>
        private async Task EnrichMerchantDetailsAsync(Merchant merchant, string detailUrl)
        {
            try
            {
                var response = await _httpClient.GetAsync(detailUrl);
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // 解析地理位置
                var scriptNodes = doc.DocumentNode.SelectNodes("//script");
                if (scriptNodes != null)
                {
                    foreach (var scriptNode in scriptNodes)
                    {
                        var script = scriptNode.InnerText;
                        if (script.Contains("poi:") && script.Contains("lat:"))
                        {
                            var latMatch = Regex.Match(script, @"lat:\s*([0-9\.]+)");
                            var lngMatch = Regex.Match(script, @"lng:\s*([0-9\.]+)");

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

                // 解析联系电话
                var phoneNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'tel')]/div[@class='content']");
                if (phoneNode != null)
                {
                    merchant.PhoneNumber = phoneNode.InnerText.Trim();
                }

                // 解析营业时间
                var hoursNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'info-indent')]/div[@class='item']/div[@class='content']");
                if (hoursNode != null)
                {
                    merchant.BusinessHours = hoursNode.InnerText.Trim();
                }

                // 解析更多图片
                var moreImgNodes = doc.DocumentNode.SelectNodes("//div[@class='photos']/a/img");
                if (moreImgNodes != null)
                {
                    if (merchant.ImageUrls == null)
                    {
                        merchant.ImageUrls = new List<string>();
                    }

                    foreach (var imgNode in moreImgNodes)
                    {
                        var imgUrl = imgNode.GetAttributeValue("src", "");
                        if (!string.IsNullOrEmpty(imgUrl) && !merchant.ImageUrls.Contains(imgUrl))
                        {
                            merchant.ImageUrls.Add(imgUrl);
                        }

                        // 最多保留5张图片
                        if (merchant.ImageUrls.Count >= 5)
                            break;
                    }
                }

                // 获取招牌菜
                await GetSignatureDishesAsync(merchant, detailUrl);

                // 获取评价
                await GetReviewsAsync(merchant, detailUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"获取大众点评商家详情失败: {detailUrl}");
            }
        }

        /// <summary>
        /// 获取招牌菜信息
        /// </summary>
        private async Task GetSignatureDishesAsync(Merchant merchant, string merchantUrl)
        {
            try
            {
                // 点评招牌菜通常在单独的页面
                var dishesUrl = merchantUrl + "/dishes";
                var response = await _httpClient.GetAsync(dishesUrl);

                if (!response.IsSuccessStatusCode)
                    return;

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // 招牌菜节点
                var dishNodes = doc.DocumentNode.SelectNodes("//div[@class='recommend-list']/div[@class='item']");
                if (dishNodes == null)
                    return;

                var dishes = new List<Dish>();

                foreach (var node in dishNodes)
                {
                    try
                    {
                        var nameNode = node.SelectSingleNode(".//p[@class='name']");
                        var name = nameNode?.InnerText.Trim();

                        var priceNode = node.SelectSingleNode(".//span[@class='price']");
                        var priceText = priceNode?.InnerText.Replace("￥", "").Trim();
                        decimal price = 0;
                        if (!string.IsNullOrEmpty(priceText))
                        {
                            decimal.TryParse(priceText, out price);
                        }

                        var imgNode = node.SelectSingleNode(".//img");
                        var imgUrl = imgNode?.GetAttributeValue("src", "");

                        var dish = new Dish
                        {
                            Name = name ?? string.Empty,
                            MerchantId = merchant.Id,
                            IsSignatureDish = true,
                            LastUpdated = DateTime.Now,
                            Prices = new List<PriceComparison>
                            {
                                new PriceComparison
                                {
                                    PlatformName = "Dianping",
                                    Price = price,
                                    LastUpdated = DateTime.Now,
                                    PlatformUrl = dishesUrl,
                                    HasDiscount = false,
                                    OriginalPrice = price
                                }
                            }
                        };

                        if (!string.IsNullOrEmpty(imgUrl))
                        {
                            dish.ImageUrls = new List<string> { imgUrl };
                        }

                        dishes.Add(dish);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "解析大众点评招牌菜失败");
                    }
                }

                // 将招牌菜添加到商家菜品列表
                if (merchant.Dishes == null)
                {
                    merchant.Dishes = dishes;
                }
                else
                {
                    merchant.Dishes.AddRange(dishes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"获取大众点评招牌菜失败: {merchantUrl}");
            }
        }

        /// <summary>
        /// 获取评价信息
        /// </summary>
        private async Task GetReviewsAsync(Merchant merchant, string merchantUrl)
        {
            try
            {
                // 点评评价页面
                var reviewUrl = merchantUrl + "/review_all";
                var response = await _httpClient.GetAsync(reviewUrl);

                if (!response.IsSuccessStatusCode)
                    return;

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // 评价节点
                var reviewNodes = doc.DocumentNode.SelectNodes("//div[@class='reviews-items']/ul/li");
                if (reviewNodes == null)
                    return;

                var reviews = new List<Review>();

                foreach (var node in reviewNodes)
                {
                    try
                    {
                        var userNode = node.SelectSingleNode(".//div[@class='dper-info']/a");
                        var userName = userNode?.InnerText.Trim();

                        var ratingNode = node.SelectSingleNode(".//div[@class='review-rank']/span");
                        var ratingClass = ratingNode?.GetAttributeValue("class", "");
                        double rating = 0;

                        // 从CSS类名解析评分
                        if (!string.IsNullOrEmpty(ratingClass))
                        {
                            var match = Regex.Match(ratingClass, @"sml-str(\d+)");
                            if (match.Success)
                            {
                                int starValue = int.Parse(match.Groups[1].Value);
                                rating = starValue / 10.0; // 例如50表示5星
                            }
                        }

                        var contentNode = node.SelectSingleNode(".//div[@class='review-words']");
                        var content = contentNode?.InnerText.Trim();

                        var timeNode = node.SelectSingleNode(".//span[@class='time']");
                        var timeText = timeNode?.InnerText.Trim();
                        DateTime reviewDate = DateTime.Now;
                        if (!string.IsNullOrEmpty(timeText))
                        {
                            DateTime.TryParse(timeText, out reviewDate);
                        }

                        var reviewIdAttr = node.GetAttributeValue("data-id", "");

                        var review = new Review
                        {
                            MerchantId = merchant.Id,
                            ReviewerName = userName ?? string.Empty,
                            Content = content ?? string.Empty,
                            Rating = rating,
                            ReviewDate = reviewDate,
                            PlatformName = "Dianping",
                            PlatformReviewId = reviewIdAttr
                        };

                        reviews.Add(review);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "解析大众点评评价失败");
                    }
                }

                // 将评价存储到外部变量或传递给调用者
                // 注意：这需要一个额外的参数来保存评价列表
                // 由于我们没有直接的ReviewRepository注入，只能先将评价存放到商家对象的临时字段中
                merchant.Reviews = reviews;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"获取大众点评评价失败: {merchantUrl}");
            }
        }
    }
}