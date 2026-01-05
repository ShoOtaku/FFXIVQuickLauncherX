using System.Collections.Generic;
using System.IO;
using XIVLauncher.Accounts.Cred;
using XIVLauncher.Common;
using XIVLauncher.Common.Addon;
using XIVLauncher.Common.Dalamud;
using XIVLauncher.Common.Game;
using XIVLauncher.Common.Game.Patch.Acquisition;
using XIVLauncher.Xaml;

namespace XIVLauncher.Settings
{
    public interface ILauncherSettingsV3
    {
        #region Launcher Setting

        DirectoryInfo GamePath { get; set; }
        bool AutologinEnabled { get; set; }
        List<AddonEntry> AddonList { get; set; }
        bool UniqueIdCacheEnabled { get; set; }
        string AdditionalLaunchArgs { get; set; }
        bool InGameAddonEnabled { get; set; }
        DalamudLoadMethod? InGameAddonLoadMethod { get; set; }
        bool OtpServerEnabled { get; set; }
        ClientLanguage? Language { get; set; }
        LauncherLanguage? LauncherLanguage { get; set; }
        string CurrentAccountId { get; set; }
        bool? EncryptArguments { get; set; }
        bool? EncryptArgumentsV2 { get; set; }
        DirectoryInfo PatchPath { get; set; }
        bool? AskBeforePatchInstall { get; set; }
        long SpeedLimitBytes { get; set; }
        decimal DalamudInjectionDelayMs { get; set; }
        bool? KeepPatches { get; set; }
        bool? HasComplainedAboutAdmin { get; set; }
        bool? HasComplainedAboutGShadeDxgi { get; set; }
        bool? HasComplainedAboutNoOtp { get; set; }
        string LastVersion { get; set; }
        AcquisitionMethod? PatchAcquisitionMethod { get; set; }
        string GitHubToken { get; set; }
        bool? HasShownAutoLaunchDisclaimer { get; set; }
        string AcceptLanguage { get; set; }
        DpiAwareness? DpiAwareness { get; set; }
        int? VersionUpgradeLevel { get; set; }
        bool? TreatNonZeroExitCodeAsFailure { get; set; }
        bool? ExitLauncherAfterGameExit { get; set; }
        bool? IsFt { get; set; }
        bool? AutoStartSteam { get; set; }
        bool? ForceNorthAmerica { get; set; }

        PreserveWindowPosition.WindowPlacement? MainWindowPlacement { get; set; }
        LoginType? SelectedLoginType { get; set; }
        int? SelectedServer { get; set; }
        bool FastLogin { get; set; }
        bool EnableInjector { get; set; }
        bool? EnableBeta { get; set; }
        bool? HasAgreeWeGameUsage { get; set; }
        bool? ShowWeGameTokenLogin { get; set; }
        CredType? CredType { get; set; }
        bool? EnableSkipUpdate { get; set; }
        bool? EnableVerboseLog { get; set; }

        // 机器码伪装设置
        bool? EnableDeviceIdSpoof { get; set; }
        string SpoofedMacAddress { get; set; }
        string SpoofedCpuId { get; set; }
        string SpoofedDiskSerial { get; set; }

        // 代理设置
        bool? EnableProxy { get; set; }
        string ProxyType { get; set; }  // SOCKS5, HTTP, SOCKS4
        string ProxyServer { get; set; }  // IP 或域名
        int? ProxyPort { get; set; }
        string ProxyUsername { get; set; }
        string ProxyPassword { get; set; }

        /// <summary>
        /// 仅扫码登录模式 - 跳过服务器检查，只访问扫码登录相关服务器
        /// </summary>
        bool? OnlyQRCodeLogin { get; set; }

        /// <summary>
        /// 代理配置列表 (JSON序列化的ProxyProfile列表)
        /// </summary>
        string ProxyProfiles { get; set; }

        /// <summary>
        /// 当前选中的代理配置名称
        /// </summary>
        string SelectedProxyProfile { get; set; }

        /// <summary>
        /// 机器码配置列表 (JSON序列化的DeviceIdProfile列表)
        /// </summary>
        string DeviceIdProfiles { get; set; }

        /// <summary>
        /// 当前选中的机器码配置名称
        /// </summary>
        string SelectedDeviceIdProfile { get; set; }

        /// <summary>
        /// 账号级机器码/代理覆盖配置(JSON序列化的AccountNetworkOverride列表)
        /// </summary>
        string AccountNetworkOverrides { get; set; }

        #endregion
    }
}
