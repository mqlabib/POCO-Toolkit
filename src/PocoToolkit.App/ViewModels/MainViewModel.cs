using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PocoToolkit.Contracts.Enums;
using PocoToolkit.Engine;

namespace PocoToolkit.App.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ToolkitEngine _engine;
    private readonly DispatcherTimer _deviceMonitorTimer;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private bool _disposed;
    private bool _hasCompletedInitialRefresh;
    private DeviceConnectionState? _lastConnectionState;

    [ObservableProperty]
    private string adbStatus = "Checking...";

    [ObservableProperty]
    private string adbVersion = "ADB Version: Checking...";

    [ObservableProperty]
    private string adbPath = "ADB Path: Checking...";

    [ObservableProperty]
    private string deviceStatus = "Checking...";

    [ObservableProperty]
    private string deviceModelDisplay = "Phone Model: Checking...";

    [ObservableProperty]
    private string deviceSerialDisplay = "Serial Number: Checking...";

    [ObservableProperty]
    private string deviceCodenameDisplay = "Codename: Checking...";

    [ObservableProperty]
    private string androidVersionDisplay = "Device: Checking...";

    [ObservableProperty]
    private string rootDisplay = "Root: Checking...";

    [ObservableProperty]
    private string bootloaderDisplay = "Bootloader: Checking...";

    [ObservableProperty]
    private IBrush rootStatusColor = Brushes.Gray;

    [ObservableProperty]
    private IBrush bootloaderStatusColor = Brushes.Gray;

    [ObservableProperty]
    private string romDisplay = "ROM: Checking...";

    [ObservableProperty]
    private string romBuildDisplay = "ROM Build: Checking...";

    public MainViewModel(ToolkitEngine engine)
    {
        _engine = engine;

        _deviceMonitorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };

        _deviceMonitorTimer.Tick += OnDeviceMonitorTick;

        _ = RefreshAsync();

        _deviceMonitorTimer.Start();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (_disposed)
            return;

        if (!await _refreshLock.WaitAsync(0))
            return;

        try
        {
            if (_disposed)
                return;

            var adbInstalled = await _engine.IsAdbInstalledAsync();

            if (!adbInstalled)
            {
                AdbStatus = "ADB: Not installed";
                AdbVersion = "ADB Version: Not available";
                AdbPath = "ADB Path: Not found";

                _lastConnectionState = DeviceConnectionState.NoDevice;

                SetDeviceUnavailable();

                DeviceStatus = "Device: Not connected";

                return;
            }

            AdbStatus = "ADB: Installed";

            var version = await _engine.GetAdbVersionAsync();

            AdbVersion = string.IsNullOrWhiteSpace(version)
                ? "ADB Version: Unknown"
                : $"ADB Version: {version}";

            var path = _engine.GetAdbPath();

            AdbPath = string.IsNullOrWhiteSpace(path)
                ? "ADB Path: Unknown"
                : $"ADB Path: {path}";

            var connectionState =
                await _engine.GetDeviceConnectionStateAsync();

            var connectionStateChanged =
                !_lastConnectionState.HasValue ||
                _lastConnectionState.Value != connectionState;

            _lastConnectionState = connectionState;

            switch (connectionState)
            {
                case DeviceConnectionState.NoDevice:
                    DeviceStatus = "Device: Not connected";

                    if (connectionStateChanged || !_hasCompletedInitialRefresh)
                        SetDeviceUnavailable();

                    return;

                case DeviceConnectionState.Unauthorized:
                    DeviceStatus = "Device: Unauthorized";

                    if (connectionStateChanged || !_hasCompletedInitialRefresh)
                        SetDeviceUnavailable();

                    return;

                case DeviceConnectionState.Offline:
                    DeviceStatus = "Device: Offline";

                    if (connectionStateChanged || !_hasCompletedInitialRefresh)
                        SetDeviceUnavailable();

                    return;

                case DeviceConnectionState.Connected:
                    break;

                default:
                    DeviceStatus = "Device: Unknown";

                    if (connectionStateChanged || !_hasCompletedInitialRefresh)
                        SetDeviceUnavailable();

                    return;
            }

            /*
             * Only show the transient "Reading..." state during
             * the initial connection or when the connection state changes.
             */
            if (connectionStateChanged)
                DeviceStatus = "Device: Reading...";

            var deviceInfo = await _engine.GetDeviceInfoAsync();

            if (deviceInfo is null)
            {
                /*
                 * Keep valid information during a transient polling
                 * failure while ADB still reports the device connected.
                 */
                if (connectionStateChanged || !_hasCompletedInitialRefresh)
                {
                    DeviceStatus = "Device: Connected";
                    SetDeviceUnknown();
                }

                _hasCompletedInitialRefresh = true;

                return;
            }

            ApplyDeviceInfo(deviceInfo);

            DeviceStatus = "Device: Connected";

            _hasCompletedInitialRefresh = true;
        }
        catch (Exception ex)
        {
            AdbStatus = "ADB: Error";
            AdbVersion = "ADB Version: Unable to read";
            AdbPath = "ADB Path: Unable to read";

            /*
             * Do not replace a known connected state with an error
             * during a normal background polling cycle.
             */
            if (!_hasCompletedInitialRefresh)
            {
                DeviceStatus = $"Device: Error - {ex.GetType().Name}";
                SetDeviceUnavailable();
            }
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private void ApplyDeviceInfo(
        PocoToolkit.Contracts.Models.DeviceInfo deviceInfo)
    {
        DeviceModelDisplay =
            string.IsNullOrWhiteSpace(deviceInfo.Model)
                ? string.IsNullOrWhiteSpace(deviceInfo.ModelNumber)
                    ? "Phone Model: Unknown"
                    : $"Phone Model: {deviceInfo.ModelNumber}"
                : string.IsNullOrWhiteSpace(deviceInfo.ModelNumber)
                    ? $"Phone Model: {deviceInfo.Model}"
                    : $"Phone Model: {deviceInfo.Model} ({deviceInfo.ModelNumber})";

        DeviceSerialDisplay =
            string.IsNullOrWhiteSpace(deviceInfo.SerialNumber)
                ? "Serial Number: Unknown"
                : $"Serial Number: {deviceInfo.SerialNumber}";

        DeviceCodenameDisplay =
            string.IsNullOrWhiteSpace(deviceInfo.DeviceCodename)
                ? "Codename: Unknown"
                : $"Codename: {FormatCodename(deviceInfo.DeviceCodename)}";

        /*
         * Android version is read directly from the device.
         * Only the human-readable codename is mapped locally.
         */
        AndroidVersionDisplay =
            string.IsNullOrWhiteSpace(deviceInfo.AndroidVersion)
                ? "Device: Unknown"
                : $"Device: Android {deviceInfo.AndroidVersion} ({GetAndroidCodename(deviceInfo.AndroidVersion)})";

        UpdateRootStatus(deviceInfo.RootState);

        UpdateBootloaderStatus(deviceInfo.BootloaderState);

        var romName = GetRomName(deviceInfo.RomName);

        var romVersion =
            string.IsNullOrWhiteSpace(deviceInfo.RomVersion)
                ? "Unknown"
                : deviceInfo.RomVersion.Trim();

        RomDisplay =
            $"ROM: {romName} ({romVersion})";

        var romBuild =
            string.IsNullOrWhiteSpace(deviceInfo.RomBuild)
                ? "Unknown"
                : deviceInfo.RomBuild.Trim();

        var romStatus =
            string.IsNullOrWhiteSpace(deviceInfo.RomStatus)
                ? "Unknown"
                : deviceInfo.RomStatus.Trim();

        RomBuildDisplay =
            $"ROM Build: {romBuild} ({romStatus})";
    }

    private void UpdateRootStatus(DeviceSecurityState state)
    {
        switch (state)
        {
            case DeviceSecurityState.Locked:
                RootDisplay = "Root: No";
                RootStatusColor = Brushes.LimeGreen;
                break;

            case DeviceSecurityState.Unlocked:
                RootDisplay = "Root: Yes (SU)";
                RootStatusColor = Brushes.Red;
                break;

            default:
                RootDisplay = "Root: Unknown";
                RootStatusColor = Brushes.Gray;
                break;
        }
    }

    private void UpdateBootloaderStatus(DeviceSecurityState state)
    {
        switch (state)
        {
            case DeviceSecurityState.Locked:
                BootloaderDisplay = "Bootloader: Locked";
                BootloaderStatusColor = Brushes.LimeGreen;
                break;

            case DeviceSecurityState.Unlocked:
                BootloaderDisplay = "Bootloader: Unlocked";
                BootloaderStatusColor = Brushes.Red;
                break;

            default:
                BootloaderDisplay = "Bootloader: Unknown";
                BootloaderStatusColor = Brushes.Gray;
                break;
        }
    }

    private void SetDeviceUnavailable()
    {
        DeviceModelDisplay = "Phone Model: —";
        DeviceSerialDisplay = "Serial Number: —";
        DeviceCodenameDisplay = "Codename: —";
        AndroidVersionDisplay = "Device: —";

        RootDisplay = "Root: —";
        BootloaderDisplay = "Bootloader: —";

        RootStatusColor = Brushes.Gray;
        BootloaderStatusColor = Brushes.Gray;

        RomDisplay = "ROM: —";
        RomBuildDisplay = "ROM Build: —";
    }

    private void SetDeviceUnknown()
    {
        DeviceModelDisplay = "Phone Model: Unknown";
        DeviceSerialDisplay = "Serial Number: Unknown";
        DeviceCodenameDisplay = "Codename: Unknown";
        AndroidVersionDisplay = "Device: Unknown";

        RootDisplay = "Root: Unknown";
        BootloaderDisplay = "Bootloader: Unknown";

        RootStatusColor = Brushes.Gray;
        BootloaderStatusColor = Brushes.Gray;

        RomDisplay = "ROM: Unknown";
        RomBuildDisplay = "ROM Build: Unknown";
    }

    private static string GetRomName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Unknown";

        if (value.Contains(
                "Project_Infinity",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Project Infinity-X";
        }

        if (value.Contains(
                "Infinity-X",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Project Infinity-X";
        }

        return value.Trim();
    }

    private static string FormatCodename(string value)
    {
        if (value.Equals(
                "onyx",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Onyx Global (Xiaomi)";
        }

        return value.Trim();
    }

    /*
     * Android version is automatically detected from:
     *
     * ro.build.version.release
     *
     * This method only translates known Android version numbers
     * into their human-readable release names.
     *
     * If a future Android version is not mapped yet, the actual
     * version number remains correct and only the codename becomes
     * "Unknown".
     */
    private static string GetAndroidCodename(string version)
    {
        return version.Trim() switch
        {
            "15" => "Vanilla Ice Cream",
            "16" => "Baklava",
            _ => "Unknown"
        };
    }

    private async void OnDeviceMonitorTick(
        object? sender,
        EventArgs e)
    {
        await RefreshAsync();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _deviceMonitorTimer.Stop();
        _deviceMonitorTimer.Tick -= OnDeviceMonitorTick;

        _refreshLock.Dispose();
    }
}