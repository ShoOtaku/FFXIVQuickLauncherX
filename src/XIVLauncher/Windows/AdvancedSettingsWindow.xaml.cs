using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Windows;
using XIVLauncher.Common;
using XIVLauncher.Common.Game;
using XIVLauncher.Common.Support;
using XIVLauncher.Windows.ViewModel;

namespace XIVLauncher.Windows
{
    /// <summary>
    ///     Interaction logic for FirstTimeSetup.xaml
    /// </summary>
    public partial class AdvancedSettingsWindow : Window
    {
        public bool WasCompleted { get; private set; } = false;

        public AdvancedSettingsWindow()
        {
            InitializeComponent();

            this.DataContext = new AdvancedSettingsViewModel();
            Load();
        }

        private void Load()
        {
            UidCacheCheckBox.IsChecked = App.Settings.UniqueIdCacheEnabled;
            ExitLauncherAfterGameExitCheckbox.IsChecked = App.Settings.ExitLauncherAfterGameExit ?? true;
            TreatNonZeroExitCodeAsFailureCheckbox.IsChecked = App.Settings.TreatNonZeroExitCodeAsFailure ?? false;
            ForceNorthAmericaCheckbox.IsChecked = App.Settings.ForceNorthAmerica ?? false;
            EnableBeta.IsChecked = App.Settings.EnableBeta ?? false;
            EnableSkipUpdate.IsChecked = App.Settings.EnableSkipUpdate ?? false;
            EnableVerboseLog.IsChecked = LogInit.LevelSwitch.MinimumLevel == LogEventLevel.Verbose;

            // 加载当前机器码信息
            RefreshCurrentDeviceId();

            // 加载机器码伪装设置
            EnableDeviceIdSpoofCheckbox.IsChecked = App.Settings.EnableDeviceIdSpoof ?? false;
            SpoofedMacAddressTextBox.Text = App.Settings.SpoofedMacAddress ?? string.Empty;
            SpoofedCpuIdTextBox.Text = App.Settings.SpoofedCpuId ?? string.Empty;
            SpoofedDiskSerialTextBox.Text = App.Settings.SpoofedDiskSerial ?? string.Empty;
            DeviceIdSpoofPanel.IsEnabled = EnableDeviceIdSpoofCheckbox.IsChecked == true;

            // 加载代理配置列表
            LoadProxyProfiles();

            // 加载机器码配置列表
            LoadDeviceIdProfiles();

            // 加载代理设置
            EnableProxyCheckbox.IsChecked = App.Settings.EnableProxy ?? false;
            ProxyTypeComboBox.SelectedIndex = (App.Settings.ProxyType ?? "SOCKS5") == "HTTP" ? 1 : 0;
            ProxyServerTextBox.Text = App.Settings.ProxyServer ?? string.Empty;
            ProxyPortTextBox.Text = (App.Settings.ProxyPort ?? 1080).ToString();
            ProxyUsernameTextBox.Text = App.Settings.ProxyUsername ?? string.Empty;
            ProxyPasswordBox.Password = App.Settings.ProxyPassword ?? string.Empty;
            ProxySettingsPanel.IsEnabled = EnableProxyCheckbox.IsChecked == true;

            // 加载仅扫码登录设置
            OnlyQRCodeLoginCheckbox.IsChecked = App.Settings.OnlyQRCodeLogin ?? false;
        }

        private void RefreshCurrentDeviceId()
        {
            try
            {
                // 获取当前实际使用的机器码（可能是真实的或伪装的）
                var currentDeviceId = SdoUtils.GetDeviceId();
                var currentMac = SdoUtils.GetMacAddress();
                var currentCpu = SdoUtils.GetCPUId();
                var currentDisk = SdoUtils.GetDiskSerialNumber();

                var statusText = SdoUtils.EnableSpoof ? "[已启用伪装]" : "[使用真实机器码]";

                CurrentDeviceIdTextBox.Text =
                    $"{statusText}\n" +
                    $"完整机器码: {currentDeviceId}\n\n" +
                    $"MAC 地址 MD5: {currentMac}\n" +
                    $"CPU ID MD5: {currentCpu}\n" +
                    $"磁盘序列号 MD5: {currentDisk}";
            }
            catch (Exception ex)
            {
                CurrentDeviceIdTextBox.Text = $"获取机器码失败: {ex.Message}";
                Log.Error(ex, "Failed to get current device ID");
            }
        }

