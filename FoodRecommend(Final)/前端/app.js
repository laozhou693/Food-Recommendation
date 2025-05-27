// app.js
App({
  onLaunch() {


    // 登录
    wx.login({
      success: res => {
        // 发送 res.code 到后台换取 openId, sessionKey, unionId
      }
    })
  },
  // 获取 token
  getToken() {
    // 这里可以从缓存中获取 token，比如使用 wx.getStorageSync
    const token = wx.getStorageSync('token');
    return token || '';
  },
  globalData: {
    userInfo: null
  }
})