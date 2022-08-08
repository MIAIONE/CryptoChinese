# CryptoChinese 

##  一个基于Base64的中文加密(混淆)项目

![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/MIAIONE/CryptoChinese?style=flat-square)![GitHub](https://img.shields.io/github/license/MIAIONE/CryptoChinese?style=flat-square)![GitHub language count](https://img.shields.io/github/languages/count/MIAIONE/CryptoChinese?style=flat-square)![GitHub all releases](https://img.shields.io/github/downloads/MIAIONE/CryptoChinese/total?style=flat-square)![GitHub last commit](https://img.shields.io/github/last-commit/MIAIONE/CryptoChinese?style=flat-square)![GitHub Release Date](https://img.shields.io/github/release-date/MIAIONE/CryptoChinese?style=flat-square)

------

### 编译环境:

1. Visual Studio 2022 或最新版本

2. .NET 6 SDK

------

### 命令参数:

! NOTE ! : 字符集用来统一保存, 打开, 加解密文档, 不知道字符集可能无法解密文件

##### 1.生成密钥: 

gen [string:密钥保存路径] [string:字典路径] [int:加密表大小 范围 2-64] [bool:是否开启GZIP压缩 true/false] [string:字符集(如utf-8, utf-32, gbk, gb2312 等)]

```powershell
CryptoChinese.exe gen key.json dic.txt 64 true utf-8
```

##### 2.加密文件:

 enc [string:密钥路径] [string:源文件路径] [string:加密后文件路径] [string:字符集]

```powershell
CryptoChinese.exe enc key.json data.zip data.encrypt utf-8
```

##### 3.解密文件:

dec [string:密钥路径] [string:加密后文件路径] [string:解密后文件路径] [string:字符集]

```powershell
CryptoChinese.exe enc key.json data.encrypt data.zip utf-8
```



Tips:

[!] 生成密钥为了保证通用性, 强烈建议使用utf-8字符集
[!] 生成密钥时注意参数范围和类型
[!] 开启GZIP可减小加密后大小, 建议开启
[!] 路径有空格需要放入英文双引号内



### 实际效果:

为了演示实际用途, 不太可能那这个传zip吧? 故用图片测试, 这里选择了老朋友哔咔作为原图测试: 

![pica](README.assets/pica.png)

先生成个Key:

```powershell
gen key.json dic.txt 64 true utf-8
```

Key JSON:

```json
{
  "CharsWordsTable": {
    "Q": "\u9603",
    "q": "\u6838",
    "W": "\u6C69",
    "w": "\u68A3",
    "E": "\u7629",
    "e": "\u5014",
    "R": "\u7A40",
    "r": "\u8FF8",
    "T": "\u5C3D",
    "t": "\u94F3",
    "Y": "\u74BA",
    "y": "\u592F",
    "U": "\u94A3",
    "u": "\u9881",
    "I": "\u803B",
    "i": "\u8475",
    "O": "\u70E6",
    "o": "\u5E9C",
    "P": "\u5783",
    "p": "\u8D56",
    "A": "\u77FE",
    "a": "\u90B8",
    "S": "\u575D",
    "s": "\u7727",
    "D": "\u58D5",
    "d": "\u7228",
    "F": "\u63B8",
    "f": "\u4EF7",
    "G": "\u9628",
    "g": "\u7236",
    "H": "\u857A",
    "h": "\u81CC",
    "J": "\u987F",
    "j": "\u77E9",
    "K": "\u8FC7",
    "k": "\u6249",
    "L": "\u895C",
    "l": "\u822B",
    "Z": "\u760A",
    "z": "\u6020",
    "X": "\u9E41",
    "x": "\u5764",
    "C": "\u798F",
    "c": "\u8F71",
    "V": "\u58F8",
    "v": "\u874C",
    "B": "\u4F7D",
    "b": "\u7B15",
    "N": "\u904D",
    "n": "\u5230",
    "M": "\u60E8",
    "m": "\u9CCC",
    "/": "\u953E",
    "\u002B": "\u9616",
    "=": "\u70E4",
    "0": "\u9648",
    "1": "\u64E6",
    "2": "\u8663",
    "3": "\u701A",
    "4": "\u957F",
    "5": "\u6DF3",
    "6": "\u8BA1",
    "7": "\u76F9",
    "8": "\u85C1",
    "9": "\u62F1"
  },
  "KeysTable": {
    "R": "Z",
    "Z": "U",
    "U": "h",
    "h": "Q",
    "Q": "c",
    "c": "k",
    "k": "w",
    "w": "G",
    "G": "i",
    "i": "\u002B",
    "\u002B": "r",
    "r": "S",
    "S": "l",
    "l": "t",
    "t": "B",
    "B": "a",
    "a": "5",
    "5": "Y",
    "Y": "F",
    "F": "n",
    "n": "g",
    "g": "e",
    "e": "s",
    "s": "X",
    "X": "2",
    "2": "8",
    "8": "T",
    "T": "3",
    "3": "D",
    "D": "4",
    "4": "/",
    "/": "E",
    "E": "1",
    "1": "d",
    "d": "o",
    "o": "z",
    "z": "0",
    "0": "C",
    "C": "O",
    "O": "x",
    "x": "m",
    "m": "I",
    "I": "A",
    "A": "K",
    "K": "=",
    "=": "b",
    "b": "v",
    "v": "q",
    "q": "j",
    "j": "u",
    "u": "M",
    "M": "6",
    "6": "P",
    "P": "H",
    "H": "J",
    "J": "L",
    "L": "y",
    "y": "V",
    "V": "f",
    "f": "7",
    "7": "N",
    "N": "p",
    "p": "W",
    "W": "R"
  },
  "IsGZip": true,
  "CharSet": "utf-8"
}
```

然后加密:

```powershell
enc key.json pica.png pica.txt utf-8
```

这里输出结果放到了pica.txt中 ->  

[pica.txt]:.\pica.txt

最后解密:

```powershell
dec key.json pica.txt pica.decrypt.png utf-8
```

![pica.decrypt](README.assets/pica.decrypt.png)

------

## 贡献代码:

欢迎各位提交PR! 和修复BUG!

------

# 许可声明:

# Crypto Chinese PROJECT是MIAIONE版权的一部分。

 # #本程序使用GPLV3许可证，同时作者做了一些限制，如果与原许可证发生冲突，作者限制优先：

##  1 .该程序禁止任何商业目的。

##  2 .您必须开源您的修改代码。

##  3 .您必须在第二次放行时自带许可证，但不能违反上游许可证，否则无效。

##  4 .你可以选择不携带许可证，但你需要明确( 如 URL或程序内部信息 , 同时保留上游许可证信息 )。



## CryptoChinese PROJECT is part of the copyright of MIAIONE.

## This program uses GPLV3 license, while the author made some restrictions, if conflict with the original license, the author restrictions priority :

## 1.The procedure is prohibited for any commercial purpose.

## 2.YOU MUST open source your modified code.

## 3.YOU MUST take your own license in the second release, but can not violate the upstream license, otherwise invalid.

## 4.YOU CAN choose not to carry a license, but you need to make clear ( such as URL or program internal information, while retaining upstream license information ).

