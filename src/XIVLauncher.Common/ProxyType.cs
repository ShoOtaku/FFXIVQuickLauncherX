namespace XIVLauncher.Common
{
    public enum ProxyType
    {
        /// <summary>
        /// SOCKS5 代理（推荐）- 支持 TCP/UDP，支持认证
        /// </summary>
        SOCKS5,

        /// <summary>
        /// HTTP 代理 - 仅支持 HTTP 协议
        /// </summary>
        HTTP,

        /// <summary>
        /// SOCKS4 代理 - 旧版本，不支持 UDP 和认证
        /// </summary>
        SOCKS4
    }
}
