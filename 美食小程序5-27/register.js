Page({
  data: {
    username: '',
    password: '',
    confirmPassword: '',
    mobile: '',
    captcha: '',
    isAgreed: false,
    loading: false,
    isGettingCaptcha: false,
    captchaBtnText: '获取验证码',
    countdown: 60
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
  
  // 确认密码输入处理
  handleConfirmPasswordInput(e) {
    this.setData({
      confirmPassword: e.detail.value
    });
  },
  
  
  // 切换协议同意状态
  toggleAgreement() {
    this.setData({
      isAgreed: !this.data.isAgreed
    });
  },
  

  // 注册处理
  handleRegister(e) {
    const { username, password, confirmPassword, mobile, captcha } = e.detail.value;
    
    // 表单验证
    if (!username) {
      wx.showToast({
        title: '请输入用户名',
        icon: 'none'
      });
      return;
    }
    
    if (!/^[a-zA-Z0-9]{6,16}$/.test(username)) {
      wx.showToast({
        title: '用户名需6-16位字母或数字',
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
    
    if (password.length < 6 || password.length > 16) {
      wx.showToast({
        title: '密码需6-16位字符',
        icon: 'none'
      });
      return;
    }
    
    if (password !== confirmPassword) {
      wx.showToast({
        title: '两次输入的密码不一致',
        icon: 'none'
      });
      return;
    }
    
    
    if (!this.data.isAgreed) {
      wx.showToast({
        title: '请同意用户协议和隐私政策',
        icon: 'none'
      });
      return;
    }
    
    this.setData({ loading: true });
    
    wx.request({
    url: 'http://localhost:3000', // 替换为实际的注册接口地址
    method: 'POST',
    data: { 
         username, 
         password
      },
      success: (res) => {
           if (res.data.code === 200) {
            // 处理注册成功逻辑
            wx.redirectTo({
           url: '/pages/login/login'
          });  
           } else {
             wx.showToast({
               title: res.data.message || '注册失败',
               icon: 'none'
             });
           }
         },
         fail: (err) => {
           wx.showToast({
             title: '网络错误，请重试',
             icon: 'none'
           });
         },
         complete: () => {
           this.setData({ loading: false });
         }
       });
    
  }
  
});