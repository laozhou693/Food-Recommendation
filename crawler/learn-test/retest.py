import re

a=re.compile(r'a.*b')
b=re.findall(a,"a\nb")
print(b)