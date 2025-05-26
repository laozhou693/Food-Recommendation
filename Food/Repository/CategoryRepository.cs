using Food.Models;
using MongoDB.Driver;

namespace Food.Repository
{
    public class CategoryRepository
    {
        private readonly IMongoCollection<Category> _categories;

        public CategoryRepository(IMongoDatabase database)
        {
            _categories = database.GetCollection<Category>("categories");
        }

        // 获取所有分类
        public async Task<List<Category>> GetAllAsync()
        {
            return await _categories.Find(_ => true).SortBy(c => c.SortOrder).ToListAsync();
        }

        // 按类型获取分类
        public async Task<List<Category>> GetByTypeAsync(CategoryType type)
        {
            return await _categories.Find(c => c.Type == type).SortBy(c => c.SortOrder).ToListAsync();
        }

        // 获取单个分类
        public async Task<Category> GetByIdAsync(string id)
        {
            return await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        // 按名称获取分类
        public async Task<Category> GetByNameAsync(string name)
        {
            return await _categories.Find(c => c.Name == name).FirstOrDefaultAsync();
        }

        // 创建单个分类
        public async Task<Category> CreateAsync(Category category)
        {
            await _categories.InsertOneAsync(category);
            return category;
        }

        // 创建多个分类
        public async Task<IEnumerable<Category>> CreateManyAsync(IEnumerable<Category> categories)
        {
            await _categories.InsertManyAsync(categories);
            return categories;
        }

        // 更新分类
        public async Task<bool> UpdateAsync(Category category)
        {
            var result = await _categories.ReplaceOneAsync(c => c.Id == category.Id, category);
            return result.ModifiedCount > 0;
        }

        // 删除分类
        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _categories.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
