from pymongo import MongoClient
import pandas as pd
import sys
import json

def import_data_to_mongodb(ele_path, meituan_path, mongo_uri='mongodb://localhost:27017/'):
    """
    将Excel数据导入MongoDB
    
    Args:
        ele_path: 饿了么数据Excel文件路径
        meituan_path: 美团数据Excel文件路径
        mongo_uri: MongoDB连接字符串
    
    Returns:
        导入的商家数量
    """
    # 1. 连接数据库
    client = MongoClient(mongo_uri)
    db = client['food_recommendation_db']
    merchants_collection = db['merchants']
    
    try:
        # 读取Excel数据
        ele_data = pd.read_excel(ele_path)
        meituan_data = pd.read_excel(meituan_path)
        
        data = {}
        
        # 处理饿了么数据
        for idx, row in ele_data.iterrows():
            name = row['名称']
            i = name.find('(')
            if i != -1:
                name = name[0:i]
            if data.get(name, 'none') == 'none':
                data[name] = {
                    "name": row['名称'],
                    "address": row['地址'],
                    "businessHours": row['营业时间'],
                    "ratingEle": row['评分'],
                    'ratingMeituan': '5.0',
                    "image": row['图片'],
                    "priceEle": row['人均花费'],
                    "priceMeituan": '4',
                    "distance": row['距离'],
                    "recentOrderNum": row['近期售出'],
                    "lowestCost": row['起送价格'],
                    "deliveryFee": row['配送费'],
                    "deliveryMode": row['配送方'],
                    "orderLeadTime": row['配送时间'],
                    "reasons": []
                }
                reasons = row['标签'].split(',')
                data[name]['reasons'] = reasons[:-1]
            else:
                print("?")
                
        # 处理美团数据
        for idx, row in meituan_data.iterrows():
            name = row['名称']
            i = name.find('（')
            if i != -1:
                name = name[0:i]
            if data.get(name, 'none') == 'none':
                continue
            data[name]['ratingMeituan'] = row['评分']
            data[name]['priceMeituan'] = row['人均花费']
            data[name]['deliveryFee'] = row['配送费']
            reasons = row['标签'].split(',')
            data[name]['reasons'] += reasons[:-1]
        
        # 移除无效记录
        dataItems = list(data.items()).copy()
        for item in dataItems:
            merchant = item[1]
            if merchant['deliveryFee'] == '?':
                data.pop(item[0])
        
        # 插入数据
        count = 0
        for item in data.items():
            # 检查是否存在相同名称的商家
            existing = merchants_collection.find_one({"name": item[1]["name"]})
            if existing:
                # 更新现有商家
                merchants_collection.update_one(
                    {"name": item[1]["name"]},
                    {"$set": item[1]}
                )
                print(f"更新商家: {item[1]['name']}")
            else:
                # 插入新商家
                result = merchants_collection.insert_one(item[1])
                count += 1
                print(f"插入商家: {item[1]['name']}, ID: {result.inserted_id}")
            
        return count
    finally:
        # 关闭连接
        client.close()
        
# 如果脚本直接运行而不是被导入
if __name__ == "__main__":
    if len(sys.argv) > 2:
        ele_path = sys.argv[1]
        meituan_path = sys.argv[2]
        mongo_uri = sys.argv[3] if len(sys.argv) > 3 else 'mongodb://localhost:27017/'
        try:
            result = import_data_to_mongodb(ele_path, meituan_path, mongo_uri)
            print(json.dumps({"success": True, "count": result}))
        except Exception as e:
            print(json.dumps({"success": False, "error": str(e)}))
    else:
        print(json.dumps({"success": False, "error": "Missing required arguments"}))