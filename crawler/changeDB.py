from pymongo import MongoClient

# 1. 连接 MongoDB
client = MongoClient("mongodb://localhost:27017/")
db = client["food_recommendation_db"]  # 替换为你的数据库名
collection = db["merchants"]  # 替换为你的集合名


# for doc in collection.find():
#     print(doc['name'])


# # 2. 读取所有文档并添加标签
for i,doc in enumerate(collection.find()):  # 遍历每一条记录
    category=[]
    for tag in tags:
        category+=tag
    # print(category)
    collection.update_one(
        {"_id": doc["_id"]},  # 用 _id 匹配当前文档
        {"$set": {"catogery": category}},
    )

# 3. 关闭连接
client.close()