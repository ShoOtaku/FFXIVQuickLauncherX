using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Serilog;
using XIVLauncher.Common;

namespace XIVLauncher.Windows
{
    public partial class ProxyProfileManager : Window
    {
        private ObservableCollection<ProxyProfile> profiles;
        private bool isModified = false;

        public ProxyProfileManager()
        {
            InitializeComponent();
            LoadProfiles();
            ProfileDataGrid.ItemsSource = profiles;
        }

        private void LoadProfiles()
        {
            try
            {
                var json = App.Settings.ProxyProfiles;
                if (!string.IsNullOrEmpty(json))
                {
                    var list = JsonSerializer.Deserialize<List<ProxyProfile>>(json);
                    profiles = new ObservableCollection<ProxyProfile>(list ?? new List<ProxyProfile>());
                }
                else
                {
                    profiles = new ObservableCollection<ProxyProfile>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载代理配置失败");
                profiles = new ObservableCollection<ProxyProfile>();
            }
        }

        private void SaveProfiles()
        {
            try
            {
                var json = JsonSerializer.Serialize(profiles.ToList());
                App.Settings.ProxyProfiles = json;
                isModified = false;
                Log.Information("[代理配置] 已保存 {Count} 个配置", profiles.Count);
                MessageBox.Show("保存成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保存代理配置失败");
                MessageBox.Show("保存配置失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newProfile = new ProxyProfile
            {
                Name = $"配置{profiles.Count + 1}",
                Type = "SOCKS5",
                Server = "127.0.0.1",
                Port = 1080,
                Username = "",
                Password = "",
                Remark = ""
            };

            profiles.Add(newProfile);
            isModified = true;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileDataGrid.SelectedItem == null)
            {
                MessageBox.Show("请先选择要删除的配置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var profile = ProfileDataGrid.SelectedItem as ProxyProfile;
            var result = MessageBox.Show($"确定要删除配置 \"{profile.Name}\" 吗?", "确认删除",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                profiles.Remove(profile);
                isModified = true;
            }
        }

        private async void TestSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileDataGrid.SelectedItem == null)
            {
                MessageBox.Show("请先选择要测试的配置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var profile = ProfileDataGrid.SelectedItem as ProxyProfile;
            TestResultTextBlock.Visibility = Visibility.Collapsed;

            try
            {
                if (string.IsNullOrWhiteSpace(profile.Server))
                {
                    ShowTestResult($"配置 \"{profile.Name}\" 缺少服务器地址", false);
                    return;
                }

                var proxyUri = $"{profile.Type.ToLower()}://{profile.Server}:{profile.Port}";
                var proxy = new System.Net.WebProxy(proxyUri);

                if (!string.IsNullOrWhiteSpace(profile.Username))
                {
                    proxy.Credentials = new System.Net.NetworkCredential(profile.Username, profile.Password);
                }

                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true
                };

                using var httpClient = new HttpClient(handler);
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await httpClient.GetAsync("https://cas.sdo.com/authen/");
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    ShowTestResult($"✓ 配置 \"{profile.Name}\" 连接成功! 响应时间: {stopwatch.ElapsedMilliseconds}ms", true);
                    Log.Information($"[代理测试] 配置 {profile.Name} 测试成功,耗时 {stopwatch.ElapsedMilliseconds}ms");
                }
                else
                {
                    ShowTestResult($"✗ 配置 \"{profile.Name}\" 连接失败: HTTP {(int)response.StatusCode}", false);
                }
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                ShowTestResult($"✗ 配置 \"{profile.Name}\" 连接超时", false);
            }
            catch (Exception ex)
            {
                ShowTestResult($"✗ 配置 \"{profile.Name}\" 连接失败: {ex.Message}", false);
            }
        }

        private void ShowTestResult(string message, bool success)
        {
            TestResultTextBlock.Text = message;
            TestResultTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                success ? System.Windows.Media.Colors.Green : System.Windows.Media.Colors.Red
            );
            TestResultTextBlock.Visibility = Visibility.Visible;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveProfiles();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isModified)
            {
                var result = MessageBox.Show("有未保存的修改,是否保存?", "提示",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SaveProfiles();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            Close();
        }

        private void ProfileDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            isModified = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (isModified)
            {
                var result = MessageBox.Show("有未保存的修改,是否保存?", "提示",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SaveProfiles();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            base.OnClosing(e);
        }
    }
}
