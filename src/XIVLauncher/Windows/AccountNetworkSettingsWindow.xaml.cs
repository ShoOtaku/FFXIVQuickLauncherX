using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using XIVLauncher.Accounts;
using XIVLauncher.Common;

namespace XIVLauncher.Windows
{
    public partial class AccountNetworkSettingsWindow : Window
    {
        private readonly XivAccount _account;
        private List<DeviceIdProfile> _deviceIdProfiles = new();
        private List<ProxyProfile> _proxyProfiles = new();

        public AccountNetworkSettingsWindow(XivAccount account)
        {
            InitializeComponent();

            _account = account;
            AccountNameTextBlock.Text = account?.DisplayName ?? account?.UserName ?? "未知账号";

            LoadProfiles();
            LoadOverride();
        }

        private void LoadProfiles()
        {
            _deviceIdProfiles = LoadDeviceIdProfiles();
            DeviceIdProfileComboBox.ItemsSource = _deviceIdProfiles;
            DeviceIdProfileComboBox.IsEnabled = _deviceIdProfiles.Count > 0;
            DeviceIdProfileHintTextBlock.Visibility = _deviceIdProfiles.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            _proxyProfiles = LoadProxyProfiles();
            ProxyProfileComboBox.ItemsSource = _proxyProfiles;
            ProxyProfileComboBox.IsEnabled = _proxyProfiles.Count > 0;
            ProxyProfileHintTextBlock.Visibility = _proxyProfiles.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadOverride()
        {
            var overrideConfig = AccountNetworkOverrideStore.Get(_account?.Id) ??
                                 new AccountNetworkOverride { AccountId = _account?.Id };

            UseGlobalDeviceIdCheckBox.IsChecked = overrideConfig.UseGlobalDeviceId;
            UseGlobalProxyCheckBox.IsChecked = overrideConfig.UseGlobalProxy;

            if (!overrideConfig.UseGlobalDeviceId)
                SelectDeviceIdProfile(overrideConfig.DeviceIdProfileName);

            if (!overrideConfig.UseGlobalProxy)
                SelectProxyProfile(overrideConfig.ProxyProfileName);

            UpdatePanels();
        }

        private void SelectDeviceIdProfile(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var match = _deviceIdProfiles.FirstOrDefault(p => p.Name == name);
            if (match != null)
                DeviceIdProfileComboBox.SelectedItem = match;
        }

        private void SelectProxyProfile(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var match = _proxyProfiles.FirstOrDefault(p => p.Name == name);
            if (match != null)
                ProxyProfileComboBox.SelectedItem = match;
        }

        private void UpdatePanels()
        {
            DeviceIdProfilePanel.IsEnabled = UseGlobalDeviceIdCheckBox.IsChecked != true;
            ProxyProfilePanel.IsEnabled = UseGlobalProxyCheckBox.IsChecked != true;
        }

        private void UseGlobalDeviceIdCheckBox_OnChanged(object sender, RoutedEventArgs e)
        {
            UpdatePanels();
        }

        private void UseGlobalProxyCheckBox_OnChanged(object sender, RoutedEventArgs e)
        {
            UpdatePanels();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_account == null)
            {
                MessageBox.Show("账号无效，无法保存。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var useGlobalDeviceId = UseGlobalDeviceIdCheckBox.IsChecked == true;
            var useGlobalProxy = UseGlobalProxyCheckBox.IsChecked == true;

            if (!useGlobalDeviceId)
            {
                if (_deviceIdProfiles.Count == 0)
                {
                    MessageBox.Show("暂无机器码配置，请先在高级设置中创建。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (DeviceIdProfileComboBox.SelectedItem == null)
                {
                    MessageBox.Show("请选择机器码配置。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            if (!useGlobalProxy)
            {
                if (_proxyProfiles.Count == 0)
                {
                    MessageBox.Show("暂无代理配置，请先在高级设置中创建。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (ProxyProfileComboBox.SelectedItem == null)
                {
                    MessageBox.Show("请选择代理配置。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            var overrideConfig = AccountNetworkOverrideStore.Get(_account.Id) ??
                                 new AccountNetworkOverride { AccountId = _account.Id };

            overrideConfig.UseGlobalDeviceId = useGlobalDeviceId;
            overrideConfig.UseGlobalProxy = useGlobalProxy;
            overrideConfig.DeviceIdProfileName = useGlobalDeviceId
                ? null
                : (DeviceIdProfileComboBox.SelectedItem as DeviceIdProfile)?.Name;
            overrideConfig.ProxyProfileName = useGlobalProxy
                ? null
                : (ProxyProfileComboBox.SelectedItem as ProxyProfile)?.Name;

            AccountNetworkOverrideStore.Upsert(overrideConfig);

            if (App.AccountManager?.CurrentAccount?.Id == _account.Id)
                AccountNetworkOverrideStore.ApplyToAccount(_account);

            MessageBox.Show("已保存账号网络配置。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
            catch
            {
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
            catch
            {
                return new List<ProxyProfile>();
            }
        }
    }
}
