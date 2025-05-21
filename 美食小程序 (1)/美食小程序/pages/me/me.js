Page({
  data: {
    userInfo: {},
    favoriteList: [],
  },

  onLoad() {
    this.loadData();
  },

  onShow() {
    this.loadData();
  },

  loadData() {
    const userInfo = wx.getStorageSync('userInfo') || {};
    const favoriteList = wx.getStorageSync('favoriteList') || [];
    this.setData({ 
      userInfo,
      favoriteList: this.addMockData(favoriteList)
    });
  },

  // 为收藏数据添加模拟的额外信息
  addMockData(list) {
    const categories = ['火锅', '日料', '西餐', '快餐', '烧烤'];
    return list.map(item => ({
      ...item,
      category: categories[Math.floor(Math.random() * categories.length)],
      price: Math.floor(Math.random() * 50) + 30 + '-' + (Math.floor(Math.random() * 50) + 60)
    }));
  },

  removeFavorite(e) {
    const id = e.currentTarget.dataset.id;
    wx.showModal({
      title: '提示',
      content: '确定要取消收藏吗？',
      success: (res) => {
        if (res.confirm) {
          const newList = this.data.favoriteList.filter(item => item.id !== id);
          wx.setStorageSync('favoriteList', newList);
          this.setData({ favoriteList: newList });
          
          wx.showToast({
            title: '已取消收藏',
            icon: 'none'
          });
          
        }
      }
    });
  },

  navigateToLogin() {
    wx.navigateTo({
      url: '/pages/login/login'
    });
  },

  navigateToDetail(e) {
    
    const foodId = e.currentTarget.dataset.id;
    wx.navigateTo({
      url: `/pages/food-detail/food-detail?id=${foodId}`
    });
  },

  navigateToIndex() {
    wx.switchTab({
      url: '/pages/index/index'
    });
  }
});