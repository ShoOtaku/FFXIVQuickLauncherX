using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using XIVLauncher.Common;

namespace XIVLauncher.Windows
{
    public partial class DeviceIdProfileManager : Window
    {
        private ObservableCollection<DeviceIdProfile> profiles;

        public DeviceIdProfileManager()
        {
            InitializeComponent();
            LoadProfiles();
            ProfileDataGrid.ItemsSource = profiles;
        }

        private void LoadProfiles()
        {
            var json = App.Settings.DeviceIdProfiles;
            if (!string.IsNullOrEmpty(json))
            {
                var list = JsonSerializer.Deserialize<List<DeviceIdProfile>>(json);
                profiles = new ObservableCollection<DeviceIdProfile>(list ?? new List<DeviceIdProfile>());
            }
            else
            {
                profiles = new ObservableCollection<DeviceIdProfile>();
            }
        }

        private void SaveProfiles()
        {
            var json = JsonSerializer.Serialize(profiles.ToList());
            App.Settings.DeviceIdProfiles = json;
        }

        private void NewProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var newProfile = new DeviceIdProfile();
            profiles.Add(newProfile);
            SaveProfiles();
            ProfileDataGrid.SelectedItem = newProfile;
        }

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileDataGrid.SelectedItem is DeviceIdProfile profile)
            {
                var result = MessageBox.Show(
                    $"确定要删除配置 \"{profile.Name}\" 吗?",
                    "确认删除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    profiles.Remove(profile);
                    SaveProfiles();
                }
            }
            else
            {
                MessageBox.Show("请先选择要删除的配置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ApplyProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileDataGrid.SelectedItem is DeviceIdProfile profile)
            {
                App.Settings.SpoofedMacAddress = profile.MacAddress;
                App.Settings.SpoofedCpuId = profile.CpuId;
                App.Settings.SpoofedDiskSerial = profile.DiskSerial;
                App.Settings.SelectedDeviceIdProfile = profile.Name;

                MessageBox.Show(
                    $"已应用配置: {profile.Name}\n\nMAC: {profile.MacAddress}\nCPU: {profile.CpuId}\nDisk: {profile.DiskSerial}",
                    "配置已应用",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("请先选择要应用的配置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GetCurrentDeviceIdButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mac = GetMacAddressHash();
                var cpu = GetCpuIdHash();
                var disk = GetDiskSerialHash();

                var newProfile = new DeviceIdProfile(
                    "当前机器码",
                    mac,
                    cpu,
                    disk,
                    "从当前设备获取"
                );

                profiles.Add(newProfile);
                SaveProfiles();
                ProfileDataGrid.SelectedItem = newProfile;

                MessageBox.Show(
                    $"已创建当前机器码配置:\n\nMAC: {mac}\nCPU: {cpu}\nDisk: {disk}",
                    "获取成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"获取机器码失败: {ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ProfileDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            SaveProfiles();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveProfiles();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private string GetMacAddressHash()
        {
            var mac = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                             nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault() ?? "000000000000";

            return ComputeMD5Hash(mac);
        }

        private string GetCpuIdHash()
        {
            try
            {
                var cpuId = string.Empty;
                using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        cpuId = obj["ProcessorId"]?.ToString() ?? "Unknown";
                        break;
                    }
                }
                return ComputeMD5Hash(cpuId);
            }
            catch
            {
                return ComputeMD5Hash("Unknown");
            }
        }

        private string GetDiskSerialHash()
        {
            try
            {
                var diskSerial = string.Empty;
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        diskSerial = obj["SerialNumber"]?.ToString()?.Trim() ?? "Unknown";
                        if (!string.IsNullOrEmpty(diskSerial))
                            break;
                    }
                }
                return ComputeMD5Hash(diskSerial);
            }
            catch
            {
                return ComputeMD5Hash("Unknown");
            }
        }

        private string ComputeMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
