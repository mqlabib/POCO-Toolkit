using PocoToolkit.Contracts.Enums;
using PocoToolkit.Contracts.Models;

namespace PocoToolkit.Contracts.Abstractions;

public interface IDeviceDiscovery
{
Task<DeviceConnectionState> GetConnectionStateAsync();

Task<DeviceInfo?> GetDeviceInfoAsync();

}
