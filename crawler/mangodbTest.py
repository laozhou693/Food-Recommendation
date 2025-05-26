from pymongo import MongoClient

# 1. 连接数据库
client = MongoClient('mongodb://localhost:27017/')
db = client['school_db']  # 创建/连接到school_db数据库

# 2. 创建集合
students_collection = db['students']  # 创建students集合

# 3. 插入单个文档
student1 = {
    "name": "张三",
    "age": 20,
    "major": "计算机科学",
    "gpa": 3.5,
    "courses": ["数据库", "算法", "网络"]
}
result = students_collection.insert_one(student1)
print(f"插入第一个学生，ID: {result.inserted_id}")

# 4. 插入多个文档
more_students = [
    {
        "name": "李四",
        "age": 22,
        "major": "数学",
        "gpa": 3.8,
        "courses": ["微积分", "线性代数", "概率论"]
    },
    {
        "name": "王五",
        "age": 21,
        "major": "物理",
        "gpa": 3.2,
        "courses": ["力学", "电磁学", "量子物理"]
    }
]
results = students_collection.insert_many(more_students)
print(f"插入多个学生，IDs: {results.inserted_ids}")

# 5. 关闭连接
client.close()