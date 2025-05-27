from pymongo import MongoClient
import pandas as pd
import random

# 1. 连接数据库
client = MongoClient('mongodb://localhost:27017/')
db = client['food_recommendation_db']  # 创建/连接到school_db数据库

# 2. 创建集合
merchants_collection = db['merchants']  # 创建students集合


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
    "reasons":[],
    "catogety":[]
}

# print(ele_data)

def createRating():
    a=random.randrange(43,47)
    b=1.0*a/10
    return b
def createPrice():
    a=random.randrange(14,20)
    return f'人均 ￥{a}'
# def checkPrice(s):
#     print(s)
#     return '人均' in s

for idx,row in ele_data.iterrows():
    name=row['名称']
    i=name.find('(')
    if name!=-1:
        name=name[0:i]
    # if type(row['人均花费'])=='float':
    #     ele_data.loc[idx,'人均花费']=createPrice()
    if data.get(name,'none')=='none':
        data[name]={
            "name":name,
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
        # print(row['标签'])
        reasons=row['标签'].split(',')
        data[name]['reasons']=reasons[:-1]
        if pd.isna(data[name]['priceEle']):
            data[name]['priceEle']=createPrice()
    else:
        print("?")
print(len(data))        

for idx,row in meituan_data.iterrows():
    name=row['名称']
    i=name.find('（')
    if name!=-1:
        name=name[0:i]
    if data.get(name,'none')=='none':
        continue
    data[name]['image']=row['图片']
    data[name]['ratingMeituan']=row['评分']
    data[name]['priceMeituan']=row['人均花费']
    data[name]['deliveryFee']=row['配送费']

print(len(data))

for key,value in data.items():
    if value['ratingMeituan']=='5.0':
        data[key]['ratingMeituan']=createRating()
    if value['priceMeituan']=='4':
        data[key]['priceMeituan']=createPrice()
dataItems=list(data.items()).copy()

print(len(data))

for item in dataItems:
    result=merchants_collection.insert_one(item[1])
    print(f"插入一个商家，ID: {result.inserted_id}")

# 5. 关闭连接
client.close()