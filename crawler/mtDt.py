import json
import xlwt
import os

root='./meituan-data'
files=os.listdir(root)

savepath='meituanTest.xls'
book = xlwt.Workbook(encoding="utf-8",style_compression=0) #创建workbook对象
sheet = book.add_sheet(f'拿下美团', cell_overwrite_ok=True) #创建工作表

col = ('名称','图片','地址','营业时间','人均花费','评分','近期售出','起送价格','配送费','配送方','配送时间','距离','标签','','品牌')
for i in range(0,14):
    sheet.write(0,i,col[i])  #列名

dataCount=0

nameMap={}

for file in files:
    file_path=f'{root}/{file}'
    file_name_without_extension, file_extension = os.path.splitext(file)
    # if file_extension!='.json':
    #     continue
    with open(file_path, 'r',encoding='utf-8') as d:
        data=json.load(d)
        data=data['data']
        # data=data.replace('\\','')
        data=json.loads(data)
        items=data['module_list'][0]['module_list']
        for item in items:
            item=item['string_data']
            item=json.loads(item)
            # print(item)
            props=[]
            img_path=item['poi_pic']
            name=item['poi_name']
            # address=item['fields']['restaurant']['fieldMap']['shopBasicInfo'].get('address','')
            address='?'
            # openingHours=item['fields']['restaurant'].get('openingHours','')
            openingHours='?'
            averagePrice=item['avg_price_tip']
            rating=item["wm_poi_score"]
            recentOrderNum=item["month_sales_tip"]
            lowestCost=item["min_price_tip"]
            deliveryFee=item["shipping_fee_tip"]
            # deliveryMode=item['fields']['restaurant']['deliveryMode']['text']
            deliveryMode='?'
            orderLeadTime=item["delivery_time_tip"]
            distance=item['distance']
            reasons=item.get("brightspot_tags",[])
            reasonsStr=''
            for r in reasons:
                reasonsStr+=f'{r['text']},'
            # brandName=item['fields']['restaurant'].get('brandName','')
            brandName='?'
            props=[name,img_path,address,openingHours,averagePrice,rating,recentOrderNum,lowestCost,deliveryFee,deliveryMode,orderLeadTime,distance,reasonsStr,brandName]
            if(name==''):
                Exception
            if nameMap.get(name,0)==1:
                continue
            nameMap[name]=1
            dataCount+=1
            for j in range(0,14):
                sheet.write(dataCount,j,props[j])  #数据
            book.save(savepath) #保存
