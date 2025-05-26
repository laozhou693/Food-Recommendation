using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Food.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Food.Service
{
    public class DataImportService
    {
        private readonly IMongoCollection<BsonDocument> _merchantsCollection;
        private readonly ILogger<DataImportService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public DataImportService(
            IMongoDatabase database,
            ILogger<DataImportService> logger,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            _merchantsCollection = database.GetCollection<BsonDocument>("merchants");
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// 运行 Python 脚本导入标签
        /// </summary>
        /// <returns>导入结果</returns>
        public async Task<ImportResult> ImportMerchantTagsAsync()
        {
            try
            {
                _logger.LogInformation("开始导入商家标签数据...");

                // 确定 Python 脚本路径
                string scriptPath = Path.Combine(_environment.ContentRootPath, "Scripts", "import_merchant_tags.py");

                // 确保脚本目录存在
                var scriptDir = Path.GetDirectoryName(scriptPath) ?? throw new InvalidOperationException("无法获取脚本目录路径");
                if (!Directory.Exists(scriptDir))
                {
                    Directory.CreateDirectory(scriptDir);
                }

                // 写入 Python 脚本
                await File.WriteAllTextAsync(scriptPath, GetPythonScriptContent());

                // 获取 Python 解释器路径
                string pythonPath = _configuration.GetValue<string>("PythonSettings:PythonPath") ?? "python";

                // 创建并配置进程
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = scriptPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // 启动进程
                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        _logger.LogError("无法启动 Python 脚本");
                        return new ImportResult { Success = false, Message = "无法启动 Python 脚本", Count = 0 };
                    }

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        _logger.LogError("Python 脚本执行失败: {Error}", error);
                        return new ImportResult { Success = false, Message = $"脚本执行失败: {error}", Count = 0 };
                    }

                    // 解析 JSON 输出
                    try
                    {
                        var result = JsonDocument.Parse(output);
                        bool success = result.RootElement.GetProperty("success").GetBoolean();

                        if (success)
                        {
                            int count = result.RootElement.GetProperty("count").GetInt32();
                            _logger.LogInformation("成功导入 {Count} 条新记录", count);
                            return new ImportResult { Success = true, Message = "导入成功", Count = count };
                        }
                        else
                        {
                            string errorMessage = result.RootElement.GetProperty("error").GetString();
                            _logger.LogError("导入失败: {Error}", errorMessage);
                            return new ImportResult { Success = false, Message = $"导入失败: {errorMessage}", Count = 0 };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "解析 Python 脚本输出失败: {Output}", output);
                        return new ImportResult { Success = false, Message = $"解析脚本输出失败: {ex.Message}", Count = 0 };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导入商家标签数据时发生异常");
                return new ImportResult { Success = false, Message = $"导入过程中发生异常: {ex.Message}", Count = 0 };
            }
        }

        /// <summary>
        /// 直接使用 C# 导入标签
        /// </summary>
        /// <returns>导入结果</returns>
        public async Task<ImportResult> ImportMerchantTagsDirectlyAsync()
        {
            try
            {
                _logger.LogInformation("开始直接导入商家标签数据...");

                var shopTags = GetMerchantTagsData();
                int newCount = 0;

                foreach (var shop in shopTags)
                {
                    // 获取商家名称
                    string name = shop.Name;

                    // 合并所有标签到一个列表
                    List<string> tags = shop.Tags.SelectMany(sublist => sublist).ToList();

                    // 更新 MongoDB 文档
                    var filter = Builders<BsonDocument>.Filter.Eq("name", name);
                    var update = Builders<BsonDocument>.Update.Set("tags", new BsonArray(tags));

                    var updateResult = await _merchantsCollection.UpdateOneAsync(
                        filter,
                        update,
                        new UpdateOptions { IsUpsert = true }
                    );

                    if (updateResult.IsModifiedCountAvailable && updateResult.ModifiedCount > 0)
                    {
                        _logger.LogInformation("更新商家: {Name}, 匹配到 {Count} 条记录", name, updateResult.ModifiedCount);
                    }
                    else if (updateResult.UpsertedId != null)
                    {
                        _logger.LogInformation("插入新商家: {Name}", name);
                        newCount++;
                    }
                }

                _logger.LogInformation("成功导入 {Count} 条新记录", newCount);
                return new ImportResult { Success = true, Message = "导入成功", Count = newCount };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "直接导入商家标签数据时发生异常");
                return new ImportResult { Success = false, Message = $"导入过程中发生异常: {ex.Message}", Count = 0 };
            }
        }

        // 获取 Python 脚本内容
        private string GetPythonScriptContent()
        {
            // 返回 Python 脚本字符串...
            return @"
from pymongo import MongoClient
import sys
import json

client = MongoClient(""mongodb://localhost:27017/"")
db = client[""food_recommendation_db""]  
collection = db[""merchants""]  

shop_tags = [
    {
        ""name"": ""御品片皮鸭(武大校园店)"",
        ""tags"": [
            [""粤菜"", ""东北菜""],
            [""快餐店""],
            [""热菜"", ""小吃""]
        ]
    },
    {
        ""name"": ""鱼你在一起(武汉大学店)"",
        ""tags"": [
            [""川菜""],
            [""快餐店""],
            [""热菜""]
        ]
    },
    {
        ""name"": ""鲜炖大碗牛腩饭(武大店)"",
        ""tags"": [
            [""粤菜""],
            [""快餐店""],
            [""主食"", ""热菜""]
        ]
    },
    # ... 其他商家数据 ...
]

def update_merchants_with_tags():
    count = 0
    for shop in shop_tags:
        name = shop[""name""]
        tags = [tag for sublist in shop[""tags""] for tag in sublist]
        
        result = collection.update_one(
            {""name"": name},  
            {""$set"": {""tags"": tags}},  
            upsert=True  
        )

        if result.matched_count > 0:
            print(f""更新商家: {name}, 匹配到 {result.matched_count} 条记录"")
        else:
            print(f""插入新商家: {name}"")
            count += 1

    return count

if __name__ == ""__main__"":
    try:
        result = update_merchants_with_tags()
        print(json.dumps({""success"": True, ""count"": result}))
    except Exception as e:
        print(json.dumps({""success"": False, ""error"": str(e)}))
";
        }

        // 获取商家标签数据
        private List<MerchantTagData> GetMerchantTagsData()
        {
            // 返回商家标签数据...
            return new List<MerchantTagData>
            {
                // 省略具体内容...
            };
        }

        // 内部类，用于商家标签数据结构
        private class MerchantTagData
        {
            public string Name { get; set; } = string.Empty;
            public List<List<string>> Tags { get; set; } = new List<List<string>>();
        }
    }
}