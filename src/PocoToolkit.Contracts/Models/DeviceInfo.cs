using PocoToolkit.Contracts.Enums;

namespace PocoToolkit.Contracts.Models;

public sealed class DeviceInfo
{
    public string SerialNumber { get; set; } = "";

    public string Model { get; set; } = "";

    public string ModelNumber { get; set; } = "";

    public string Manufacturer { get; set; } = "";

    public string DeviceCodename { get; set; } = "";

    public string AndroidVersion { get; set; } = "";

    public string RomName { get; set; } = "";

    public string RomVersion { get; set; } = "";

    public string RomBuild { get; set; } = "";

    public string RomStatus { get; set; } = "";

    public DeviceSecurityState RootState { get; set; }
        = DeviceSecurityState.Unknown;

    public DeviceSecurityState BootloaderState { get; set; }
        = DeviceSecurityState.Unknown;
}