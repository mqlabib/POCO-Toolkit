using PocoToolkit.Contracts.Models;

namespace PocoToolkit.Contracts.Abstractions;

public interface IAdbService
{
Task<bool> IsInstalledAsync();

Task<string?> GetVersionAsync();

string? GetPath();

Task<bool> IsDeviceConnectedAsync();

Task<DeviceInfo?> GetDeviceInfoAsync();

}
