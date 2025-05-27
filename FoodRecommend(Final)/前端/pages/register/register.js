Page({
  data: {
    username: '',
    password: '',
    confirmPassword: '',
    mobile: '',
    captcha: '',
    isAgreed: false,
    loading: false,
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
  
  // 手机号输入处理
  handleMobileInput(e) {
    this.setData({
      mobile: e.detail.value
    });
  },
  
  // 验证码输入处理
  handleCaptchaInput(e) {
    this.setData({
      captcha: e.detail.value
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
    
    if (this.data.showMobileField) {
      if (!mobile) {
        wx.showToast({
          title: '请输入手机号',
          icon: 'none'
        });
        return;
      }
      
      if (!/^1[3-9]\d{9}$/.test(mobile)) {
        wx.showToast({
          title: '手机号格式不正确',
          icon: 'none'
        });
        return;
      }
      
      // if (!captcha) {
      //   wx.showToast({
      //     title: '请输入验证码',
      //     icon: 'none'
      //   });
      //   return;
      // }
      
      if (!/^\d{6}$/.test(captcha)) {
        wx.showToast({
          title: '验证码需6位数字',
          icon: 'none'
        });
        return;
      }
    }
    
    if (!this.data.isAgreed) {
      wx.showToast({
        title: '请同意用户协议和隐私政策',
        icon: 'none'
      });
      return;
    }
    
    // this.setData({ loading: true });
    
    // 这里替换为实际的注册API调用
    // 模拟注册请求
      // this.setData({ loading: false });
      
      // 注册成功处理
      // wx.showToast({
      //   title: '注册成功',
      //   icon: 'success'
      // });
      
      // 跳转到登录页面
      // wx.redirectTo({
      //   url: '/pages/login/login'
      // });
      
      // 实际开发中应该这样处理：
      wx.request({
        url: 'http://localhost:5299/api/Auth/register',
        method: 'POST',
        data: { 
          username, 
          password,
        },
        success: (res) => {
          if (res.data.code === 200) {
            // 处理注册成功逻辑
            wx.showToast({
              title: '注册成功',
              icon: 'none'
            });
            wx.switchTab({
              url: '/page/index/index',
            })
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
      
  },
  
  // 切换是否显示手机号字段
  toggleMobileField() {
    this.setData({
      showMobileField: !this.data.showMobileField
    });
  }
});