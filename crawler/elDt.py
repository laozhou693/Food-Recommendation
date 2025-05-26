import json


with open('./eleData.json', 'r',encoding='utf-8') as d:
    data=json.load(d)
    items=data['data']['resultMap']['menu']['itemGroups']
    for item in items:
        name=item['items'][1]['name']
        print(name)