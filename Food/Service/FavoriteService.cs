using Food.Models;
using Food.Repository;

namespace Food.Service
{
    public class FavoriteService
    {
        private readonly UserRepository _userRepository;
        private readonly MerchantRepository _merchantRepository;

        public FavoriteService(UserRepository userRepository, MerchantRepository merchantRepository)
        {
            _userRepository = userRepository;
            _merchantRepository = merchantRepository;
        }

        // 添加收藏
        public async Task<bool> AddFavoriteAsync(string userId, string merchantId)
        {
            // 确认商家存在
            var merchant = await _merchantRepository.GetByIdAsync(merchantId);
            if (merchant == null)
            {
                return false;
            }

            // 添加到用户收藏
            var success = await _userRepository.AddFavoriteAsync(userId, merchantId);

            if (success)
            {
                // 增加商家收藏计数
                await _merchantRepository.IncrementFavoriteCountAsync(merchantId);
            }

            return success;
        }

        // 移除收藏
        public async Task<bool> RemoveFavoriteAsync(string userId, string merchantId)
        {
            var success = await _userRepository.RemoveFavoriteAsync(userId, merchantId);

            if (success)
            {
                // 减少商家收藏计数
                await _merchantRepository.DecrementFavoriteCountAsync(merchantId);
            }

            return success;
        }

        // 获取用户收藏的商家
        public async Task<List<Merchant>> GetUserFavoritesAsync(string userId)
        {
            var favoriteIds = await _userRepository.GetFavoritesAsync(userId);

            if (favoriteIds.Count == 0)
            {
                return new List<Merchant>();
            }

            return await _merchantRepository.GetByIdsAsync(favoriteIds);
        }

        // 检查商家是否被收藏
        public async Task<bool> IsFavoritedAsync(string userId, string merchantId)
        {
            var favorites = await _userRepository.GetFavoritesAsync(userId);
            return favorites.Contains(merchantId);
        }

        // 查询商家被收藏的次数
        public async Task<int> GetFavoriteCountAsync(string merchantId)
        {
            var merchant = await _merchantRepository.GetByIdAsync(merchantId);
            return merchant?.FavoriteCount ?? 0;
        }
    }
}
