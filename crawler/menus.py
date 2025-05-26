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
options.add_argument("--disable-blink-features=AutomationControlled")  # 禁用自动化控制标志
options.add_experimental_option("excludeSwitches", ["enable-automation"])  # 隐藏"Chrome正受到自动测试软件控制"
options.add_experimental_option("useAutomationExtension", False)  # 禁用自动化扩展

options.add_argument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")

caps = {
    "browserName": "chrome",
    'goog:loggingPrefs': {'performance': 'ALL'}  # 开启日志性能监听
}

# 将caps添加到options中
for key, value in caps.items():
    options.set_capability(key, value)

options.add_argument("--user-data-dir=C:/Users/lenovo/AppData/Local/Google/Chrome/test-User-Data")  # 替换为你的路径
options.add_argument("--profile-directory=Default")  # 默认配置文件，如有自定义则改为对应名称

driver = webdriver.Chrome(options=options)

driver.execute_script("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})")

driver.get('https://h5.ele.me/minisite/?spm=a2ogi.13147251.0.0&spm=a2ogi.13162730.zebra-ele-login-module-9089118186&spm=a2ogi.13162730.zebra-ele-login-module-9089118186&spm-pre=a2f6g.12507204.ebridge.login&spm=a2ogi.13162730.zebra-ele-login-module-9089118186&spm=a2ogi.13162730.zebra-ele-login-module-9089118186')  
# driver.get('https://baidu.com')

# time.sleep(10)
# driver.switch_to.frame('alibaba-login-box')
# print('switch')
# sel = WebDriverWait(driver, 10).until(
#     EC.presence_of_element_located((By.XPATH, "//*[@id=\"fm-agreement-checkbox\"]"))
# )
# print(sel)
# sel.click()
# btn = WebDriverWait(driver, 10).until(
#     EC.presence_of_element_located((By.XPATH, "//*[@id=\"login-form\"]/div[6]/button"))
# )
# print(btn)
# btn.click()

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

savepath=f'拿下饿了么-{time.time()}.xls'
def process_and_save():
    dataCount=0
    print(f'save.......')
    book = xlwt.Workbook(encoding="utf-8",style_compression=0) #创建workbook对象
    sheet = book.add_sheet(f'拿下饿了么', cell_overwrite_ok=True) #创建工作表
    performance_log = driver.get_log('performance')  # 获取名称为 performance 的日志

    col = ('名称','图片','地址','营业时间','人均花费','评分','近期售出','起送价格','配送费','配送方','配送时间','距离','标签','','品牌')
    for i in range(0,14):
        sheet.write(0,i,col[i])  #列名

    for packet in performance_log:
        message = json.loads(packet.get('message')).get('message')  # 获取message的数据
        if message.get('method') != 'Network.responseReceived':  # 如果method 不是 responseReceived 类型就不往下执行
            continue
        packet_type = message.get('params').get('response').get('mimeType')  # 获取该请求返回的type
        # if not filter_type(_type=packet_type):  # 过滤type
        #     continue
        if(packet_type!='application/json'):
            continue
        requestId = message.get('params').get('requestId')  # 唯一的请求标识符。相当于该请求的身份证
        url = message.get('params').get('response').get('url')  # 获取 该请求  url
        try:
            resp = driver.execute_cdp_cmd('Network.getResponseBody', {'requestId': requestId})  # selenium调用 cdp
            body = base64.b64decode(resp["body"]).decode('utf-8') if resp["base64Encoded"] else resp["body"]
            # 解析JSON
            try:
                data = json.loads(body)  # 转换为Python字典
                if(data.get('api')!='mtop.alsc.eleme.miniapp.homepagev1' and data.get('api')!='mtop.alsc.eleme.miniapp.collection.homepagev1'): 
                    continue
                items=data['data']['resultMap']['menu']['itemGroups']
                for item in items:
                    name=item['items'][1]['name']
                
                
                    book.save(savepath) #保存
            except Exception as e:
                # print(e)nn
                print("响应不是有效的JSON格式 或者 不存在\n")
        except WebDriverException:  # 忽略异常
            pass

for i in range(500):
    driver.execute_script('document.getElementsByClassName("mor-comp-page-content")[0].scrollBy(0,1000)')
    time.sleep(0.5)
    if(i%100==10):
        print(i)
        # time.sleep(1)
        # process_and_save()
process_and_save()

input("请手动完成验证码后按回车继续...")