        private void Save()
        {
            App.Settings.UniqueIdCacheEnabled          = UidCacheCheckBox.IsChecked                      == true;
            App.Settings.ExitLauncherAfterGameExit     = ExitLauncherAfterGameExitCheckbox.IsChecked     == true;
            App.Settings.TreatNonZeroExitCodeAsFailure = TreatNonZeroExitCodeAsFailureCheckbox.IsChecked == true;
            App.Settings.ForceNorthAmerica             = ForceNorthAmericaCheckbox.IsChecked             == true;
            App.Settings.EnableBeta                    = EnableBeta.IsChecked                            == true;
            App.Settings.EnableSkipUpdate              = EnableSkipUpdate.IsChecked                      == true;
            App.Settings.EnableVerboseLog              = EnableVerboseLog.IsChecked                      == true;
            LogInit.LevelSwitch.MinimumLevel           = this.EnableVerboseLog.IsChecked == true ? LogEventLevel.Verbose : LogInit.GetDefaultLevel();

            // 保存机器码伪装设置
            App.Settings.EnableDeviceIdSpoof = EnableDeviceIdSpoofCheckbox.IsChecked == true;
            App.Settings.SpoofedMacAddress = SpoofedMacAddressTextBox.Text;
            App.Settings.SpoofedCpuId = SpoofedCpuIdTextBox.Text;
            App.Settings.SpoofedDiskSerial = SpoofedDiskSerialTextBox.Text;

            // 应用机器码伪装设置到 SdoUtils
            SdoUtils.EnableSpoof = App.Settings.EnableDeviceIdSpoof.GetValueOrDefault(false);
            SdoUtils.SpoofedMacAddress = App.Settings.SpoofedMacAddress;
            SdoUtils.SpoofedCpuId = App.Settings.SpoofedCpuId;
            SdoUtils.SpoofedDiskSerial = App.Settings.SpoofedDiskSerial;

            // 保存代理设置
            App.Settings.EnableProxy = EnableProxyCheckbox.IsChecked == true;
            App.Settings.ProxyType = ((ComboBoxItem)ProxyTypeComboBox.SelectedItem).Content.ToString();
            App.Settings.ProxyServer = ProxyServerTextBox.Text;
            if (int.TryParse(ProxyPortTextBox.Text, out var port))
                App.Settings.ProxyPort = port;
            else
                App.Settings.ProxyPort = 1080;
            App.Settings.ProxyUsername = ProxyUsernameTextBox.Text;
            App.Settings.ProxyPassword = ProxyPasswordBox.Password;

            // 应用代理设置到 ProxySettings
            ProxySettings.EnableProxy = App.Settings.EnableProxy.GetValueOrDefault(false);
            ProxySettings.ProxyType = App.Settings.ProxyType;
            ProxySettings.ProxyServer = App.Settings.ProxyServer;
            ProxySettings.ProxyPort = App.Settings.ProxyPort.GetValueOrDefault(1080);
            ProxySettings.ProxyUsername = App.Settings.ProxyUsername;
            ProxySettings.ProxyPassword = App.Settings.ProxyPassword;

            if (ProxySettings.EnableProxy)
            {
                Log.Information("[代理] 保存代理设置: {ProxyInfo}", ProxySettings.GetProxyInfo());
            }

            // 保存仅扫码登录设置
            App.Settings.OnlyQRCodeLogin = OnlyQRCodeLoginCheckbox.IsChecked == true;
            if (App.Settings.OnlyQRCodeLogin == true)
            {
                Log.Information("[仅扫码登录] 已启用 - 将跳过服务器更新检查");
            }
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Save();
            Close();
        }

        private void ResetCacheButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.UniqueIdCache.Reset();
        }

