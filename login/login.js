Page({
  data: {
    username: '',
    password: '',
  },
  
  // 用户名输入处理
  handleUsernameInput(e) {
    this.setData({
      username: e.detail.value
    });
  },
  
  // 密码输入处理
  handlePasswordInput(e) {
    this.setData({
      password: e.detail.value
    });
  },
  
  // 登录处理
  handleLogin(e) {
    const { username, password } = e.detail.value;
    
    // 简单验证
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
    
    
      
      
      wx.request({
      url: 'http://localhost:5000/api/users/profile', // 替换为实际的登录接口地址
      method: 'POST',
      data: { username, password },
      success: (res) => {
           if (res.statusCode === 200 && res.data.userId) {
          // 登录成功后保存 userId
          wx.setStorageSync('userId', res.data.userId);

          wx.showToast({
            title: '登录成功',
            icon: 'success'
          });

          // 跳转到首页或浏览页
          wx.switchTab({
            url: '/pages/index/index' 
          });
        }else {
          wx.showToast({
            title: '登录失败，请检查用户名和密码',
            icon: 'none'
          });
        }
      },
         fail: (err) => {
           // 处理错误
         },
         complete: () => {
           this.setData({ loading: false });
   }
      });
      
  }
});