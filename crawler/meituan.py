from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.options import Options
from selenium.common.exceptions import WebDriverException
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import time  
import json
import base64
import xlwt

options = Options()


mobileEmulation = {'deviceName': 'iPhone 6'}
options.add_experimental_option('mobileEmulation', mobileEmulation)

options.add_argument("--disable-blink-features=AutomationControlled")  # 禁用自动化控制标志
options.add_experimental_option("excludeSwitches", ["enable-automation"])  # 隐藏"Chrome正受到自动测试软件控制"
options.add_experimental_option("useAutomationExtension", False)  # 禁用自动化扩展

# options.add_argument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")

# 1. 设置 iPhone 6 的 User-Agent
iphone6_user_agent = (
    "Mozilla/5.0 (iPhone; CPU iPhone OS 12_0 like Mac OS X) "
    "AppleWebKit/605.1.15 (KHTML, like Gecko) "
    "Version/12.0 Mobile/15E148 Safari/604.1"
)
options.add_argument(f"user-agent={iphone6_user_agent}")

# 2. 设置视口大小（iPhone 6 分辨率：375x667）
options.add_argument("--window-size=375,667")

caps = {
    "browserName": "chrome",
    'goog:loggingPrefs': {'performance': 'ALL'}  # 开启日志性能监听
}

# 将caps添加到options中
for key, value in caps.items():
    options.set_capability(key, value)

options.add_argument("--user-data-dir=C:/Users/lenovo/AppData/Local/Google/Chrome/test-User-Data2")  # 替换为你的路径
options.add_argument("--profile-directory=Default")  # 默认配置文件，如有自定义则改为对应名称

driver = webdriver.Chrome(options=options)

driver.execute_script("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})")

driver.get('https://h5.waimai.meituan.com/waimai/mindex/home')  

driver.switch_to.default_content()

input("请手动完成验证码后按回车继续...")

def filter_type(_type: str):
    types = [
        'application/javascript', 'application/x-javascript', 'text/css', 'webp', 'image/png', 'image/gif',
        'image/jpeg', 'image/x-icon', 'application/octet-stream','text/plain','image/webp','text/html'
    ]
    if _type not in types:
        return True
    return False

savepath=f'拿下美团-{time.time()}.xls'
def process_and_save():
    dataCount=0
    requestMap={}
    print(f'save.......')
    book = xlwt.Workbook(encoding="utf-8",style_compression=0) #创建workbook对象
    sheet = book.add_sheet(f'拿下美团', cell_overwrite_ok=True) #创建工作表
    performance_log = driver.get_log('performance')  # 获取名称为 performance 的日志

    col = ('名称','图片','地址','营业时间','人均花费','评分','近期售出','起送价格','配送费','配送方','配送时间','距离','标签','','品牌')
    for i in range(0,14):
        sheet.write(0,i,col[i])  #列名

    for packet in performance_log:
        message = json.loads(packet.get('message')).get('message')  # 获取message的数据
        if message.get('method') == 'Network.requestWillBeSent': 
            request_data = message.get("params", {}).get("request", {})
            request_method = request_data.get("method")
            request_url = request_data.get("url")       # 获取请求 URL
            request_id = message.get("params", {}).get("requestId")
            # if request_method=='POST':
            #     print(request_method,request_url)
            if request_method=='POST' and ('shopList' in request_url):
                requestMap[str(request_id)]=1
                print(requestMap)
        if message.get('method') != 'Network.responseReceived':        
            continue
        packet_type = message.get('params').get('response').get('mimeType')  # 获取该请求返回的type
        if(packet_type!='application/json'):
            continue
        requestId = message.get('params').get('requestId')  # 唯一的请求标识符。相当于该请求的身份证
        if requestMap.get(requestId,0)==0:
            continue
        url = message.get('params').get('response').get('url')  # 获取 该请求  url
        try:
            resp = driver.execute_cdp_cmd('Network.getResponseBody', {'requestId': requestId})  # selenium调用 cdp
            body = base64.b64decode(resp["body"]).decode('utf-8') if resp["base64Encoded"] else resp["body"]
            # 解析JSON
            try:
                data = json.loads(body)  # 转换为Python字典
                items=data['module_list'][0]['module_list']
                for item in items:
                    item=item['string_data']
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
                    reasons=item["brightspot_tags"]
                    reasonsStr=''
                    for r in reasons:
                        reasonsStr+=f'{r['text']},'
                    # brandName=item['fields']['restaurant'].get('brandName','')
                    brandName='?'
                    props=[name,img_path,address,openingHours,averagePrice,rating,recentOrderNum,lowestCost,deliveryFee,deliveryMode,orderLeadTime,distance,reasonsStr,brandName]
                    if(name==''):
                        Exception
                    dataCount+=1
                    for j in range(0,14):
                        sheet.write(dataCount,j,props[j])  #数据
                    book.save(savepath) #保存
            except Exception as e:
                # print(e)nn
                print("响应不是有效的JSON格式 或者 不存在\n")
        except WebDriverException:  # 忽略异常
            pass

