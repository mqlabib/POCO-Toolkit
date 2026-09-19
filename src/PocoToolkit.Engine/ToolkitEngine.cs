using PocoToolkit.Contracts.Abstractions;
using PocoToolkit.Contracts.Enums;
using PocoToolkit.Contracts.Models;
using PocoToolkit.Engine.Adb;
using PocoToolkit.Engine.Fastboot;

namespace PocoToolkit.Engine;

public sealed class ToolkitEngine
{
    private readonly IAdbRuntime _adbRuntime;
    private readonly IDeviceDiscovery _deviceDiscovery;
    private readonly IFastbootRuntime _fastbootRuntime;

    public ToolkitEngine(
        IAdbRuntime adbRuntime,
        IDeviceDiscovery deviceDiscovery,
        IFastbootRuntime fastbootRuntime)
    {
        _adbRuntime = adbRuntime;
        _deviceDiscovery = deviceDiscovery;
        _fastbootRuntime = fastbootRuntime;
    }

    public Task<bool> IsAdbInstalledAsync()
    {
        return _adbRuntime.IsInstalledAsync();
    }

    public async Task<string> GetAdbVersionAsync()
    {
        var version = await _adbRuntime.GetVersionAsync();

        return string.IsNullOrWhiteSpace(version)
            ? "Unknown"
            : version.Trim();
    }

    public string GetAdbPath()
    {
        var path = _adbRuntime.GetAdbPath();

        return string.IsNullOrWhiteSpace(path)
            ? "Unknown"
            : path.Trim();
    }

    public Task<DeviceConnectionState> GetDeviceConnectionStateAsync()
    {
        return _deviceDiscovery.GetConnectionStateAsync();
    }

    public Task<DeviceInfo?> GetDeviceInfoAsync()
    {
        return _deviceDiscovery.GetDeviceInfoAsync();
    }

    public Task<string> ExecuteFastbootAsync(string arguments)
    {
        return _fastbootRuntime.ExecuteAsync(arguments);
    }
}