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
    
    // 这里替换为实际的登录API调用
    // 模拟登录请求
    setTimeout(() => {
      
      // 登录成功处理
      wx.showToast({
        title: '登录成功',
        icon: 'success'
      });
      
      // 跳转到首页
      wx.switchTab({
        url: '/pages/index/index'
      });
      
      // 实际开发中应该这样处理：
      // wx.request({
      //   url: '你的登录接口',
      //   method: 'POST',
      //   data: { username, password },
      //   success: (res) => {
      //     // 处理登录成功逻辑
      //   },
      //   fail: (err) => {
      //     // 处理错误
      //   },
      //   complete: () => {
      //     this.setData({ loading: false });
      //   }
      // });
      
    }, 1500);
  }
});