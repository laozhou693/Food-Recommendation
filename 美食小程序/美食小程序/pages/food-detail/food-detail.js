Page({
  data: {
    foodDetail: null
  },
  
  onLoad(options) {
    const foodId = options.id;
    this.loadFoodDetail(foodId);
  },
  
  // 加载美食详情
  loadFoodDetail(foodId) {
    // 模拟API请求
    setTimeout(() => {
      const mockData = this.generateMockDetail(foodId);
      this.setData({
        foodDetail: mockData
      });
    }, 500);
  },
  
  // 生成模拟详情数据
  generateMockDetail(foodId) {
    const categories = ['fastfood', 'hotpot', 'japanese', 'western'];
    const category = categories[Math.floor(Math.random() * categories.length)];
    const distance = (Math.random() * 5).toFixed(1);
    const rating = (Math.random() * 1 + 4).toFixed(1);
    
    return {
      id: foodId,
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
      store: {
        address: '北京市朝阳区某某路123号',
        businessHours: '10:00-22:00',
        phone: '13800138000'
      }
    };
  },
  
  // 获取随机美食名称 (与首页相同)
  getRandomFoodName(category) {
    const names = {
      fastfood: ['肯德基', '麦当劳', '汉堡王', '赛百味', '真功夫'],
      hotpot: ['海底捞', '小龙坎', '大龙燚', '呷哺呷哺', '凑凑火锅'],
      japanese: ['将太无二', '村上一屋', '元气寿司', '筑底食堂', '铃木食堂'],
      western: ['必胜客', '达美乐', '牛排家', '蓝蛙', '星期五餐厅']
    };
    return names[category][Math.floor(Math.random() * names[category].length)];
  },
  
  // 获取随机描述 (与首页相同)
  getRandomDescription(category) {
    const descs = {
      fastfood: ['快捷方便的美式快餐', '全球连锁快餐品牌', '美味汉堡与炸鸡'],
      hotpot: ['正宗川味火锅', '新鲜食材现场制作', '特色锅底风味独特'],
      japanese: ['新鲜刺身寿司', '日式居酒屋风格', '正宗日本料理'],
      western: ['经典西式餐点', '优雅用餐环境', '进口食材精心烹制']
    };
    return descs[category][Math.floor(Math.random() * descs[category].length)];
  },
  
  // 获取随机图片URL (与首页相同)
  getRandomImage(category) {
    const baseUrl = 'https://example.com/images/';
    const images = {
      fastfood: ['fastfood1.jpg', 'fastfood2.jpg', 'fastfood3.jpg'],
      hotpot: ['hotpot1.jpg', 'hotpot2.jpg', 'hotpot3.jpg'],
      japanese: ['japanese1.jpg', 'japanese2.jpg', 'japanese3.jpg'],
      western: ['western1.jpg', 'western2.jpg', 'western3.jpg']
    };
    return baseUrl + images[category][Math.floor(Math.random() * images[category].length)];
  },
  
  // 获取随机标签 (与首页相同)
  getRandomTags(category) {
    const tags = {
      fastfood: ['快餐', '汉堡', '炸鸡', '套餐'],
      hotpot: ['火锅', '麻辣', '牛肉', '毛肚'],
      japanese: ['寿司', '刺身', '清酒', '天妇罗'],
      western: ['牛排', '披萨', '意面', '沙拉']
    };
    
    const count = Math.floor(Math.random() * 2) + 2;
    const shuffled = [...tags[category]].sort(() => 0.5 - Math.random());
    return shuffled.slice(0, count);
  },
  
  // 跳转到外卖平台
  navigateToPlatform(e) {
    const platform = e.currentTarget.dataset.platform;
    const foodName = this.data.foodDetail.name;
    
    wx.showModal({
      title: '提示',
      content: `即将跳转到${platform === 'eleme' ? '饿了么' : '美团'}购买${foodName}`,
      success(res) {
        if (res.confirm) {
          // 实际开发中这里应该跳转到对应平台的Deep Link或小程序
          wx.showToast({
            title: `跳转到${platform === 'eleme' ? '饿了么' : '美团'}`,
            icon: 'none'
          });
        }
      }
    });
  }
});