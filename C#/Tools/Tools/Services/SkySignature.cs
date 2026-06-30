using System.Security.Cryptography;
using System.Text;

namespace Tools.Services
{
    public class SkySignature
    {
        private const string SignKey = "affa62e3b7376a0cbd20ea2f6c07072f";

        /// <summary>
        /// 生成 GL-CheckSum 请求头（由网易大神客户端逆向得出）
        /// </summary>
        /// <remarks>
        /// 算法来源：com.netease.gl.servicenet.proto.AbstractC40307c.signHeaders()
        /// 签名密钥硬编码在 Android 客户端内，值为 "affa62e3b7376a0cbd20ea2f6c07072f"
        /// 
        /// 签名逻辑：
        /// 1. 将密钥与以下请求头参数按顺序拼接（无分隔符）：
        ///    [密钥] + GL-ClientType + GL-CurTime + GL-DeviceId +
        ///    GL-Nonce + GL-Source + GL-Token + GL-Uid + GL-Version
        /// 2. 计算拼接字符串的 SHA1 哈希
        /// 3. 转换为全小写十六进制字符串
        /// </remarks>
        /// <param name="clientType">GL-ClientType 头的值，GL-ClientType 请求头的值。• iOS："51"• Android："50"</param>
        /// <param name="curTime">GL-CurTime 头的值（毫秒时间戳字符串）。/v1/app/init/server/time，建议使用服务器时间（如 /v1/app/init/server/time 返回的时间），也可用本地时间。</param>
        /// <param name="deviceId">GL-DeviceId 头的值。"1BFA9166-F683-44EE-8533-A91C697C5D87"。GL-Nonce 请求头的值，格式为 <毫秒时间戳>_<随机数>。毫秒时间戳应与 curTime 一致或相近，随机数可用 Random.Shared.NextInt64() 生成。"1782469165737_123456789"</param>
        /// <param name="nonce">GL-Nonce 头的值（格式：时间戳_随机数）</param>
        /// <param name="source">GL-Source 头的值（如 "URS"）</param>
        /// <param name="token">GL-Token 头的值</param>
        /// <param name="uid">GL-Uid 头的值</param>
        /// <param name="version">GL-Version 头的值（如 "4.19.2"）</param>
        /// <returns>GL-CheckSum 值（40位小写十六进制字符串）</returns>
        public static string GenerateCheckSum(
            string clientType,
            string curTime,
            string deviceId,
            string nonce,
            string source,
            string token,
            string uid,
            string version)
        {
            string raw = SignKey + clientType + curTime + deviceId +
                         nonce + source + token + uid + version;
            byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(hash).ToLower();
        }


        /// <summary>生成 GL-Nonce（服务器时间戳_随机数）</summary>
        /// <param name="serverTimeMillis">服务器毫秒时间戳，若为 null 则使用本地时间</param>
        public static string GenerateNonce(long? serverTimeMillis = null)
        {
            long millis = serverTimeMillis ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long random = Random.Shared.NextInt64();
            return $"{millis}_{random}";
        }
    }
}
