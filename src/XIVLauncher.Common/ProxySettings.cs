using System;
using System.Net;
using Serilog;

namespace XIVLauncher.Common
{
    /// <summary>
    /// 全局代理设置管理器
    /// </summary>
    public static class ProxySettings
    {
        public static bool EnableProxy { get; set; } = false;
        public static string ProxyType { get; set; } = "SOCKS5";
        public static string ProxyServer { get; set; } = string.Empty;
        public static int ProxyPort { get; set; } = 1080;
        public static string ProxyUsername { get; set; } = string.Empty;
        public static string ProxyPassword { get; set; } = string.Empty;

        /// <summary>
        /// 获取配置的 WebProxy 对象（用于 HTTP/HTTPS 代理）
        /// </summary>
        public static IWebProxy GetWebProxy()
        {
            if (!EnableProxy || string.IsNullOrEmpty(ProxyServer))
            {
                Log.Verbose("[代理] GetWebProxy - 代理未启用 (EnableProxy={Enable}, ProxyServer={Server})",
                    EnableProxy, ProxyServer ?? "null");
                return null;
            }

            try
            {
                var proxyUri = new Uri($"http://{ProxyServer}:{ProxyPort}");
                var proxy = new WebProxy(proxyUri)
                {
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false
                };

                // 如果有认证信息
                if (!string.IsNullOrEmpty(ProxyUsername))
                {
                    proxy.Credentials = new NetworkCredential(ProxyUsername, ProxyPassword);
                    Log.Information("[代理] GetWebProxy 调用 - 返回带认证的HTTP代理: http://{ProxyServer}:{ProxyPort} (用户: {Username})",
                        ProxyServer, ProxyPort, ProxyUsername);
                }
                else
                {
                    Log.Information("[代理] GetWebProxy 调用 - 返回HTTP代理: http://{ProxyServer}:{ProxyPort}",
                        ProxyServer, ProxyPort);
                }

                return proxy;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[代理] 创建代理配置失败");
                return null;
            }
        }

        /// <summary>
        /// 设置系统代理（用于 SOCKS5）
        /// </summary>
        public static void ApplySystemProxy()
        {
            if (!EnableProxy || string.IsNullOrEmpty(ProxyServer))
            {
                return;
            }

            if (ProxyType == "SOCKS5")
            {
                Log.Warning("[代理] SOCKS5 代理需要使用第三方工具（如 Proxifier）或系统代理设置");
                Log.Information("[代理] 建议配置: socks5://{ProxyServer}:{ProxyPort}", ProxyServer, ProxyPort);
            }
        }

        /// <summary>
        /// 获取代理信息字符串
        /// </summary>
        public static string GetProxyInfo()
        {
            if (!EnableProxy)
            {
                return "[未启用代理]";
            }

            var auth = string.IsNullOrEmpty(ProxyUsername) ? "无认证" : $"认证用户: {ProxyUsername}";
            return $"[{ProxyType}] {ProxyServer}:{ProxyPort} ({auth})";
        }
    }
}
