using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CryptoChinese
{
    internal static class CryptoCore
    {
        /// <summary>
        /// Base64字符
        /// </summary>
        public static char[] Base64Chars = @"QqWwEeRrTtYyUuIiOoPpAaSsDdFfGgHhJjKkLlZzXxCcVvBbNnMm/+=0123456789".ToCharArray();

        /// <summary>
        /// 非法字符
        /// </summary>
        public static char[] RemoveChars = (@"~!@#$%^&*()_+-=`{}:<>?\|[];',./" + "\"").ToCharArray();

        /// <summary>
        /// 移除非法字符的正则表达式
        /// </summary>
        public static string[] ToRemoveChars = { @"\s", @"\r", @"\n", @"\v", @"\t", @"\f", @"\cX" };

        public static JsonSerializerOptions JsOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// 初始化编码代码页
        /// </summary>
        public static void InitEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// 生成密钥
        /// </summary>
        /// <param name="dictionaryfilepath"> 对照表字典文件路径 </param>
        /// <param name="cryptoGroupCount"> 加密表数量,默认64,最大=64,最小=2 </param>
        /// <param name="isGzip"> 是否启用GZIP </param>
        /// <param name="encoding"> 密钥编码 </param>
        /// <returns> 密钥 </returns>
        public static Keystore GenerateKey(string dictionaryfilepath, int cryptoGroupCount = 64, bool isGzip = true, string encoding = "UTF-8")
        {
            if (!File.Exists(dictionaryfilepath))
            {
                L("生成密钥错误, 字典文件不存在.", LogType.Error);
                Exit(Exceptions.FileInvalid);
                return new();
            }

            var dictionarytext = File.ReadAllText(dictionaryfilepath, Encoding.GetEncoding(encoding));
            if (dictionarytext == null || dictionarytext.Length < Base64Chars.Length)
            {
                L("生成密钥错误, 字典文件为空或长度小于65个字符.", LogType.Error);
                Exit(Exceptions.LengthOverflow);
                return new();
            }
            if ((64 < cryptoGroupCount || cryptoGroupCount < 2))
            {
                L("生成密钥错误, 加密表最大=64,最小=2.", LogType.Error);
                Exit(Exceptions.FileInvalid);
                return new();
            }
            try
            {
                var key = new Keystore();
                var dictionaryCharArray = FilterStringToChars(dictionarytext);
                //----
                key.CharSet = encoding;
                key.IsGZip = isGzip;

                //生成加密表(当明文不满足Z字形首尾相接的字母环回数量时,不进行加密)
                var CharRndList = new List<char>();
                var StartOrEndChar = RandomOneCharFromArray(Base64Chars, ref CharRndList);
                var nextChar = RandomOneCharFromArray(Base64Chars, ref CharRndList);
                for (int i = 1; i <= cryptoGroupCount; i++)
                {
                    if (i == 1)
                    {
                        //头处理
                        key.KeysTable.Add(StartOrEndChar, nextChar);
                    }
                    else if (i == cryptoGroupCount)
                    {
                        //末尾处理
                        key.KeysTable.Add(nextChar, StartOrEndChar);
                    }
                    else
                    {
                        //中间处理
                        var oldChar = nextChar;
                        nextChar = RandomOneCharFromArray(Base64Chars, ref CharRndList);
                        key.KeysTable.Add(oldChar, nextChar);
                    }
                }
                //生成对照表
                var charIndex = 0;
                var charArray = RandomCharsFromArray(dictionaryCharArray);
                foreach (var c in Base64Chars)
                {
                    key.CharsWordsTable.Add(c, charArray[charIndex]);
                    charIndex++;
                }

                //----
                return key;
            }
            catch (Exception ex)
            {
                L("密钥生成错误.", LogType.Error);
                L("错误信息:\r\n" + ex.ToString(), LogType.Error);
                Exit(Exceptions.UncaughtException);
                return new();
            }
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="Defilepath"> 加密前的文件 </param>
        /// <param name="Enfilepath"> 加密后的文件 </param>
        /// <param name="key"> 密钥 </param>
        public static void Encrypt(string Defilepath, string Enfilepath, Keystore key)
        {
            if (!File.Exists(Defilepath))
            {
                L("加密错误, 文件不存在.", LogType.Error);
                Exit(Exceptions.FileInvalid);
                return;
            }
            var originBytes = File.ReadAllBytes(Defilepath);
            if (originBytes == null || originBytes.Length == 0)
            {
                L("加密错误, 文件为空.", LogType.Error);
                Exit(Exceptions.FileInvalid);
                return;
            }
            try
            {
                var compressedData = originBytes;
                if (key.IsGZip)
                {
                    compressedData = compressedData.Compress();
                }
                var base64CharArray = Convert.ToBase64String(compressedData).ToCharArray();
                var keyReplacedData = new List<char>();

                if (base64CharArray.ToList().ContainsAll(key.KeysTable.Keys.ToList()))
                {
                    foreach (var c in base64CharArray)
                    {
                        if (key.KeysTable.ContainsKey(c))
                        {
                            keyReplacedData.Add(key.KeysTable[c]);
                        }
                        else
                        {
                            keyReplacedData.Add(c);
                        }
                    }
                }
                else
                {
                    keyReplacedData = base64CharArray.ToList();
                }

                var LastArray = keyReplacedData.ToArray();
                var resultList = new List<char>();
                foreach (var c in LastArray)
                {
                    resultList.Add(key.CharsWordsTable[c]);
                }
                File.WriteAllText(Enfilepath, string.Join(string.Empty, resultList), Encoding.GetEncoding(key.CharSet));
            }
            catch (Exception ex)
            {
                L("加密错误.", LogType.Error);
                L("错误信息:\r\n" + ex.ToString(), LogType.Error);
                Exit(Exceptions.UncaughtException);
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="Enfilepath"> 加密后的文件 </param>
        /// <param name="Defilepath"> 解密后的文件 </param>
        /// <param name="key"> 密钥 </param>
        public static void Decrypt(string Enfilepath, string Defilepath, Keystore key)
        {
            if (!File.Exists(Enfilepath))
            {
                L("解密错误, 文件不存在.", LogType.Error);
                Exit(Exceptions.FileInvalid);
                return;
            }
            var originText = File.ReadAllText(Enfilepath, Encoding.GetEncoding(key.CharSet));
            if (originText == null || originText.Length == 0)
            {
                L("解密错误, 文件为空.", LogType.Error);
                Exit(Exceptions.FileInvalid);
                return;
            }
            try
            {
                originText = FilterString(originText);
                var keyArray = new List<char>();
                foreach (char c in originText)
                {
                    keyArray.Add(key.CharsWordsTable.Where(x => x.Value == c).FirstOrDefault().Key);
                }

                var originBase64Data = new List<char>();

                if (keyArray.ContainsAll(key.KeysTable.Keys.ToList()))
                {
                    foreach (var c in keyArray)
                    {
                        if (key.KeysTable.ContainsValue(c))
                        {
                            originBase64Data.Add(key.KeysTable.Where(x => x.Value == c).FirstOrDefault().Key);
                        }
                        else
                        {
                            originBase64Data.Add(c);
                        }
                    }
                }
                else
                {
                    originBase64Data = keyArray;
                }
                var compressedData = Convert.FromBase64String(string.Join(string.Empty, originBase64Data));
                var decompressedData = compressedData;
                if (key.IsGZip)
                {
                    decompressedData = decompressedData.Decompress();
                }
                File.WriteAllBytes(Defilepath, decompressedData);
            }
            catch (Exception ex)
            {
                L("解密错误.", LogType.Error);
                L("错误信息:\r\n" + ex.ToString(), LogType.Error);
                Exit(Exceptions.UncaughtException);
            }
        }

        /// <summary>
        /// 随机抽取一个不重复的字符在字典中
        /// </summary>
        /// <param name="chars"> 字典字符数组 </param>
        /// <param name="Rndlist"> 已经抽取的字符 </param>
        /// <returns> 单个字符 </returns>
        public static char RandomOneCharFromArray(char[] chars, ref List<char> Rndlist)
        {
            var rndChar = RandomOneCharFromArray(chars);
            while (Rndlist.Contains(rndChar))
            {
                rndChar = RandomOneCharFromArray(chars);
            }
            Rndlist.Add(rndChar);
            return rndChar;
        }

        /// <summary>
        /// 随机抽取一个字符在字典中
        /// </summary>
        /// <param name="chars"> 字典字符数组 </param>
        /// <returns> 单个字符 </returns>
        public static char RandomOneCharFromArray(char[] chars)
        {
            if (chars.LongLength > int.MaxValue)
            {
                L("替换字典太长.", LogType.Error);
                Exit(Exceptions.LengthOverflow);
            }
            var rndint = GenerateNoDuplicateRandom(0, chars.Length - 1, 1).FirstOrDefault();
            return chars[rndint];
        }

        /// <summary>
        /// 随机抽取字符数组在字典中
        /// </summary>
        /// <param name="chars"> 字典字符数组 </param>
        /// <returns> 字符数组 </returns>
        public static char[] RandomCharsFromArray(char[] chars)
        {
            if (chars.LongLength > int.MaxValue)
            {
                L($"替换字典太长, 长须需小于 int.MaxValue = {int.MaxValue}.", LogType.Error);
                Exit(Exceptions.LengthOverflow);
            }

            var charsArray = new List<char>();

            foreach (var rndint in GenerateNoDuplicateRandom(0, chars.Length - 1, Base64Chars.Length))
            {
                var newChar = chars[rndint];
                var failedCount = 0;
            reTrynewCahr:

                if (!charsArray.Contains(newChar))
                {
                    charsArray.Add(newChar);
                }
                else
                {
                    failedCount++;
                    if (failedCount >= 1024)
                    {
                        L($"字典含有重复字符 {newChar}.", LogType.Warning);
                        Exit(Exceptions.ParameterInvalid);
                    }
                    else
                    {
                        newChar = RandomOneCharFromArray(chars);
                        goto reTrynewCahr;
                    }
                }
            }

            return charsArray.ToArray();
        }

        /// <summary>
        /// 生成不重复的随机数
        /// </summary>
        /// <param name="minValue"> (包含)最小值 </param>
        /// <param name="maxValue"> (包含)最大值 </param>
        /// <param name="count"> 数量 </param>
        /// <returns> 随机数数组 </returns>
        public static List<int> GenerateNoDuplicateRandom(int minValue, int maxValue, int count)
        {
            var rndList = new List<int>();
            for (int i = 1; i <= count; i++)
            {
                rndList.Add(Random.Shared.Next(minValue, maxValue));
            }
            return rndList.OrderBy(g => Guid.NewGuid()).ToList();
        }

        /// <summary>
        /// 加密表/对照表 非法字符过滤器
        /// </summary>
        /// <param name="text"> 包含控制字符的文本 </param>
        /// <returns> 单个字符数组 </returns>
        public static char[] FilterStringToChars(string text)
        {
            var result = FilterString(text);
            var listnew = new List<char>();
            var charlist = result.ToCharArray();
            foreach (char c in charlist)
            {
                bool isneedRemove = false;
                foreach (char c2 in Base64Chars)
                {
                    if (c == c2)
                    {
                        isneedRemove = true;
                    }
                }
                foreach (char c2 in RemoveChars)
                {
                    if (c == c2)
                    {
                        isneedRemove = true;
                    }
                }
                if (!isneedRemove)
                {
                    listnew.Add(c);
                }
            }
            return listnew.ToArray();
        }

        /// <summary>
        /// 过滤字符
        /// </summary>
        /// <param name="text"> 文本 </param>
        /// <returns> 过滤后的文本 </returns>
        public static string FilterString(string text)
        {
            var result = text.Trim(); ;
            foreach (var c in ToRemoveChars)
            {
                result = Regex.Replace(result.Trim(), c, string.Empty);
            }
            return result;
        }

        public static void Exit(Exceptions exceptions)
        {
            Environment.Exit((int)exceptions);
        }

        public static void L(string text = "", LogType logType = LogType.Normal)
        {
            switch (logType)
            {
                case LogType.Information:
                    Console.WriteLine("[*] " + text);
                    break;

                case LogType.Warning:
                    Console.WriteLine("[!] " + text);
                    break;

                case LogType.Error:
                    Console.WriteLine("[-] " + text);
                    break;

                case LogType.Success:
                    Console.WriteLine("[+] " + text);
                    break;

                case LogType.Failed:
                    Console.WriteLine("[x] " + text);
                    break;

                case LogType.Normal:
                    Console.WriteLine(text);
                    break;

                default:
                    break;
            }
        }

        #region 扩展方法

        /// <summary>
        /// 判断一个列表里的元素是否在另一个列表中全部出现
        /// </summary>
        /// <typeparam name="T"> 泛型 </typeparam>
        /// <param name="ListA"> 被判断的列表 </param>
        /// <param name="ListB"> 提供元素的列表 </param>
        /// <returns> 是否全部存在 </returns>
        public static bool ContainsAll<T>(this List<T> ListA, List<T> ListB)
        {
            return ListB.All(b => ListA.Any(a => Equals(a, b)));
        }

        /// <summary>
        /// 判断列表count是否和指定长度相等
        /// </summary>
        /// <typeparam name="T"> 泛型 </typeparam>
        /// <param name="list"> 列表 </param>
        /// <param name="count"> 要判断的数量 </param>
        /// <returns> 是否相等 </returns>
        public static bool LengthCheck<T>(this List<T> list, int count)
        {
            return list.Count == count;
        }

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="data"> 数据 </param>
        /// <returns> 压缩后的数据 </returns>
        public static byte[] Compress(this byte[] data)
        {
            using var compressedStream = new MemoryStream();
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
            zipStream.Write(data, 0, data.Length);
            zipStream.Close();
            return compressedStream.ToArray();
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="data"> 数据 </param>
        /// <returns> 解压后的数据 </returns>
        public static byte[] Decompress(this byte[] data)
        {
            using var compressedStream = new MemoryStream(data);
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            zipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }

        #endregion 扩展方法

        #region 枚举

        [Flags]
        public enum Exceptions : byte
        {
            Success = 0x01,
            SignCheckInvalid = 0x02,
            ParameterInvalid = 0x03,
            LengthOverflow = 0x04,
            UncaughtException = 0x05,
            FileInvalid = 0x06,
            NullReference = 0x07,
            Failed = 0x08
        }

        public enum LogType
        {
            Normal,
            Information,
            Warning,
            Error,
            Success,
            Failed
        }

        #endregion 枚举
    }

    internal class Keystore
    {
        /// <summary>
        /// base64 对照表,直接替换字符
        /// </summary>
        public Dictionary<char, char> CharsWordsTable { get; set; } = new();

        /// <summary>
        /// 加密表,对还原后密文ToCharArray,然后替换base64中内容(加密=丢失信息),数量不限 当原文没有加密表里的对照字符时,等同于不加密
        /// </summary>
        public Dictionary<char, char> KeysTable { get; set; } = new();

        /// <summary>
        /// 是否启用压缩
        /// </summary>
        public bool IsGZip { get; set; } = true;

        /// <summary>
        /// 密钥编码
        /// </summary>
        public string CharSet { get; set; } = "UTF-8";

        /// <summary>
        /// 从文件加载密钥
        /// </summary>
        /// <param name="keypath"> 密钥文件路径 </param>
        public static Keystore Load(string keypath, string encoding = "UTF-8")
        {
            if (File.Exists(keypath))
            {
                try
                {
                    var keyfilestring = File.ReadAllText(keypath, Encoding.GetEncoding(encoding));
                    if (!string.IsNullOrEmpty(keyfilestring))
                    {
                        var result = JsonSerializer.Deserialize<Keystore>(keyfilestring, JsOptions);
                        if (result != null)
                        {
                            return result;
                        }
                        else
                        {
                            throw new JsonException();
                        }
                    }
                    else
                    {
                        throw new FileLoadException();
                    }
                }
                catch
                {
                    L("尝试加载密钥时发生错误,请检查文件是否损坏.", LogType.Error);
                    Exit(Exceptions.FileInvalid);
                    return new();
                }
            }
            else
            {
                L("密钥文件不存在.", LogType.Error);
                Exit(Exceptions.FileInvalid);
                return new();
            }
        }

        /// <summary>
        /// 保存密钥到文件
        /// </summary>
        /// <param name="keypath"> 密钥文件路径 </param>
        public void Save(string keypath, string encoding = "UTF-8")
        {
            if (File.Exists(keypath))
            {
                L("目标密钥文件存在,正在覆盖.", LogType.Warning);
            }
            try
            {
                var jsonText = JsonSerializer.Serialize(this, JsOptions);
                File.WriteAllText(keypath, jsonText, Encoding.GetEncoding(encoding));
            }
            catch
            {
                L("密钥输出错误,可能写入权限不足或密钥配置非法.", LogType.Error);
                Exit(Exceptions.FileInvalid);
            }
        }
    }
}