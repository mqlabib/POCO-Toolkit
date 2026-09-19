using PocoToolkit.Contracts.Abstractions;
using PocoToolkit.Contracts.Models;

namespace PocoToolkit.Engine.Adb;

public sealed class AdbService : IAdbService
{
private readonly IAdbRuntime _runtime;

public AdbService(IAdbRuntime runtime)
{
    _runtime = runtime;
}

public Task<bool> IsInstalledAsync()
    => _runtime.IsInstalledAsync();

public Task<string?> GetVersionAsync()
    => _runtime.GetVersionAsync();

public string? GetPath()
    => _runtime.GetAdbPath();

public async Task<bool> IsDeviceConnectedAsync()
{
    var output = await _runtime.ExecuteAsync("devices");

    if (string.IsNullOrWhiteSpace(output))
        return false;

    foreach (var line in output.Split(
                 Environment.NewLine,
                 StringSplitOptions.RemoveEmptyEntries))
    {
        var trimmed = line.Trim();

        if (trimmed.EndsWith("\tdevice"))
            return true;
    }

    return false;
}

public async Task<DeviceInfo?> GetDeviceInfoAsync()
{
    var devicesOutput = await _runtime.ExecuteAsync("devices");

    if (string.IsNullOrWhiteSpace(devicesOutput))
        return null;

    string? serial = null;

    foreach (var line in devicesOutput.Split(
                 Environment.NewLine,
                 StringSplitOptions.RemoveEmptyEntries))
    {
        var trimmed = line.Trim();

        if (trimmed.EndsWith("\tdevice"))
        {
            var parts = trimmed.Split('\t');

            if (parts.Length >= 2)
            {
                serial = parts[0].Trim();
                break;
            }
        }
    }

    if (string.IsNullOrWhiteSpace(serial))
        return null;

    var model = await _runtime.ExecuteAsync(
        $"-s {serial} shell getprop ro.product.marketname");

    var modelNumber = await _runtime.ExecuteAsync(
        $"-s {serial} shell getprop ro.product.model");

    if (string.IsNullOrWhiteSpace(model))
        model = "Unknown";

    if (string.IsNullOrWhiteSpace(modelNumber))
        modelNumber = "Unknown";

    return new DeviceInfo
    {
        SerialNumber = serial,
        Model = model.Trim(),
        ModelNumber = modelNumber.Trim()
    };
}

}
