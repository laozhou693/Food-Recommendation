from pymongo import MongoClient
import pandas as pd

# 1. 连接数据库
client = MongoClient('mongodb://localhost:27017/')
db = client['food_recommendation_db']  # 创建/连接到school_db数据库

# 2. 创建集合
merchants_collection = db['merchants']  # 创建students集合

# 3. 插入单个文档
# student1 = {
#     "name": "张三",
#     "age": 20,
#     "major": "计算机科学",
#     "gpa": 3.5,
#     "courses": ["数据库", "算法", "网络"]
# }
# result = students_collection.insert_one(student1)
# print(f"插入第一个学生，ID: {result.inserted_id}")

ele_data=pd.read_excel('./ele-data.xls')
meituan_data=pd.read_excel('./meituan-data.xls')

data={}
test_data={
    "name":'w',
    "address":"xxx",
    "businessHours":'a:b~c:d',
    "ratingEle":'5',
    'ratingMeituan':'5.0',
    "image":'w://w',
    "priceEle":"5",
    "priceMeituan":'4',
    "distance":"1",
    "recentOrderNum":'5',
    "lowestCost":'4',
    "deliveryFee":'3',
    "deliveryMode":'awa',
    "orderLeadTime":'1.2',
    "reasons":[]
}

# print(ele_data)

for idx,row in ele_data.iterrows():
    name=row['名称']
    i=name.find('(')
    name=name[0:i]
    if data.get(name,'none')=='none':
        data[name]={
            "name":row['名称'],
            "address":row['地址'],
            "businessHours":row['营业时间'],
            "ratingEle":row['评分'],
            'ratingMeituan':'5.0',
            "image":row['图片'],
            "priceEle":row['人均花费'],
            "priceMeituan":'4',
            "distance":row['距离'],
            "recentOrderNum":row['近期售出'],
            "lowestCost":row['起送价格'],
            "deliveryFee":row['配送费'],
            "deliveryMode":row['配送方'],
            "orderLeadTime":row['配送时间'],
            "reasons":[]
        }
        reasons=row['标签'].split(',')
        data[name]['reasons']=reasons[:-1]
    else:
        print("?")
        
for idx,row in meituan_data.iterrows():
    name=row['名称']
    i=name.find('（')
    name=name[0:i]
    if data.get(name,'none')=='none':
        continue
    data[name]['ratingMeituan']=row['评分']
    data[name]['priceMeituan']=row['人均花费']
    data[name]['deliveryFee']=row['配送费']
    reasons=row['标签'].split(',')
    data[name]['reasons']+=reasons[:-1]

dataItems=list(data.items()).copy()
for item in dataItems:
    merchant=item[1]
    if merchant['deliveryFee']=='?':
        data.pop(item[0])
        
# print(len(data))
# print(data)

for item in data.items():
    result=merchants_collection.insert_one(item[1])
    print(f"插入一个商家，ID: {result.inserted_id}")

# 5. 关闭连接
client.close()