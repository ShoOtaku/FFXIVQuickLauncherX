using System;

namespace XIVLauncher.Common
{
    /// <summary>
    /// 机器码配置文件
    /// </summary>
    public class DeviceIdProfile
    {
        /// <summary>
        /// 配置名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// MAC地址哈希 (MD5)
        /// </summary>
        public string MacAddress { get; set; }

        /// <summary>
        /// CPU ID哈希 (MD5)
        /// </summary>
        public string CpuId { get; set; }

        /// <summary>
        /// 磁盘序列号哈希 (MD5)
        /// </summary>
        public string DiskSerial { get; set; }

        /// <summary>
        /// 备注说明
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreatedTime { get; set; }

        public DeviceIdProfile()
        {
            Name = "新建配置";
            MacAddress = "";
            CpuId = "";
            DiskSerial = "";
            Remark = "";
            CreatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public DeviceIdProfile(string name, string mac, string cpu, string disk, string remark = "")
        {
            Name = name;
            MacAddress = mac;
            CpuId = cpu;
            DiskSerial = disk;
            Remark = remark;
            CreatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 获取完整的机器码
        /// </summary>
        public string GetFullDeviceId()
        {
            return $"{MacAddress}:{CpuId}:{DiskSerial}";
        }

        /// <summary>
        /// 克隆配置
        /// </summary>
        public DeviceIdProfile Clone()
        {
            return new DeviceIdProfile
            {
                Name = this.Name,
                MacAddress = this.MacAddress,
                CpuId = this.CpuId,
                DiskSerial = this.DiskSerial,
                Remark = this.Remark,
                CreatedTime = this.CreatedTime
            };
        }

        public override string ToString()
        {
            return $"{Name} ({GetFullDeviceId().Substring(0, Math.Min(20, GetFullDeviceId().Length))}...)";
        }
    }
}
