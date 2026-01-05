using System;
using System.Collections.Generic;

namespace XIVLauncher.Common
{
    /// <summary>
    /// 代理配置文件
    /// </summary>
    public class ProxyProfile
    {
        /// <summary>
        /// 配置名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 代理类型 (SOCKS5 或 HTTP)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 代理服务器地址
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 代理端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 用户名 (可选)
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 密码 (可选)
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 备注说明
        /// </summary>
        public string Remark { get; set; }

        public ProxyProfile()
        {
            Name = "新建代理";
            Type = "SOCKS5";
            Server = "";
            Port = 1080;
            Username = "";
            Password = "";
            Remark = "";
        }

        public ProxyProfile(string name, string type, string server, int port, string username = "", string password = "", string remark = "")
        {
            Name = name;
            Type = type;
            Server = server;
            Port = port;
            Username = username;
            Password = password;
            Remark = remark;
        }

        /// <summary>
        /// 克隆配置
        /// </summary>
        public ProxyProfile Clone()
        {
            return new ProxyProfile
            {
                Name = this.Name,
                Type = this.Type,
                Server = this.Server,
                Port = this.Port,
                Username = this.Username,
                Password = this.Password,
                Remark = this.Remark
            };
        }

        public override string ToString()
        {
            return $"{Name} ({Type} - {Server}:{Port})";
        }
    }
}
