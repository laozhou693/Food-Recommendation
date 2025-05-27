Page({
  data: {
    userInfo: {},
    favoriteList: [],
  },

  onLoad() {
    this.setData({
      userInfo: wx.getStorageSync('userInfo') || {}
    })
    this.loadData();
  },
  loadData() {
    wx.request({
      url: `http://localhost:5299/api/favorites`,
      header: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + getApp().getToken(), // 携带 token
      },
      method: 'GET',
      success: (res) => {
        console.log(res.data.data,'收藏列表');
        const { data, statusCode } = res.data;
        if (statusCode == 200) {
          console.log(data, statusCode, '======')
          this.setData({
            favoriteList: data.favorites,
          })

          console.log(this.data.favoriteList);
        }

      },
      fail: (err) => {
        wx.showToast({
          title: '网络错误，请重试',
          icon: 'none'
        });
      }
    });
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
    
    const merchant = e.currentTarget.dataset.merchant;
    const str = JSON.stringify(merchant);
    wx.navigateTo({
      url: `/pages/food-detail/food-detail?merchant=${encodeURIComponent(str)}`
    });
  },

  navigateToIndex() {
    wx.switchTab({
      url: '/pages/index/index'
    });
  }
});