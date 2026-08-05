using PocoToolkit.Contracts.Abstractions;

namespace PocoToolkit.Engine;

public sealed class ToolkitEngine
{
    private readonly IAdbService _adb;

    public ToolkitEngine(IAdbService adb)
    {
        _adb = adb;
    }

    public Task<bool> IsAdbInstalledAsync()
        => _adb.IsInstalledAsync();

    public Task<string?> GetAdbVersionAsync()
        => _adb.GetVersionAsync();

    public string? GetAdbPath()
        => _adb.GetPath();

    public Task<bool> IsDeviceConnectedAsync()
        => _adb.IsDeviceConnectedAsync();
}