# for i in range(1000):
#     driver.execute_script('document.getElementsByClassName("mor-comp-page-content")[0].scrollBy(0,1000)')
#     time.sleep(0.5)
#     if(i%100==10):
#         print(i)
#         # time.sleep(1)
#         # process_and_save()
process_and_save()

input("请手动完成验证码后按回车继续...")



# with open("test.txt","w",encoding='utf-8') as f:
#     with open("json.txt","w",encoding='utf-8') as t:
#         with open("data.txt","w",encoding='utf-8') as d:
#             performance_log = driver.get_log('performance')  # 获取名称为 performance 的日志
#             for packet in performance_log:
#                 message = json.loads(packet.get('message')).get('message')  # 获取message的数据
#                 if message.get('method') != 'Network.responseReceived':  # 如果method 不是 responseReceived 类型就不往下执行
#                     continue
#                 packet_type = message.get('params').get('response').get('mimeType')  # 获取该请求返回的type
#                 # if not filter_type(_type=packet_type):  # 过滤type
#                 #     continue
#                 if(packet_type!='application/json'):
#                     continue
#                 requestId = message.get('params').get('requestId')  # 唯一的请求标识符。相当于该请求的身份证
#                 url = message.get('params').get('response').get('url')  # 获取 该请求  url
#                 try:
#                     resp = driver.execute_cdp_cmd('Network.getResponseBody', {'requestId': requestId})  # selenium调用 cdp
#                     # f.write(f'type: {packet_type} url: {url}\n')
#                     # f.write(f'response: {resp}\n')
#                     body = base64.b64decode(resp["body"]).decode('utf-8') if resp["base64Encoded"] else resp["body"]
#                     t.write(body)
#                     t.write('\n\n')
#                     # 解析JSON
#                     try:
#                         data = json.loads(body)  # 转换为Python字典
#                         # print(data)
#                         # f.write('\n\n')
#                         if(data.get('api')!='mtop.alsc.eleme.miniapp.homepagev1' and data.get('api')!='mtop.alsc.eleme.miniapp.collection.homepagev1'): 
#                             continue
#                         items=data['data']['data']["frontend_page_shop_list_recommend"]['fields']['items']
#                         for item in items:
#                             dataCount+=1
#                             img_path=item['fields']['restaurant']['imagePath']
#                             d.write(f'img_path:{img_path}\n')
#                             name=item['fields']['restaurant']['name']
#                             d.write(f'name:{name}\n')
#                             # item['fields']['restaurant']['fieldMap']
#                             address=item['fields']['restaurant']['fieldMap']['shopBasicInfo']['address']
#                             d.write(f'address:{address}\n')
#                             openingHours=item['fields']['restaurant']['openingHours']
#                             d.write(f'openingHours:{openingHours}\n')
#                             averagePrice=item['fields']['restaurant'].get('averagePrice','')
#                             d.write(f'averagePrice:{averagePrice}\n')
#                             rating=item['fields']['restaurant']['rating']
#                             d.write(f'rating:{rating}\n')
#                             recentOrderNum=item['fields']['restaurant']['recentOrderNumDesc']
#                             d.write(f'recentOrderNum:{recentOrderNum}\n')
#                             lowestCost=item['fields']['restaurant']['piecewiseAgentFee']['rules'][0]['price']
#                             d.write(f'lowestCost:{lowestCost}\n')
#                             deliveryFee=item['fields']['restaurant']['piecewiseAgentFee']['rules'][0]['fee']
#                             d.write(f'deliveryFee:{deliveryFee}\n')
#                             deliveryMode=item['fields']['restaurant']['deliveryMode']['text']
#                             d.write(f'deliveryMode:{deliveryMode}\n')
#                             orderLeadTime=item['fields']['restaurant']['orderLeadTime']
#                             d.write(f'orderLeadTime:{orderLeadTime}\n')
#                             distance=item['fields']['restaurant']['distance']
#                             d.write(f'distance:{distance}\n')
#                             reasons=item['fields']['restaurant']['recommendReasons']
#                             reasonsStr=''
#                             for r in reasons:
#                                 reasonsStr+=f'{r['reason']},'
#                             d.write(f'recommendReasons:{reasonsStr}\n')
#                             brandName=item['fields']['restaurant']['brandName']
#                             d.write(f'brandName:{brandName}\n')
#                             d.write("\n\n")
                            
#                     except Exception as e:
#                         # print(e.message)
#                         print("响应不是有效的JSON格式 或者 不存在\n")
#                 except WebDriverException:  # 忽略异常
#                     pass
# print(f'总数：{dataCount}')
