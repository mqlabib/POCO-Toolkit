namespace PocoToolkit.Contracts.Models;

public class DeviceInfo
{
    public string Model { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string AndroidVersion { get; set; } = "";
    public string HyperOSVersion { get; set; } = "";
    public bool IsRooted { get; set; }
    public bool BootloaderUnlocked { get; set; }
}