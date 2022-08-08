///导入命名空间
global using static CryptoChinese.CryptoCore;
using CryptoChinese;

internal class Program
{
    private static void Main()
    {
        //注册更多代码页编码,如GBK,GB2312
        InitEncoding();

        // string->byte[]->GZIP to byte[]->toBase64->if need to encrypt, replace some
        // letter->Replace Key Value Pair to Chinese words->Chinese

        // 版权信息
        var args = Environment.GetCommandLineArgs().ToList();
        args.RemoveAt(0);
        if (args.Count > 0)
        {
            var Optr = args[0].ToLower();
            args.RemoveAt(0);
            if (Optr == "gen")
            {
                var IskeyOk = int.TryParse(args[2], out int keysize);
                var IsgzipOk = bool.TryParse(args[3], out bool gzip);

                if (IskeyOk && IsgzipOk && args.LengthCheck(5))
                {
                    GenerateKey(args[1], keysize, gzip, args[4]).Save(args[0], args[4]);
                }
                else
                {
                    L("参数非法", LogType.Error);
                }
            }
            else if (Optr == "enc")
            {
                if (args.LengthCheck(4))
                {
                    Encrypt(args[1], args[2], Keystore.Load(args[0], args[3]));
                }
                else
                {
                    L("参数非法", LogType.Error);
                }
            }
            else if (Optr == "dec")
            {
                if (args.LengthCheck(4))
                {
                    Decrypt(args[1], args[2], Keystore.Load(args[0], args[3]));
                }
                else
                {
                    L("参数非法", LogType.Error);
                }
            }
            else
            {
                L("无效参数", LogType.Error);
            }
        }
        else
        {
            L("CryptoChinese PROJECT is part of the copyright of MIAIONE. ", LogType.Information);
            L("This program uses GPLV3 license, while the author made some restrictions, if conflict with the original license, the author restrictions priority : ", LogType.Information);
            L("1. The procedure is prohibited for any commercial purpose. ", LogType.Information);
            L("2. YOU MUST open source your modified code. ", LogType.Information);
            L("3. YOU MUST take your own license in the second release, but can not violate the upstream license, otherwise invalid. ", LogType.Information);
            L("4. YOU CAN choose not to carry a license, but you need to make clear ( such as URL or program internal information, while retaining upstream license information ). ", LogType.Information);
            L();
            L();
            L("命令参数: ", LogType.Information);
            L();
            L("生成密钥: gen [string:密钥保存路径] [string:字典路径] [int:加密表大小 范围 2-64] [bool:是否开启GZIP压缩 true/false] [string:字符集(如utf-8, utf-32, gbk, gb2312 等)]", LogType.Information);
            L("示例: gen \"C:\\key.txt\" \"C:\\dic.txt\" 64 true utf-8", LogType.Information);
            L("示例意为在C:根目录保存密钥key, 同时使用C:根目录下的字典文件, 加密表大小=64, 开启GZIP压缩, UTF-8编码", LogType.Information);
            L();
            L("加密文件: enc [string:密钥路径] [string:源文件路径] [string:加密后文件路径] [string:字符集]", LogType.Information);
            L("示例: enc \"C:\\key.txt\" \"C:\\video.mp4\" \"C:\\video.mp4.txt\"", LogType.Information);
            L("示例意为读取C:根目录下的密钥key, 加密C:下的video.mp4并保存为video.mp4.txt", LogType.Information);
            L();
            L("解密文件: dec [string:密钥路径] [string:加密后文件路径] [string:解密后文件路径] [string:字符集]", LogType.Information);
            L("示例: dec \"C:\\key.txt\" \"C:\\video.mp4.txt\" \"C:\\video.mp4\"", LogType.Information);
            L("示例意为读取C:根目录下的密钥key, 解密C:下的video.mp4.txt并保存为video.mp4", LogType.Information);
            L();
            L("生成密钥为了保证通用性, 强烈建议使用utf-8字符集", LogType.Warning);
            L("生成密钥时注意参数范围和类型", LogType.Warning);
            L("开启GZIP可减小加密后大小, 建议开启", LogType.Warning);
            L("路径有空格需要放入英文双引号内", LogType.Warning);
            L();
            Console.ReadKey();
        }
    }

     
}