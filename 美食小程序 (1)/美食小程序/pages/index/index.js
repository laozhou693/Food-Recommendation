Page({
  data: {
    searchKeyword: '',
    filterIndex: 0,
    filterOptions: [
      { label: '全部美食', value: 'all' },
      { label: '快餐简餐', value: 'fastfood' },
      { label: '火锅', value: 'hotpot' },
      { label: '日料', value: 'japanese' },
      { label: '西餐', value: 'western' }
    ],
    sortType: 'recommend', // recommend/distance/rating
    foodList: [],
    isLoading: false,
    hasMore: true,
    page: 1,
    pageSize: 10
  },
  
  onLoad() {
    this.loadFoodData();
  },
  
  // 加载美食数据
  loadFoodData() {
    if (this.data.isLoading || !this.data.hasMore) return;
    
    this.setData({ isLoading: true });
    
    // 模拟API请求
    setTimeout(() => {
      const newData = this.generateMockData();
      this.setData({
        foodList: this.data.page === 1 ? newData : [...this.data.foodList, ...newData],
        isLoading: false,
        hasMore: this.data.page < 3, // 模拟只有3页数据
        page: this.data.page + 1
      });
    }, 800);
  },
  
  // 生成模拟数据
  generateMockData() {
    const categories = ['fastfood', 'hotpot', 'japanese', 'western'];
    const mockData = [];
    
    for (let i = 0; i < this.data.pageSize; i++) {
      const category = categories[Math.floor(Math.random() * categories.length)];
      const distance = (Math.random() * 5).toFixed(1);
      const rating = (Math.random() * 1 + 4).toFixed(1);
      const isRecommended = Math.random() > 0.7; // 30%概率是推荐菜品
      
      mockData.push({
        id: `food-${this.data.page}-${i}`,
        name: this.getRandomFoodName(category),
        description: this.getRandomDescription(category),
        image: this.getRandomImage(category),
        tags: this.getRandomTags(category),
        distance,
        rating,
        prices: {
          eleme: (Math.random() * 10 + 15).toFixed(1),
          meituan: (Math.random() * 10 + 15).toFixed(1)
        },
        category,
        isRecommended // 新增推荐标记
      });
    }
    
    // 根据排序类型排序
    return this.sortFoodList(mockData);
  },
  
  // 排序方法
  sortFoodList(list) {
    const sortType = this.data.sortType;
    
    if (sortType === 'recommend') {
      return list.sort((a, b) => {
        // 优先显示推荐菜品，然后按评分排序
        if (a.isRecommended && !b.isRecommended) return -1;
        if (!a.isRecommended && b.isRecommended) return 1;
        return b.rating - a.rating;
      });
    } else if (sortType === 'distance') {
      return list.sort((a, b) => a.distance - b.distance);
    } else {
      return list.sort((a, b) => b.rating - a.rating);
    }
  },
  
  // 获取随机美食名称
  getRandomFoodName(category) {
    const names = {
      fastfood: ['肯德基', '麦当劳', '汉堡王', '赛百味', '真功夫'],
      hotpot: ['海底捞', '小龙坎', '大龙燚', '呷哺呷哺', '凑凑火锅'],
      japanese: ['将太无二', '村上一屋', '元气寿司', '筑底食堂', '铃木食堂'],
      western: ['必胜客', '达美乐', '牛排家', '蓝蛙', '星期五餐厅']
    };
    return names[category][Math.floor(Math.random() * names[category].length)];
  },
  
  // 获取随机描述
  getRandomDescription(category) {
    const descs = {
      fastfood: ['快捷方便的美式快餐', '全球连锁快餐品牌', '美味汉堡与炸鸡'],
      hotpot: ['正宗川味火锅', '新鲜食材现场制作', '特色锅底风味独特'],
      japanese: ['新鲜刺身寿司', '日式居酒屋风格', '正宗日本料理'],
      western: ['经典西式餐点', '优雅用餐环境', '进口食材精心烹制']
    };
    return descs[category][Math.floor(Math.random() * descs[category].length)];
  },
  
  // 获取随机图片URL
  getRandomImage(category) {
    const baseUrl = 'https://example.com/images/'; // 替换为实际图片路径
    const images = {
      fastfood: ['fastfood1.jpg', 'fastfood2.jpg', 'fastfood3.jpg'],
      hotpot: ['hotpot1.jpg', 'hotpot2.jpg', 'hotpot3.jpg'],
      japanese: ['japanese1.jpg', 'japanese2.jpg', 'japanese3.jpg'],
      western: ['western1.jpg', 'western2.jpg', 'western3.jpg']
    };
    return baseUrl + images[category][Math.floor(Math.random() * images[category].length)];
  },
  
  // 获取随机标签
  getRandomTags(category) {
    const tags = {
      fastfood: ['快餐', '汉堡', '炸鸡', '套餐'],
      hotpot: ['火锅', '麻辣', '牛肉', '毛肚'],
      japanese: ['寿司', '刺身', '清酒', '天妇罗'],
      western: ['牛排', '披萨', '意面', '沙拉']
    };
    
    const result = [];
    const count = Math.floor(Math.random() * 2) + 2; // 2-3个标签
    const shuffled = [...tags[category]].sort(() => 0.5 - Math.random());
    
    return shuffled.slice(0, count);
  },
  
  // 搜索输入处理
  handleSearchInput(e) {
    this.setData({
      searchKeyword: e.detail.value
    });
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
    this.setData({
      filterIndex: e.detail.value,
      page: 1,
      hasMore: true
    });
    this.loadFoodData();
  },
  
  // 改变排序方式
  changeSortType(e) {
    const type = e.currentTarget.dataset.type;
    if (this.data.sortType === type) return;
    
    this.setData({
      sortType: type,
      page: 1,
      hasMore: true
    });
    
    // 对现有数据进行排序
    this.setData({
      foodList: this.sortFoodList([...this.data.foodList])
    });
  },
  
  // 滚动加载更多
  onReachBottom() {
    this.loadFoodData();
  },
  
  // 下拉刷新
  onPullDownRefresh() {
    this.setData({
      page: 1,
      hasMore: true
    });
    this.loadFoodData(() => {
      wx.stopPullDownRefresh();
    });
  },
  
  // 跳转到详情页
  navigateToDetail(e) {
    const foodId = e.currentTarget.dataset.id;
    wx.navigateTo({
      url: `/pages/food-detail/food-detail?id=${foodId}`
    });
  }
});