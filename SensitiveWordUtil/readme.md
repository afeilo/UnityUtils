#### C#匹配敏感词
采用牺牲空间换取时间的策略，与正则表达式匹配根本不在一个数量级，经测试6000+敏感词，生成树需要30ms左右时间，正常匹配时间只有1-5ms，而正则表达式超过1000几乎卡死。
敏感词匹配支持空格匹配，大小写匹配，详情看demo。
Util.cs:工具类，直接生成对应关键字数组。
[参考](http://blog.csdn.net/chenssy/article/details/26961957)