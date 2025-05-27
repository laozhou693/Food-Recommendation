const {
  request
} = require('../../utils/api');

Page({
  data: {
    username: 'meishi',
    password: '123123123',
  },
  onLoad() {
    this.fetchUserInfo();
  },
  // 验证登录接口
  fetchUserInfo(e) {
    const {
      username,
      password
    } = e.detail.value;
    wx.request({
      url: 'http://localhost:5299/api/Auth/login',
      method: 'POST',
      data: {
        username: username, // 使用页面中的用户名
        password: password,
        email: 'ceshi@qq.com'
      },
      success: (res) => {
        console.log('====',res.data);
        if (res.data.statusCode == 200) {
          console.log('token', res.data.token);
          wx.setStorageSync('token', res.data.data.token);
          wx.setStorageSync('userInfo', res.data.data.user);

          setTimeout(() => {
            wx.switchTab({
              url: '/pages/index/index',
            })
          }, 1000)
        }

      },
      fail: (err) => {
        wx.showToast({
          title: '网络错误，请重试',
          icon: 'none'
        });
      },
      complete: () => {
        this.setData({
          loading: false
        });
      }
    });

    // request('users/login', 'POST', {
    //     username: username, // 使用页面中的用户名
    //     password: password,
    //     email: 'ceshi@qq.com'
    //   })
    //   .then(res => {
    //     console.log(res);
    //     // this.setData({
    //     //   userInfo: data
    //     // });
    //     wx.setStorageSync('token', res.Token);
    //     wx.setStorageSync('userInfo', res);

    //     setTimeout(() => {
    //       wx.switchTab({
    //         url: '/pages/index/index',
    //       })
    //     }, 1000)
    //   })
    //   .catch(error => {
    //     console.error('Error fetching user info:', error);
    //     wx.showToast({
    //       title: '获取用户信息失败',
    //       icon: 'none'
    //     });
    //   });
  },

  handleUsernameInput(e) {
    this.setData({
      username: e.detail.value
    });
  },

  handlePasswordInput(e) {
    this.setData({
      password: e.detail.value
    });
  },

  handleLogin(e) {
    const {
      username,
      password
    } = e.detail.value;

    if (!username) {
      wx.showToast({
        title: '请输入用户名',
        icon: 'none'
      });
      return;
    }

    if (!password) {
      wx.showToast({
        title: '请输入密码',
        icon: 'none'
      });
      return;
    }

    // 模拟登录请求
    wx.showLoading({
      title: '登录中...'
    });

    setTimeout(() => {
      // 模拟登录成功返回的用户数据
      const userInfo = {
        nickName: username,
        avatarUrl: 'https://example.com/avatars/' + username + '.jpg',
        phone: '138****' + Math.floor(Math.random() * 10000).toString().padStart(4, '0')
      };

      // 模拟收藏数据
      const favoriteList = this.generateMockFavorites();

      // 保存到本地缓存
      wx.setStorageSync('userInfo', userInfo);
      wx.setStorageSync('favoriteList', favoriteList);

      wx.hideLoading();
      wx.showToast({
        title: '登录成功',
        icon: 'success'
      });

      // 跳转到首页
      wx.switchTab({
        url: '/pages/index/index'
      });
    }, 1500);
  },

  // 生成模拟收藏数据
  generateMockFavorites() {
    const categories = ['fastfood', 'hotpot', 'japanese', 'western'];
    const count = Math.floor(Math.random() * 5) + 3; // 3-7个收藏

    return Array.from({
      length: count
    }, (_, i) => {
      const category = categories[Math.floor(Math.random() * categories.length)];
      return {
        id: `food-${i}`,
        name: this.getRandomFoodName(category),
        image: this.getRandomImage(category),
        rating: (Math.random() * 1 + 4).toFixed(1)
      };
    });
  },

  getRandomFoodName(category) {
    const names = {
      fastfood: ['肯德基', '麦当劳', '汉堡王', '赛百味', '真功夫'],
      hotpot: ['海底捞', '小龙坎', '大龙燚', '呷哺呷哺', '凑凑火锅'],
      japanese: ['将太无二', '村上一屋', '元气寿司', '筑底食堂', '铃木食堂'],
      western: ['必胜客', '达美乐', '牛排家', '蓝蛙', '星期五餐厅']
    };
    return names[category][Math.floor(Math.random() * names[category].length)];
  },

  getRandomImage(category) {
    const baseUrl = 'https://example.com/images/';
    const images = {
      fastfood: ['fastfood1.jpg', 'fastfood2.jpg', 'fastfood3.jpg'],
      hotpot: ['hotpot1.jpg', 'hotpot2.jpg', 'hotpot3.jpg'],
      japanese: ['japanese1.jpg', 'japanese2.jpg', 'japanese3.jpg'],
      western: ['western1.jpg', 'western2.jpg', 'western3.jpg']
    };
    return baseUrl + images[category][Math.floor(Math.random() * images[category].length)];
  }
});