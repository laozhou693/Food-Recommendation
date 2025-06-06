from pymongo import MongoClient

# 1. 连接 MongoDB
client = MongoClient("mongodb://localhost:27017/")
db = client["food_recommendation_db"]  # 替换为你的数据库名
collection = db["merchants"]  # 替换为你的集合名


shop_tags=[
{"店铺": "御品片皮鸭", "标签": ["粤菜", "主食", "热菜", "凉菜"]},
{"店铺": "鱼你在一起", "标签": ["川菜", "热菜", "主食"]},
{"店铺": "鲜炖大碗牛腩饭", "标签": ["热菜", "主食", "快餐店"]},
{"店铺": "珞珈鲜可达水果店", "标签": ["小吃店", "小吃", "饮品"]},
{"店铺": "小熊荷包鸡", "标签": ["热菜", "主食", "快餐店"]},
{"店铺": "街头牛排・烤肉饭・芝士烤冷面", "标签": ["烧烤店", "主食", "快餐店"]},
{"店铺": "北京烤鸭", "标签": ["京菜", "主食", "热菜", "凉菜"]},
{"店铺": "郑恩强黏糊糊麻辣烫・麻辣拌・炸串", "标签": ["小吃店", "小吃", "火锅店"]},
{"店铺": "卤汁肴猪脚饭", "标签": ["粤菜", "主食", "热菜", "快餐店"]},
{"店铺": "味来小厨", "标签": ["家常菜", "热菜", "主食"]},
{"店铺": "黄蜀郎鸡公煲", "标签": ["川菜", "热菜", "主食", "火锅店"]},
{"店铺": "东北菜饺子馆", "标签": ["东北菜", "主食", "凉菜"]},
{"店铺": "骞山", "标签": ["未明确，需更多信息"]},
{"店铺": "有柒有喝・盖浇饭", "标签": ["主食", "热菜", "快餐店", "饮品店"]},
{"店铺": "超牛堡", "标签": ["西餐厅", "主食", "快餐店"]},
{"店铺": "匠心卤・热卤拌饭", "标签": ["热菜", "主食", "快餐店"]},
{"店铺": "良食汀铁板焦饭", "标签": ["主食", "热菜", "快餐店"]},
{"店铺": "蒙古烤鸡碳烤肉", "标签": ["烧烤店", "热菜", "主食"]},
{"店铺": "缘味先石锅饭", "标签": ["主食", "热菜", "快餐店"]},
{"店铺": "尚品麻辣鸡架", "标签": ["小吃店", "小吃", "凉菜"]},
{"店铺": "小湘港餐厅", "标签": ["湘菜", "热菜", "主食"]},
{"店铺": "三米粥铺", "标签": ["粥类", "主食", "汤类", "快餐店"]},
{"店铺": "乐芝士・中国披萨", "标签": ["西餐厅", "主食", "快餐店"]},
{"店铺": "奈斯炸鸡・烤鸡", "标签": ["小吃店", "小吃", "烧烤店"]},
{"店铺": "珞珈杂粮煎饼烤冷面恩施炕土豆", "标签": ["小吃店", "小吃", "鄂菜"]},
{"店铺": "生煎锅贴汤包", "标签": ["小吃店", "小吃", "主食"]},
{"店铺": "早春香稻米猛火炒饭・炒面・星选翻滚吧", "标签": ["主食", "热菜", "快餐店"]},
{"店铺": "淮南牛肉汤・锅贴・烧饼", "标签": ["小吃店", "小吃", "汤类", "主食"]},
{"店铺": "骨留香手撕大排", "标签": ["小吃店", "小吃", "热菜"]},
{"店铺": "拼壹碗蛋炒饭炒面", "标签": ["主食", "热菜", "快餐店"]},
{"店铺": "薄荷轻食", "标签": ["轻食", "主食", "热菜"]},
{"店铺": "一品鲜小海鲜", "标签": ["粤菜", "热菜", "主食"]},
{"店铺": "安庆特色馄饨饺子", "标签": ["小吃店", "小吃", "主食"]},
{"店铺": "胡乐乐猪脚烧腊饭", "标签": ["粤菜", "主食", "热菜", "快餐店"]},
{"店铺": "阿疆 X 味・新疆炒米粉", "标签": ["西北菜", "主食", "热菜"]},
{"店铺": "恩施土家扣肉", "标签": ["鄂菜", "热菜", "主食"]},
{"店铺": "湘伴食光・风味现炒・川湘家常", "标签": ["湘菜", "川菜", "热菜", "主食"]},
{"店铺": "郭记手撕鸡", "标签": ["小吃店", "小吃", "凉菜"]},
{"店铺": "老猫无骨烤鱼", "标签": ["川菜", "热菜", "主食"]},
{"店铺": "鸡柳大人", "标签": ["小吃店", "小吃"]},
{"店铺": "遵义羊肉粉", "标签": ["小吃店", "小吃", "主食"]},
{"店铺": "老成都冒烤鸭", "标签": ["川菜", "热菜", "主食", "凉菜"]},
{"店铺": "王记隆江猪脚饭", "标签": ["粤菜", "主食", "热菜", "快餐店"]},
{"店铺": "关东水饺", "标签": ["东北菜", "主食", "凉菜"]},
{"店铺": "蔡明纬热干面", "标签": ["鄂菜", "小吃店", "小吃", "主食"]},
{"店铺": "香茵波克现烤汉堡", "标签": ["西餐厅", "主食", "快餐店"]},
{"店铺": "黑粉你・肥牛火锅粉", "标签": ["小吃店", "小吃", "主食", "火锅店"]},
{"店铺": "5 号隆记・隆江猪脚饭・广式烧腊", "标签": ["粤菜", "主食", "热菜", "快餐店"]},
{"店铺": "纯手工馄饨水饺养生汤", "标签": ["小吃店", "小吃", "主食", "汤类"]},
{"店铺": "烤鸭世家", "标签": ["京菜", "主食", "热菜", "凉菜"]},
{"店铺": "武大大牛腩叉烧饭", "标签": ["粤菜", "主食", "热菜", "快餐店"]},
{"店铺": "东欧食堂", "标签": ["西餐", "主食", "热菜"]},
{"店铺": "老十八椒・麻辣香锅", "标签": ["川菜", "热菜", "火锅店"]},
{"店铺": "小正水饺", "标签": ["小吃店", "小吃", "主食"]},
{"店铺": "黄饷・咖喱蛋包饭", "标签": ["日式", "主食", "热菜", "快餐店"]},
{"店铺": "煎百味・煎饼卤粉", "标签": ["小吃店", "小吃", "主食"]},
{"店铺": "蜜哆哆炸鸡", "标签": ["小吃店", "小吃"]},
{"店铺": "匠心卤热卤拌 饭理工云创城", "标签": ["热菜", "主食", "快餐店"]},
{"店铺": "特色锅仔", "标签": ["热菜", "主食", "火锅店"]},
{"店铺": "有家黄焖鸡米饭", "标签": ["鲁菜", "热菜", "主食", "快餐店"]},
{"店铺": "卫宫家・咖喱饭", "标签": ["日式", "主食", "热菜", "快餐店"]},
{"店铺": "秦记潼关肉夹馍・酸辣粉", "标签": ["西北菜", "小吃店", "小吃", "主食"]},
{"店铺": "久久正宗固始鹅块", "标签": ["豫菜", "热菜", "主食", "汤类"]},
{"店铺": "LOOKING 韩式炸鸡", "标签": ["韩式", "小吃店", "小吃"]},
{"店铺": "丁记炒饭", "标签": ["主食", "热菜", "快餐店"]},
{"店铺": "珞珈酱烧黑鸭排骨饭", "标签": ["鄂菜", "主食", "热菜", "快餐店"]},
{"店铺": "土家酱香饼", "标签": ["鄂菜", "小吃店", "小吃", "主食"]},
{"店铺": "羊额烧鹅", "标签": ["粤菜", "主食", "热菜", "凉菜"]},
{"店铺": "清真・伊兰斋烤羊", "标签": ["西北菜", "烧烤店", "热菜", "主食"]},
{"店铺": "牛小匠・芝士牛肉抱蛋饭", "标签": ["主食", "热菜", "快餐店"]},
{"店铺": "珞珈轻食坊", "标签": ["轻食", "主食", "热菜"]},
{"店铺": "石记煎饼王", "标签": ["小吃店", "小吃", "主食"]},
{"店铺": "铁锅大鹅炖排骨嘎嘎香", "标签": ["东北菜", "热菜", "主食", "火锅店"]},
{"店铺": "研卤堂・卤味拌饭", "标签": ["热菜", "主食", "快餐店"]},
{"店铺": "鲜滑虾滑肥牛饭", "标签": ["热菜", "主食", "快餐店"]},
{"店铺": "小食坊・粉丝煲", "标签": ["小吃店", "小吃", "主食"]},
{"店铺": "珞家冒菜", "标签": ["川菜", "热菜", "主食", "火锅店"]},
{"店铺": "疆味新疆炒米", "标签": ["西北菜", "主食", "热菜"]},
{"店铺": "好好饭团・鸡蛋仔", "标签": ["小吃店", "小吃", "甜点"]},
{"店铺": "九筒冒菜", "标签": ["川菜", "热菜", "主食", "火锅店"]},
{"店铺": "地道北京烤鸭", "标签": ["京菜", "主食", "热菜", "凉菜"]},
{"店铺": "煲仔范・明火煲仔饭", "标签": ["粤菜", "主食", "热菜", "快餐店"]},
{"店铺": "舒記京味卤肉卷", "标签": ["京菜", "小吃店", "小吃", "主食"]},
{"店铺": "哈尔滨饺子馆", "标签": ["东北菜", "主食", "凉菜"]},
{"店铺": "味博士煲仔饭", "标签": ["粤菜", "主食", "热菜", "快餐店"]},
{"店铺": "食一锅・肥牛饭", "标签": ["主食", "热菜", "快餐店"]},
{"店铺": "食总管・现炒快餐", "标签": ["热菜", "主食", "快餐店"]},
{"店铺": "首 尔欧巴炸鸡", "标签": ["韩式", "小吃店", "小吃"]},
{"店铺": "港吃港喝・铁板饭米线武大店", "标签": ["粤菜", "主食", "热菜", "快餐店"]},
{"店铺": "武大擂椒木桶饭", "标签": ["湘菜", "主食", "热菜", "快餐店"]},
{"店铺": "恩施炕小土豆", "标签": ["鄂菜", "小吃店", "小吃"]},
{"店铺": "狮尚帝・柳州螺蛳粉", "标签": ["广西菜", "小吃店", "小吃", "主食"]},
{"店铺": "大学生都爱吃的格子饭", "标签": ["主食", "热菜", "快餐店"]},
{"店铺": "熊猫盖码饭", "标签": ["湘菜", "主食", "热菜", "快餐店"]},
{"店铺": "港式滑蛋饭猪脚饭", "标签": ["粤菜", "主食", "热菜", "快餐店"]},
{"店铺": "品好味鸭血粉丝汤", "标签": ["江浙菜", "小吃店", "小吃", "汤类", "主食"]},
{"店铺": "秦晋油泼面", "标签": ["西北菜", "主食", "热菜"]},
{"店铺": "马氏现卤鸭脖", "标签": ["小吃店", "小吃", "凉菜"]},
{"店铺": "安格斯牛肉拌饭", "标签": ["主食", "热菜", "快餐店"]},
{"店铺": "珞珈果果", "标签": ["饮品店", "小吃", "饮品"]},
{"店铺": "杨姐烤肉脆皮鸡饭", "标签": ["烧烤店", "主食", "热菜", "快餐店"]},
{"店铺": "正新鸡排", "标签": ["小吃店", "小吃"]},
{"店铺": "丰禄泰・襄阳牛肉面", "标签": ["鄂菜", "小吃店", "小吃", "主食"]},
{"店铺": "爆炒哥・私厨爆炒", "标签": ["川菜 / 湘菜", "热菜", "主食"]},
{"店铺": "灿灿餐厅", "标签": ["家常菜", "热菜", "主食"]},
{"店铺": "小肆川", "标签": ["川菜", "热菜", "主食"]},
{"店铺": "食畈广式烧腊", "标签": ["粤菜", "主食", "热菜", "快餐店"]},
{"店铺": "武大家常菜", "标签": ["鄂菜", "热菜", "主食"]},
{"店铺": "状元桥过桥米线", "标签": ["滇菜", "小吃店", "小吃", "主食", "汤类"]},
{"店铺": "徐记养生粥", "标签": ["粥类", "主食", "汤类", "快餐店"]},
{"店铺": "桃源章鱼小丸子", "标签": ["小吃店", "小吃"]},
{"店铺": "隆江猪脚饭", "标签": ["粤菜", "主食", "热菜", "快餐店"]},
{"店铺": "古茗", "标签": ["饮品店", "饮品"]},
{"店铺": "火遍全网的渣渣牛肉饭", "标签": ["川菜", "主食", "热菜", "快餐店"]},
{"店铺": "氢简厨房・暖轻食超级碗", "标签": ["轻食", "主食", "热菜"]},
{"店铺": "重庆麻辣烫", "标签": ["川菜", "小吃店", "小吃", "火锅店"]},
{"店铺": "桂林米粉", "标签": ["广西菜", "小吃店", "小吃", "主食"]},
{"店铺": "重庆小面", "标签": ["川菜", "小吃店", "小吃", "主食"]},
{"店铺": "川 悦情麻辣香锅", "标签": ["川菜", "热菜", "火锅店"]},
{"店铺": "蜜雪冰城", "标签": ["饮品店", "饮品"]},
{"店铺": "宝岛小厨", "标签": ["台式", "热菜", "主食"]},
{"店铺": "土耳其烤肉拌饭", "标签": ["西餐", "主食", "热菜", "快餐店"]},
{"店铺": "大志小厨・烧烤", "标签": ["烧烤店", "热菜", "主食"]},
{"店铺": "焦本味铁板焦饭", "标签": ["主食", "热菜", "快餐店"]},
{"店铺": "糯米包油条", "标签": ["小吃店", "小吃", "鄂菜"]},
{"店铺": "吞口熊牛排手作意面", "标签": ["西餐厅", "主食", "快餐店"]},
{"店铺": "中国兰州拉面", "标签": ["西北菜", "小吃店", "小吃", "主食"]},
{"店铺": "爱的锅三汁焖锅", "标签": ["热菜", "主食", "火锅店"]},
{"店铺": "香饱饱麻辣 香锅", "标签": ["川菜", "热菜", "火锅店"]},
{"店铺": "曹氏鸭脖", "标签": ["小吃店", "小吃", "凉菜"]},
{"店铺": "香香炸串", "标签": ["小吃店", "小吃", "烧烤店"]},
{"店铺": "沙县小吃", "标签": ["闽菜", "小吃店", "小吃", "主食"]},
{"店铺": "小观园快餐厅", "标签": ["快餐店", "热菜", "主食"]},
{"店铺": "精炙烧烤", "标签": ["烧烤店", "热菜", "主食"]},
{"店铺": "好味道粉面馆", "标签": ["小吃店", "小吃", "主食"]},
{"店铺": "袁记云饺", "标签": ["粤式", "小吃店", "小吃", "主食"]},
{"店铺": "诸小鲜猪肘面・杀猪粉", "标签": ["小吃店", "小吃", "主食", "汤类"]},
{"店铺": "椒大喜铜炉火锅鸡", "标签": ["川菜", "热菜", "火锅店"]},
{"店铺": "重庆鸭煲王", "标签": ["川菜", "热菜", "主食", "火锅店"]},
{"店铺": "安代表・新韩式炸鸡", "标签": ["韩式", "小吃店", "小吃"]},
{"店铺": "小伙计", "标签": ["未明确，需更多信息"]},
{"店铺": "特色小土豆鸡排饭", "标签": ["小吃店", "小吃", "主食", "热菜"]},
{"店铺": "呷哺呷哺火锅", "标签": ["火锅店", "热菜", "主食"]},
{"店铺": "不颠哥港式甜品店", "标签": ["甜品店", "甜点", "饮品店", "饮品"]},
{"店铺": "广东海鲜砂锅", "标签": ["粤菜", "热菜", "主食", "汤类"]},
{"店铺": "顶屋咖喱", "标签": ["日式", "主食", "热菜", "快餐店"]},
{"店铺": "炸鸡汉堡", "标签": ["小吃店", "小吃", "西餐厅", "主食", "快餐店"]},
    {"店铺": "蔡氏传承·中式快餐", "标签": ["快餐店", "主食", "热菜", "汤类"]},
    {"店铺": "氧气层", "标签": ["饮品店", "饮品"]},
    {"店铺": "重庆鸡公煲", "标签": ["川菜", "热菜", "主食"]},
    {"店铺": "百可基炸鸡汉堡", "标签": ["快餐店", "小吃", "主食"]},
    {"店铺": "一周吃七次中华肉酱面天花板", "标签": ["主食", "小吃店", "热菜"]},
    {"店铺": "夸父炸串", "标签": ["小吃店", "小吃", "烧烤店"]},
    {"店铺": "川味麻辣香锅·万州烤鱼", "标签": ["川菜", "热菜", "主食"]},
    {"店铺": "张菜菜麻辣烫麻辣拌", "标签": ["小吃店", "小吃", "热菜"]},
    {"店铺": "美其客汉堡", "标签": ["快餐店", "小吃", "主食"]},
    {"店铺": "刘长河手工粉", "标签": ["小吃店", "主食", "汤类"]},
    {"店铺": "卤鼎记热卤拌饭", "标签": ["快餐店", "主食", "热菜", "凉菜"]},
    {"店铺": "lucky top韩式炸鸡", "标签": ["快餐店", "小吃", "主食"]},
    {"店铺": "派乐汉堡·炸鸡", "标签": ["快餐店", "小吃", "主食"]},
    {"店铺": "瓦香鸡米饭", "标签": ["快餐店", "主食", "热菜"]},
    {"店铺": "舌尖大师铁板锅", "标签": ["小吃店", "主食", "热菜"]},
    {"店铺": "川北郎鸡公煲砂锅菜", "标签": ["川菜", "热菜", "主食"]},
    {"店铺": "苏叽苏叽跷脚牛肉", "标签": ["川菜", "热菜", "主食", "汤类"]},
    {"店铺": "辣嘴妹老成都冒烤", "标签": ["川菜", "小吃店", "小吃", "热菜"]},
    {"店铺": "胖哥饺子馆", "标签": ["东北菜", "主食", "小吃店", "凉菜"]},
    {"店铺": "山姆上校芝士牛肉卷·意面", "标签": ["西餐厅", "主食", "小吃"]},
    {"店铺": "秋果果·浇汁土豆泥拌饭", "标签": ["快餐店", "主食", "热菜"]},
    {"店铺": "失控猫·元気烧肉饭", "标签": ["快餐店", "主食", "热菜"]},
    {"店铺": "筷客烧烤炒菜", "标签": ["烧烤店", "热菜", "主食"]},
    {"店铺": "饱立", "标签": ["快餐店", "主食"]},
    {"店铺": "鱼籽村秘制拌饭", "标签": ["快餐店", "主食", "热菜"]},
    {"店铺": "江南糕点", "标签": ["甜点", "小吃店"]},
    {"店铺": "广东烧鹅", "标签": ["粤菜", "热菜", "主食"]},
    {"店铺": "黔味牛羊肉粉", "标签": ["小吃店", "主食", "汤类"]},
    {"店铺": "陕老顺油泼面·肉夹馍", "标签": ["西北菜", "主食", "小吃店"]},
    {"店铺": "大浪包子·咖啡", "标签": ["小吃店", "主食", "饮品店", "饮品", "甜点"]},
    {"店铺": "周麻婆·川菜现炒大王", "标签": ["川菜", "热菜", "主食"]},
    {"店铺": "胖大富蒜香排骨", "标签": ["小吃店", "小吃", "热菜"]},
    {"店铺": "杨 氏干拌烤鸭·烤鸡", "标签": ["热菜", "主食", "小吃店"]},
    {"店铺": "广东肠粉", "标签": ["粤菜", "主食", "小吃店", "汤类"]},
    {"店铺": "28CAKE面包蛋糕", "标签": ["甜点", "小吃店"]},
    {"店铺": "王春春鸡汤饭", "标签": ["小吃店", "主食", "汤类", "热菜"]},
    {"店铺": "安肉米·芝士和牛浇汁饭", "标签": ["快餐店", "主食", "热菜"]},
    {"店铺": "大米先生", "标签": ["快餐店", "主食", "热菜"]},
    {"店铺": "蛋挞家的烫", "标签": ["小吃店", "小吃", "热菜"]},
    {"店铺": "鱿鱼埠绝", "标签": ["小吃店", "小吃", "热菜"]},
    {"店铺": "寻味·新疆炒米粉", "标签": ["小吃店", "主食", "热菜"]},
    {"店铺": "辛香盖码饭", "标签": ["湘菜", "快餐店", "主食", "热菜"]},
    {"店铺": "重庆肥肠牛肉 小面", "标签": ["川菜", "主食", "小吃店", "汤类"]},
    {"店铺": "超牛堡·炙烤牛肉汉堡", "标签": ["快餐店", "主食", "小吃"]},
    {"店铺": "正新鸡排·炸鸡烧烤", "标签": ["小吃店", "小吃", "烧烤店", "热菜"]},
    {"店铺": "想来份水果捞", "标签": ["饮品店", "饮品", "甜点", "小吃店"]},
    {"店铺": "心飨の咖喱鸡番茄牛腩饭", "标签": ["快餐店", "主食", "热菜"]},
    {"店铺": "大口章鱼烧", "标签": ["小吃店", "小吃"]},
    {"店铺": "塔斯汀·中国汉堡", "标签": ["快餐店", "主食", "小吃"]},
    {"店铺": "茳葫荇·荆州铁板烧", "标签": ["鄂菜", "小吃店", "主食", "热菜"]},
    {"店铺": "韩食记·嫩豆腐汤·石锅拌饭", "标签": ["主食", "汤类", "热菜", "小吃店"]}
]

print(len(shop_tags))

data=[]
for doc in collection.find():
    # print(doc['name'])
    # data.append(doc['name'])
    data.append(doc['priceEle'])
# print(data)



# # 2. 读取所有文档并添加标签
for i,doc in enumerate(collection.find()):  # 遍历每一条记录
    # 更新当前文档，添加 `tag` 字段
    tags=shop_tags[i].get('tags',[])+shop_tags[i].get('标签',[])
    # print(tags)
    category=tags
    # print(category)
    collection.update_one(
        {"_id": doc["_id"]},  # 用 _id 匹配当前文档
        {"$set": {"catogery": category}},
    )

# 3. 关闭连接
client.close()