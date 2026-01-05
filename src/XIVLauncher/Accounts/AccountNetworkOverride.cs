using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Serilog;
using XIVLauncher.Common;
using XIVLauncher.Common.Game;

namespace XIVLauncher.Accounts
{
    public class AccountNetworkOverride
    {
        public string AccountId { get; set; }
        public bool UseGlobalDeviceId { get; set; } = true;
        public string DeviceIdProfileName { get; set; }
        public bool UseGlobalProxy { get; set; } = true;
        public string ProxyProfileName { get; set; }
    }

    public static class AccountNetworkOverrideStore
    {
        public static AccountNetworkOverride Get(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return null;

            return Load().FirstOrDefault(x => x.AccountId == accountId);
        }

        public static void Upsert(AccountNetworkOverride overrideConfig)
        {
            if (overrideConfig == null || string.IsNullOrWhiteSpace(overrideConfig.AccountId))
                return;

            var list = Load();
            var existing = list.FirstOrDefault(x => x.AccountId == overrideConfig.AccountId);
            if (existing == null)
            {
                list.Add(overrideConfig);
            }
            else
            {
                existing.UseGlobalDeviceId = overrideConfig.UseGlobalDeviceId;
                existing.DeviceIdProfileName = overrideConfig.DeviceIdProfileName;
                existing.UseGlobalProxy = overrideConfig.UseGlobalProxy;
                existing.ProxyProfileName = overrideConfig.ProxyProfileName;
            }

            Save(list);
        }

        public static void Remove(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return;

            var list = Load();
            var removed = list.RemoveAll(x => x.AccountId == accountId) > 0;
            if (removed)
                Save(list);
        }

        public static void ApplyToAccount(XivAccount account)
        {
            if (account == null)
            {
                ApplyGlobalSettings();
                return;
            }

            var overrideConfig = Get(account.Id);
            if (overrideConfig == null)
            {
                ApplyGlobalSettings();
                return;
            }

            ApplyDeviceIdOverride(overrideConfig);
            ApplyProxyOverride(overrideConfig);
        }

        private static void ApplyGlobalSettings()
        {
            ApplyGlobalDeviceId();
            ApplyGlobalProxy();
        }

        private static void ApplyGlobalDeviceId()
        {
            SdoUtils.EnableSpoof = App.Settings.EnableDeviceIdSpoof.GetValueOrDefault(false);
            SdoUtils.SpoofedMacAddress = App.Settings.SpoofedMacAddress;
            SdoUtils.SpoofedCpuId = App.Settings.SpoofedCpuId;
            SdoUtils.SpoofedDiskSerial = App.Settings.SpoofedDiskSerial;
        }

        private static void ApplyGlobalProxy()
        {
            ProxySettings.EnableProxy = App.Settings.EnableProxy.GetValueOrDefault(false);
            ProxySettings.ProxyType = App.Settings.ProxyType ?? "SOCKS5";
            ProxySettings.ProxyServer = App.Settings.ProxyServer ?? string.Empty;
            ProxySettings.ProxyPort = App.Settings.ProxyPort.GetValueOrDefault(1080);
            ProxySettings.ProxyUsername = App.Settings.ProxyUsername ?? string.Empty;
            ProxySettings.ProxyPassword = App.Settings.ProxyPassword ?? string.Empty;

            if (ProxySettings.EnableProxy)
                ProxySettings.ApplySystemProxy();
        }

        private static void ApplyDeviceIdOverride(AccountNetworkOverride overrideConfig)
        {
            if (overrideConfig.UseGlobalDeviceId)
            {
                ApplyGlobalDeviceId();
                return;
            }

            var profile = FindDeviceIdProfile(overrideConfig.DeviceIdProfileName);
            if (profile == null)
            {
                SdoUtils.EnableSpoof = false;
                SdoUtils.SpoofedMacAddress = null;
                SdoUtils.SpoofedCpuId = null;
                SdoUtils.SpoofedDiskSerial = null;
                return;
            }

            SdoUtils.EnableSpoof = true;
            SdoUtils.SpoofedMacAddress = profile.MacAddress;
            SdoUtils.SpoofedCpuId = profile.CpuId;
            SdoUtils.SpoofedDiskSerial = profile.DiskSerial;
        }

        private static void ApplyProxyOverride(AccountNetworkOverride overrideConfig)
        {
            if (overrideConfig.UseGlobalProxy)
            {
                ApplyGlobalProxy();
                return;
            }

            var profile = FindProxyProfile(overrideConfig.ProxyProfileName);
            if (profile == null)
            {
                ProxySettings.EnableProxy = false;
                ProxySettings.ProxyType = "SOCKS5";
                ProxySettings.ProxyServer = string.Empty;
                ProxySettings.ProxyPort = 1080;
                ProxySettings.ProxyUsername = string.Empty;
                ProxySettings.ProxyPassword = string.Empty;
                return;
            }

            ProxySettings.EnableProxy = true;
            ProxySettings.ProxyType = profile.Type ?? "SOCKS5";
            ProxySettings.ProxyServer = profile.Server ?? string.Empty;
            ProxySettings.ProxyPort = profile.Port;
            ProxySettings.ProxyUsername = profile.Username ?? string.Empty;
            ProxySettings.ProxyPassword = profile.Password ?? string.Empty;

            if (ProxySettings.EnableProxy)
                ProxySettings.ApplySystemProxy();
        }

        private static List<DeviceIdProfile> LoadDeviceIdProfiles()
        {
            var json = App.Settings.DeviceIdProfiles;
            if (string.IsNullOrWhiteSpace(json))
                return new List<DeviceIdProfile>();

            try
            {
                return JsonSerializer.Deserialize<List<DeviceIdProfile>>(json) ?? new List<DeviceIdProfile>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[账号配置] 解析机器码配置列表失败");
                return new List<DeviceIdProfile>();
            }
        }

        private static List<ProxyProfile> LoadProxyProfiles()
        {
            var json = App.Settings.ProxyProfiles;
            if (string.IsNullOrWhiteSpace(json))
                return new List<ProxyProfile>();

            try
            {
                return JsonSerializer.Deserialize<List<ProxyProfile>>(json) ?? new List<ProxyProfile>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[账号配置] 解析代理配置列表失败");
                return new List<ProxyProfile>();
            }
        }

        private static DeviceIdProfile FindDeviceIdProfile(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return LoadDeviceIdProfiles().FirstOrDefault(p => p.Name == name);
        }

        private static ProxyProfile FindProxyProfile(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return LoadProxyProfiles().FirstOrDefault(p => p.Name == name);
        }

        private static List<AccountNetworkOverride> Load()
        {
            var json = App.Settings.AccountNetworkOverrides;
            if (string.IsNullOrWhiteSpace(json))
                return new List<AccountNetworkOverride>();

            try
            {
                return JsonSerializer.Deserialize<List<AccountNetworkOverride>>(json) ?? new List<AccountNetworkOverride>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[账号配置] 解析账号网络配置失败");
                return new List<AccountNetworkOverride>();
            }
        }

        private static void Save(List<AccountNetworkOverride> list)
        {
            App.Settings.AccountNetworkOverrides = JsonSerializer.Serialize(list ?? new List<AccountNetworkOverride>());
        }
    }
}
