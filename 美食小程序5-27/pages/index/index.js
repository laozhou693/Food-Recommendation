const {
  request
} = require('../../utils/api');
Page({
  data: {
    searchKeyword: '',
    filterIndex: 0,
    filterName: '全部分类',
    filterOptions: [{
        label: '全部美食',
        value: 'all'
      },
      {
        label: '快餐简餐',
        value: 'fastfood'
      },
      {
        label: '火锅',
        value: 'hotpot'
      },
      {
        label: '日料',
        value: 'japanese'
      },
      {
        label: '西餐',
        value: 'western'
      }
    ],
    sortType: 'recommended', // recommended,nearest,highest-rated
    foodList: [],
    isLoading: false,
    hasMore: true,
    page: 1,
    pageSize: 10
  },
  getList() {
    wx.request({
      url: 'http://localhost:5299/api/Categories',
      header: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + getApp().getToken(), // 携带 token
      },
      method: 'GET',
      data: {},
      success: (res) => {
        console.log(res, '========');
        if (res.data.statusCode == 200) {
          this.setData({
            filterOptions: [{
              name: '全部分类',
              Id: 'abcdefg1234567'
            }, ...res.data.data.categories].map(item => {
              return {
                label: item.name,
                value: item.Id
              }
            })
          })
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
    // request('Categories', 'POST', {})
    //   .then(res => {
    //     console.log(res);

    //   })
    //   .catch(error => {
    //     console.error('Error fetching user info:', error);
    //   });
  },

  onLoad() {
    this.getList()
    this.loadFoodData();
  },

  // 加载美食数据
  loadFoodData() {
    let encodedKeyword = encodeURIComponent(this.data.searchKeyword);
    if(this.data.searchKeyword.length==0){
      encodedKeyword=encodeURIComponent("空值")
    }
    const encodeTag = encodeURIComponent(this.data.filterName);

    console.log(encodedKeyword, '=====查看请求参数', encodeTag);

    // 构建查询字符串
    let queryParams = [];
    if (encodedKeyword) queryParams.push(`keyword=${encodedKeyword}`);
    if (encodeTag) queryParams.push(`tag=${encodeTag}`);

    let url = `http://localhost:5299/api/merchants/search`;
    if (queryParams.length > 0) {
      url += `?${queryParams.join('&')}`;
    }

    console.log(url, '=======最终请求URL');

    wx.request({
      url: url,
      header: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + getApp().getToken(), // 携带 token
      },
      method: 'GET',
      success: (res) => {
        console.log(res.data, 'meisjuti');
        if (res.statusCode == 200) {
          // console.log(res.data);
          this.setData({
            foodList: this.sortFoodList(res.data.data)
          });
            // console.log(foodList);
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


  // 排序方法
  sortFoodList(list) {
    const sortType = this.data.sortType;
    console.log(sortType)
    if (sortType === 'recommended') {
      return list.sort((a, b) => {
        // 优先显示推荐菜品，然后按评分排序
        // if (a.isRecommended && !b.isRecommended) return -1;
        // if (!a.isRecommended && b.isRecommended) return 1;
        return (b.rating*0.7+b.favoriteCount*0.3) - (a.rating*0.7+a.favoriteCount*0.3);
      });
    } else if (sortType === 'nearest') {
      return list.sort((a, b) => a.distance - b.distance);
    } else {
      return list.sort((a, b) => (b.ratingEle+b.ratingMeituan) - (a.ratingEle+a.ratingMeituan));
    }
  },

  // 搜索输入处理
  handleSearchInput(e) {
    this.setData({
      searchKeyword: e.detail.value
    });
    // this.loadFoodData();
  },

  // 执行搜索
  handleSearch() {
    this.setData({
      page: 1,
      hasMore: true
    });
    this.loadFoodData();
  },

  // 筛选条件变化
  handleFilterChange(e) {
    console.log(e.detail, this.data.filterOptions);
    const selectedIndex = Number(e.detail.value);
    // const matchedItem = this.data.filterOptions.find(item => item.value === targetValue);
    const matchedItem = this.data.filterOptions[selectedIndex];
    console.log(matchedItem);
    if (matchedItem) {
      console.log(matchedItem.label);
      this.setData({
        filterIndex: e.detail.value,
        filterName: matchedItem.label
      });
    } else {
      console.log("未找到匹配项");
    }

    this.loadFoodData();
  },

  // 改变排序方式
  changeSortType(e) {
    const type = e.currentTarget.dataset.type;
    if (this.data.sortType === type) return;
    
    let encodedKeyword = encodeURIComponent(this.data.searchKeyword);
    if(this.data.searchKeyword.length==0){
      encodedKeyword=encodeURIComponent("空值")
    }
    const encodeTag = encodeURIComponent(this.data.filterName);

    console.log(encodedKeyword, '=====查看请求参数', encodeTag);

    // 构建查询字符串
    let queryParams = [];
    if (encodedKeyword) queryParams.push(`keyword=${encodedKeyword}`);
    if (encodeTag) queryParams.push(`tag=${encodeTag}`);

    let url = `http://localhost:5299/api/merchants/search`;
    if (queryParams.length > 0) {
      url += `?${queryParams.join('&')}`;
    }

    console.log(url, '=======最终请求URL');

    wx.request({
      url: url,
      header: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + getApp().getToken(), // 携带 token
      },
      method: 'GET',
      success: (res) => {
        console.log(res.data, 'meisjuti');
        if (res.statusCode == 200) {
          // console.log(res.data);
          this.setData({
            sortType:type,
          });
          this.setData({
            foodList: this.sortFoodList(res.data.data)
          });
            // console.log(foodList);
        }
      },
      fail: (err) => {
        wx.showToast({
          title: '网络错误，请重试',
          icon: 'none'
        });
      }
    }
    // 对现有数据进行排序
    // this.setData({
    //   foodList: this.sortFoodList([...this.data.foodList])
    // })
    )},


  // 跳转到详情页
  navigateToDetail(e) {
    const merchant = e.currentTarget.dataset.merchant;
    const str = JSON.stringify(merchant);
    wx.navigateTo({
      url: `/pages/food-detail/food-detail?merchant=${encodeURIComponent(str)}`
    });
  }
});