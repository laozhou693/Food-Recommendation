from selenium import webdriver
import time

driver=webdriver.Chrome()
driver.get('https://movie.douban.com/top250?start=1')
for i in range(5):
    if i%2==0:
        driver.execute_script('window.scrollBy(0, document.body.scrollHeight)')
    else:
        driver.execute_script('window.scrollBy(0, -document.body.scrollHeight)')
    time.sleep(0.5)