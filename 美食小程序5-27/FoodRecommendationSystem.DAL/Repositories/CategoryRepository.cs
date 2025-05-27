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
    /// 分类数据访问仓储
    /// </summary>
    public class CategoryRepository
    {
        private readonly IMongoCollection<Category> _collection;

        public CategoryRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Category>("categories");

            // 创建类型索引
            var typeIndexModel = new CreateIndexModel<Category>("{ Type: 1 }");
            _collection.Indexes.CreateOne(typeIndexModel);

            // 创建父分类索引
            var parentIndexModel = new CreateIndexModel<Category>("{ ParentCategoryId: 1 }");
            _collection.Indexes.CreateOne(parentIndexModel);

            // 创建唯一名称索引(按类型)
            var nameIndexModel = new CreateIndexModel<Category>(
                "{ Type: 1, Name: 1 }",
                new CreateIndexOptions { Unique = true });
            _collection.Indexes.CreateOne(nameIndexModel);
        }

        /// <summary>
        /// 获取所有分类
        /// </summary>
        public async Task<List<Category>> GetAllAsync()
        {
            return await _collection.Find(_ => true)
                .Sort(Builders<Category>.Sort.Ascending(c => c.SortOrder))
                .ToListAsync();
        }

        /// <summary>
        /// 根据类型获取分类
        /// </summary>
        public async Task<List<Category>> GetByTypeAsync(CategoryType type)
        {
            return await _collection.Find(c => c.Type == type)
                .Sort(Builders<Category>.Sort.Ascending(c => c.SortOrder))
                .ToListAsync();
        }

        /// <summary>
        /// 获取顶级分类(没有父分类的分类)
        /// </summary>
        public async Task<List<Category>> GetTopCategoriesAsync(CategoryType type)
        {
            return await _collection.Find(c => c.Type == type && c.ParentCategoryId == null)
                .Sort(Builders<Category>.Sort.Ascending(c => c.SortOrder))
                .ToListAsync();
        }

        /// <summary>
        /// 获取子分类
        /// </summary>
        public async Task<List<Category>> GetChildrenAsync(string parentId)
        {
            return await _collection.Find(c => c.ParentCategoryId == parentId)
                .Sort(Builders<Category>.Sort.Ascending(c => c.SortOrder))
                .ToListAsync();
        }

        /// <summary>
        /// 根据名称搜索分类
        /// </summary>
        public async Task<List<Category>> SearchByNameAsync(string name, CategoryType? type = null)
        {
            var builder = Builders<Category>.Filter;
            var filter = builder.Regex(c => c.Name, new BsonRegularExpression(name, "i"));

            if (type.HasValue)
            {
                filter = filter & builder.Eq(c => c.Type, type.Value);
            }

            return await _collection.Find(filter).ToListAsync();
        }

        /// <summary>
        /// 创建分类
        /// </summary>
        public async Task CreateAsync(Category category)
        {
            if (string.IsNullOrEmpty(category.Id))
            {
                category.Id = ObjectId.GenerateNewId().ToString();
            }
            await _collection.InsertOneAsync(category);
        }

        /// <summary>
        /// 批量创建分类
        /// </summary>
        public async Task CreateManyAsync(List<Category> categories)
        {
            if (categories == null || categories.Count == 0)
                return;

            // 确保所有分类都有ID
            foreach (var category in categories)
            {
                if (string.IsNullOrEmpty(category.Id))
                {
                    category.Id = ObjectId.GenerateNewId().ToString();
                }
            }

            await _collection.InsertManyAsync(categories);
        }

        /// <summary>
        /// 更新分类
        /// </summary>
        public async Task UpdateAsync(string id, Category category)
        {
            await _collection.ReplaceOneAsync(c => c.Id == id, category);
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(c => c.Id == id);
        }

        /// <summary>
        /// 获取分类树(所有层级)
        /// </summary>
        public async Task<List<CategoryTreeNode>> GetCategoryTreeAsync(CategoryType type)
        {
            // 获取所有指定类型的分类
            var allCategories = await GetByTypeAsync(type);

            // 构建分类树
            var result = new List<CategoryTreeNode>();
            var categoryDict = new Dictionary<string, CategoryTreeNode>();

            // 先将所有分类转换为树节点
            foreach (var category in allCategories)
            {
                var node = new CategoryTreeNode
                {
                    Id = category.Id,
                    Name = category.Name,
                    IconUrl = category.IconUrl ?? string.Empty,
                    SortOrder = category.SortOrder,
                    Children = new List<CategoryTreeNode>()
                };

                categoryDict[category.Id] = node;
            }

            // 然后构建父子关系
            foreach (var category in allCategories)
            {
                if (string.IsNullOrEmpty(category.ParentCategoryId))
                {
                    // 顶级分类
                    result.Add(categoryDict[category.Id]);
                }
                else if (categoryDict.ContainsKey(category.ParentCategoryId))
                {
                    // 添加到父分类的子节点
                    categoryDict[category.ParentCategoryId].Children.Add(categoryDict[category.Id]);
                }
            }

            // 返回排序后的结果
            result.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            return result;
        }
    }

    /// <summary>
    /// 分类树节点
    /// </summary>
    public class CategoryTreeNode
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public required List<CategoryTreeNode>  Children { get; set; }
    }
}