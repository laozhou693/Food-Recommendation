// api.js

const BASE_URL = 'http://localhost:5199/api/'; // 替换为你的 API 基础 URL

// 封装请求
const request = (url, method, data, header = {}) => {
  return new Promise((resolve, reject) => {
    wx.request({
      url: BASE_URL + url,
      method: method,
      data: data,
      header: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + getToken(), // 携带 token
        ...header
      },
      success(res) {
        // if (res.statusCode === 200) {
          resolve(res.data);
        // } else {
        //   reject(res.data);
        // }
      },
      fail(err) {
        reject(err);
      }
    });
  });
};

// 获取 token
const getToken = () => {
  // 这里可以从缓存中获取 token，比如使用 wx.getStorageSync
  const token = wx.getStorageSync('token');
  return token || '';
};

// 导出方法
module.exports = {
  request,
  getToken
};