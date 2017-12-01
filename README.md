# UnityUtils
收集一些Unity中常用的工具类
- obb

obb校验类，详情可查看[blog](http://www.jianshu.com/p/af3f8e8f2a96)

- splash

android冷启动黑屏问题

- permission

Unity使用android权限相关的类，Unity原生的权限显示会在>6.0版本activity recreate的时候crash。

- screenshot

Unity截屏分享

- EmojiText

Unity图文混排，[参考](https://blog.uwa4d.com/archives/Sparkle_UGUI.html)修改了一下替换占位符的算法，mark一下

- uGUI-Hypertext

Unity 超链接，[参考](https://github.com/setchi/uGUI-Hypertext)

- SensitiveWordUtil

C#匹配敏感词

采用牺牲空间换取时间的策略，与正则表达式匹配根本不在一个数量级，经测试6000+敏感词，生成树需要30ms左右时间，正常匹配时间只有1-5ms，而正则表达式超过1000几乎卡死。 敏感词匹配支持空格匹配，大小写匹配，详情看demo。 Util.cs:工具类，直接生成对应关键字数组。 [参考](http://blog.csdn.net/chenssy/article/details/26961957)

- ExcelToText

excel转化成文本，用来转化android ios原生多语言，需要安装python以及[xlrd](https://pypi.python.org/pypi/xlrd)，使用方法：
```
python excelToAndroidXml.py 文件名1 文件名2 文件名3
```