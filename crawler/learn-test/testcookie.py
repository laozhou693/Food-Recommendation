from selenium import webdriver
from selenium.webdriver.chrome.options import Options

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

options.add_argument("--user-data-dir=C:/Users/lenovo/AppData/Local/Google/Chrome/User Data")  # 替换为你的路径
options.add_argument("--profile-directory=Default")  # 默认配置文件，如有自定义则改为对应名称

driver = webdriver.Chrome(options=options)
driver.get("https://bilibili.com")  # 直接进入已登录状态

input("wait")