        private void RefreshCurrentDeviceIdButton_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshCurrentDeviceId();
        }

        private void EnableDeviceIdSpoofCheckbox_OnChecked(object sender, RoutedEventArgs e)
        {
            DeviceIdSpoofPanel.IsEnabled = true;
            // 临时应用伪装设置以显示效果
            SdoUtils.EnableSpoof = true;
            SdoUtils.SpoofedMacAddress = SpoofedMacAddressTextBox.Text;
            SdoUtils.SpoofedCpuId = SpoofedCpuIdTextBox.Text;
            SdoUtils.SpoofedDiskSerial = SpoofedDiskSerialTextBox.Text;
            RefreshCurrentDeviceId();
        }

        private void EnableDeviceIdSpoofCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            DeviceIdSpoofPanel.IsEnabled = false;
            // 临时禁用伪装以显示真实机器码
            SdoUtils.EnableSpoof = false;
            RefreshCurrentDeviceId();
        }

        private void GenerateRandomDeviceIdButton_OnClick(object sender, RoutedEventArgs e)
        {
            var (mac, cpu, disk) = SdoUtils.GenerateRandomDeviceId();
            SpoofedMacAddressTextBox.Text = mac;
            SpoofedCpuIdTextBox.Text = cpu;
            SpoofedDiskSerialTextBox.Text = disk;

            // 更新 SdoUtils 中的伪装值并刷新显示
            if (EnableDeviceIdSpoofCheckbox.IsChecked == true)
            {
                SdoUtils.SpoofedMacAddress = mac;
                SdoUtils.SpoofedCpuId = cpu;
                SdoUtils.SpoofedDiskSerial = disk;
                RefreshCurrentDeviceId();
            }

            MessageBox.Show(
                $"已生成随机机器码:\n\n" +
                $"MAC: {mac}\n" +
                $"CPU: {cpu}\n" +
                $"Disk: {disk}\n\n" +
                $"完整机器码: {mac}:{cpu}:{disk}",
                "随机机器码",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ShowRealDeviceIdButton_OnClick(object sender, RoutedEventArgs e)
        {
            // 临时禁用伪装以获取真实机器码
            var wasEnabled = SdoUtils.EnableSpoof;
            SdoUtils.EnableSpoof = false;

            try
            {
                var realMac = SdoUtils.GetMacAddress();
                var realCpu = SdoUtils.GetCPUId();
                var realDisk = SdoUtils.GetDiskSerialNumber();
                var realDeviceId = SdoUtils.GetDeviceId();

                MessageBox.Show(
                    $"当前真实机器码:\n\n" +
                    $"MAC: {realMac}\n" +
                    $"CPU: {realCpu}\n" +
                    $"Disk: {realDisk}\n\n" +
                    $"完整机器码: {realDeviceId}",
                    "真实机器码",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            finally
            {
                SdoUtils.EnableSpoof = wasEnabled;
            }
        }

        private void EnableProxyCheckbox_OnChecked(object sender, RoutedEventArgs e)
        {
            ProxySettingsPanel.IsEnabled = true;
        }

        private void EnableProxyCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ProxySettingsPanel.IsEnabled = false;
        }

        private void LoadProxyProfiles()
        {
            try
            {
                ProxyProfileComboBox.Items.Clear();
                ProxyProfileComboBox.Items.Add("手动配置");

                var json = App.Settings.ProxyProfiles;
                if (!string.IsNullOrEmpty(json))
                {
                    var profiles = System.Text.Json.JsonSerializer.Deserialize<List<ProxyProfile>>(json);
                    if (profiles != null)
                    {
                        foreach (var profile in profiles)
                        {
                            ProxyProfileComboBox.Items.Add(profile);
                        }
                    }
                }

                // 设置当前选中项
                var selectedName = App.Settings.SelectedProxyProfile;
                if (!string.IsNullOrEmpty(selectedName))
                {
                    for (int i = 1; i < ProxyProfileComboBox.Items.Count; i++)
                    {
                        if (ProxyProfileComboBox.Items[i] is ProxyProfile profile && profile.Name == selectedName)
                        {
                            ProxyProfileComboBox.SelectedIndex = i;
                            return;
                        }
                    }
                }

                ProxyProfileComboBox.SelectedIndex = 0; // 默认选中"手动配置"
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载代理配置列表失败");
                ProxyProfileComboBox.Items.Add("手动配置");
                ProxyProfileComboBox.SelectedIndex = 0;
            }
        }

        private void ProxyProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProxyProfileComboBox.SelectedIndex <= 0)
            {
                // 选中"手动配置"
                App.Settings.SelectedProxyProfile = null;
                return;
            }

            var selected = ProxyProfileComboBox.SelectedItem;
            if (selected is ProxyProfile profile)
            {
                // 应用配置到UI
                ProxyTypeComboBox.SelectedIndex = profile.Type == "HTTP" ? 1 : 0;
                ProxyServerTextBox.Text = profile.Server;
                ProxyPortTextBox.Text = profile.Port.ToString();
                ProxyUsernameTextBox.Text = profile.Username;
                ProxyPasswordBox.Password = profile.Password;

                App.Settings.SelectedProxyProfile = profile.Name;
                Log.Information($"[代理配置] 已应用配置: {profile.Name}");
            }
        }

        private void ManageProxyProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            var managerWindow = new ProxyProfileManager();
            managerWindow.ShowDialog();

            // 刷新配置列表
            LoadProxyProfiles();
        }

        private async void TestProxyButton_OnClick(object sender, RoutedEventArgs e)
        {
            // 隐藏之前的测试结果
            ProxyTestResultTextBlock.Visibility = Visibility.Collapsed;
            TestProxyButton.IsEnabled = false;
            TestProxyButton.Content = "测试中...";

            try
            {
                // 获取当前代理设置
                var proxyType = ((ComboBoxItem)ProxyTypeComboBox.SelectedItem).Content.ToString();
                var proxyServer = ProxyServerTextBox.Text;
                var proxyPort = int.TryParse(ProxyPortTextBox.Text, out var port) ? port : 1080;
                var proxyUsername = ProxyUsernameTextBox.Text;
                var proxyPassword = ProxyPasswordBox.Password;

                if (string.IsNullOrWhiteSpace(proxyServer))
                {
                    ShowTestResult("请先填写代理服务器地址", false);
                    return;
                }

                // 创建临时代理配置
                var proxyUri = $"{proxyType.ToLower()}://{proxyServer}:{proxyPort}";
                var proxy = new System.Net.WebProxy(proxyUri);

                if (!string.IsNullOrWhiteSpace(proxyUsername))
                {
                    proxy.Credentials = new System.Net.NetworkCredential(proxyUsername, proxyPassword);
                }

                // 创建 HttpClient 测试连接
                var handler = new System.Net.Http.HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true
                };

                using var httpClient = new System.Net.Http.HttpClient(handler);
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                // 测试访问 SDO 登录服务器
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await httpClient.GetAsync("https://cas.sdo.com/authen/");
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    ShowTestResult($"✓ 代理连接成功! 响应时间: {stopwatch.ElapsedMilliseconds}ms", true);
                    Log.Information($"[代理测试] 成功连接到 SDO 服务器,耗时 {stopwatch.ElapsedMilliseconds}ms");
                }
                else
                {
                    ShowTestResult($"✗ 连接失败: HTTP {(int)response.StatusCode}", false);
                    Log.Warning($"[代理测试] 连接失败: {response.StatusCode}");
                }
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                ShowTestResult("✗ 连接超时,请检查代理设置", false);
                Log.Warning("[代理测试] 连接超时");
            }
            catch (Exception ex)
            {
                ShowTestResult($"✗ 连接失败: {ex.Message}", false);
                Log.Error(ex, "[代理测试] 测试代理连接时出错");
            }
            finally
            {
                TestProxyButton.IsEnabled = true;
                TestProxyButton.Content = "测试代理连接";
            }
        }

        private void ShowTestResult(string message, bool success)
        {
            ProxyTestResultTextBlock.Text = message;
            ProxyTestResultTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                success ? System.Windows.Media.Colors.Green : System.Windows.Media.Colors.Red
            );
            ProxyTestResultTextBlock.Visibility = Visibility.Visible;
        }

        // 辅助方法：从 SdoUtils 反射调用私有方法
        private static string GetCPUId()
        {
            var method = typeof(SdoUtils).GetMethod("GetCPUId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (string)method?.Invoke(null, null);
        }

        private static string GetDiskSerialNumber()
        {
            var method = typeof(SdoUtils).GetMethod("GetDiskSerialNumber",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (string)method?.Invoke(null, null);
        }

        // 机器码配置管理
        private void LoadDeviceIdProfiles()
        {
            DeviceIdProfileComboBox.Items.Clear();
            DeviceIdProfileComboBox.Items.Add("手动配置");

            var json = App.Settings.DeviceIdProfiles;
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var profiles = System.Text.Json.JsonSerializer.Deserialize<List<DeviceIdProfile>>(json);
                    foreach (var profile in profiles)
                    {
                        DeviceIdProfileComboBox.Items.Add(profile);
                    }

                    // 恢复之前选中的配置
                    if (!string.IsNullOrEmpty(App.Settings.SelectedDeviceIdProfile))
                    {
                        var selected = profiles.FirstOrDefault(p => p.Name == App.Settings.SelectedDeviceIdProfile);
                        if (selected != null)
                        {
                            DeviceIdProfileComboBox.SelectedItem = selected;
                        }
                        else
                        {
                            DeviceIdProfileComboBox.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        DeviceIdProfileComboBox.SelectedIndex = 0;
                    }
                }
                catch
                {
                    DeviceIdProfileComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                DeviceIdProfileComboBox.SelectedIndex = 0;
            }
        }

        private void DeviceIdProfileComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DeviceIdProfileComboBox.SelectedItem is DeviceIdProfile profile)
            {
                SpoofedMacAddressTextBox.Text = profile.MacAddress;
                SpoofedCpuIdTextBox.Text = profile.CpuId;
                SpoofedDiskSerialTextBox.Text = profile.DiskSerial;
                App.Settings.SelectedDeviceIdProfile = profile.Name;
            }
            else if (DeviceIdProfileComboBox.SelectedIndex == 0)
            {
                // 选择了"手动配置"
                App.Settings.SelectedDeviceIdProfile = null;
            }
        }

        private void ManageDeviceIdProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            var managerWindow = new DeviceIdProfileManager();
            managerWindow.ShowDialog();

            // 刷新配置列表
            LoadDeviceIdProfiles();
        }
    }
}
