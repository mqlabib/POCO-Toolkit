using System;
using System.Threading.Tasks;
using PocoToolkit.Contracts.Abstractions;
using PocoToolkit.Contracts.Enums;
using PocoToolkit.Contracts.Models;

namespace PocoToolkit.Engine.Adb;

public sealed class DeviceDiscovery : IDeviceDiscovery
{
    private readonly IAdbRuntime _runtime;

    public DeviceDiscovery(IAdbRuntime runtime)
    {
        _runtime = runtime;
    }

    public async Task<DeviceConnectionState> GetConnectionStateAsync()
    {
        var output = await _runtime.ExecuteAsync("devices");

        if (string.IsNullOrWhiteSpace(output))
            return DeviceConnectionState.NoDevice;

        foreach (var line in output.Split(
                     Environment.NewLine,
                     StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith(
                    "List of devices attached",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = trimmed.Split(
                '\t',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                continue;

            return parts[1].Trim().ToLowerInvariant() switch
            {
                "device" => DeviceConnectionState.Connected,
                "unauthorized" => DeviceConnectionState.Unauthorized,
                "offline" => DeviceConnectionState.Offline,
                _ => DeviceConnectionState.NoDevice
            };
        }

        return DeviceConnectionState.NoDevice;
    }

    public async Task<DeviceInfo?> GetDeviceInfoAsync()
    {
        var state = await GetConnectionStateAsync();

        if (state != DeviceConnectionState.Connected)
            return null;

        var output = await _runtime.ExecuteAsync("devices");

        var serial = GetConnectedSerial(output);

        if (string.IsNullOrWhiteSpace(serial))
            return null;

        var model = await GetPropertyAsync(
            serial,
            "ro.product.marketname");

        var modelNumber = await GetPropertyAsync(
            serial,
            "ro.product.model");

        var manufacturer = await GetPropertyAsync(
            serial,
            "ro.product.manufacturer");

        var deviceCodename = await GetPropertyAsync(
            serial,
            "ro.product.device");

        var androidVersion = await GetPropertyAsync(
            serial,
            "ro.build.version.release");

        /*
         * Xiaomi / HyperOS identification.
         */
        var miuiVersion = await GetPropertyAsync(
            serial,
            "ro.miui.ui.version.name");

        var miuiIncremental = await GetPropertyAsync(
            serial,
            "ro.build.version.incremental");

        /*
         * Generic custom ROM / AOSP identification.
         */
        var romModVersion = await GetPropertyAsync(
            serial,
            "ro.modversion");

        var buildDisplayId = await GetPropertyAsync(
            serial,
            "ro.build.display.id");

        var buildId = await GetPropertyAsync(
            serial,
            "ro.build.id");

        var baseOs = await GetPropertyAsync(
            serial,
            "ro.build.version.base_os");

        /*
         * Infinity-X properties.
         */
        var infinityVersion = await GetPropertyAsync(
            serial,
            "ro.infinity.version");

        var infinityBuild = await GetPropertyAsync(
            serial,
            "ro.infinity.build.version");

        var infinityStatus = await GetPropertyAsync(
            serial,
            "ro.infinity.build.status");

        var rootState = await DetectRootStateAsync(serial);

        var bootloaderState =
            await DetectBootloaderStateAsync(serial);

        var romInfo = DetectRom(
            miuiVersion,
            miuiIncremental,
            romModVersion,
            buildDisplayId,
            buildId,
            baseOs,
            infinityVersion,
            infinityBuild,
            infinityStatus);

        return new DeviceInfo
        {
            SerialNumber = serial,

            Model = CleanValue(model),

            ModelNumber = CleanValue(modelNumber),

            Manufacturer = CleanValue(manufacturer),

            DeviceCodename = CleanValue(deviceCodename),

            AndroidVersion = CleanValue(androidVersion),

            RomName = romInfo.Name,

            RomVersion = romInfo.Version,

            RomBuild = romInfo.Build,

            RomStatus = romInfo.Status,

            RootState = rootState,

            BootloaderState = bootloaderState
        };
    }

    private async Task<string?> GetPropertyAsync(
        string serial,
        string property)
    {
        var output = await _runtime.ExecuteAsync(
            $"-s {serial} shell getprop {property}");

        return string.IsNullOrWhiteSpace(output)
            ? null
            : output.Trim();
    }

    private async Task<DeviceSecurityState> DetectRootStateAsync(
        string serial)
    {
        var output = await _runtime.ExecuteAsync(
            $"-s {serial} shell su -c id");

        if (string.IsNullOrWhiteSpace(output))
            return DeviceSecurityState.Unknown;

        if (output.Contains(
                "uid=0",
                StringComparison.OrdinalIgnoreCase))
        {
            return DeviceSecurityState.Unlocked;
        }

        if (output.Contains(
                "not found",
                StringComparison.OrdinalIgnoreCase)
            ||
            output.Contains(
                "inaccessible",
                StringComparison.OrdinalIgnoreCase)
            ||
            output.Contains(
                "permission denied",
                StringComparison.OrdinalIgnoreCase))
        {
            return DeviceSecurityState.Locked;
        }

        return DeviceSecurityState.Unknown;
    }

    private async Task<DeviceSecurityState> DetectBootloaderStateAsync(
        string serial)
    {
        /*
         * IMPORTANT:
         *
         * Do not use fastboot from the normal ADB polling loop.
         *
         * Android-side bootloader properties can be spoofed by
         * custom ROMs. Hardware-level confirmation belongs to a
         * dedicated Fastboot check.
         */

        var verifiedBootState = await GetPropertyAsync(
            serial,
            "ro.boot.verifiedbootstate");

        var flashLocked = await GetPropertyAsync(
            serial,
            "ro.boot.flash.locked");

        var vbmetaDeviceState = await GetPropertyAsync(
            serial,
            "ro.boot.vbmeta.device_state");

        if (string.Equals(
                flashLocked,
                "0",
                StringComparison.OrdinalIgnoreCase)
            ||
            string.Equals(
                vbmetaDeviceState,
                "unlocked",
                StringComparison.OrdinalIgnoreCase)
            ||
            string.Equals(
                verifiedBootState,
                "orange",
                StringComparison.OrdinalIgnoreCase))
        {
            return DeviceSecurityState.Unlocked;
        }

        if (string.Equals(
                flashLocked,
                "1",
                StringComparison.OrdinalIgnoreCase)
            &&
            string.Equals(
                vbmetaDeviceState,
                "locked",
                StringComparison.OrdinalIgnoreCase)
            &&
            string.Equals(
                verifiedBootState,
                "green",
                StringComparison.OrdinalIgnoreCase))
        {
            return DeviceSecurityState.Locked;
        }

        return DeviceSecurityState.Unknown;
    }

    private static (
        string Name,
        string Version,
        string Build,
        string Status)
        DetectRom(
            string? miuiVersion,
            string? miuiIncremental,
            string? romModVersion,
            string? buildDisplayId,
            string? buildId,
            string? baseOs,
            string? infinityVersion,
            string? infinityBuild,
            string? infinityStatus)
    {
        /*
         * 1. Infinity-X
         *
         * Check this before generic custom-ROM detection.
         */
        if (!string.IsNullOrWhiteSpace(infinityVersion)
            ||
            !string.IsNullOrWhiteSpace(infinityBuild)
            ||
            !string.IsNullOrWhiteSpace(infinityStatus))
        {
            return (
                "Project Infinity-X",
                CleanValue(infinityVersion),
                CleanValue(infinityBuild),
                CleanValue(infinityStatus));
        }

        /*
         * 2. HyperOS / MIUI
         *
         * Xiaomi exposes ro.miui.ui.version.name on
         * HyperOS/MIUI builds.
         */
        if (!string.IsNullOrWhiteSpace(miuiVersion))
        {
            var version = miuiVersion.Trim();

            var build = !string.IsNullOrWhiteSpace(miuiIncremental)
                ? miuiIncremental.Trim()
                : CleanValue(buildDisplayId);

            return (
                "Xiaomi HyperOS",
                version,
                build,
                "Official Xiaomi");
        }

        /*
         * 3. Generic custom ROM.
         *
         * Many AOSP-based ROMs expose ro.modversion.
         */
        if (!string.IsNullOrWhiteSpace(romModVersion))
        {
            var modVersion = romModVersion.Trim();

            return (
                ExtractRomName(modVersion),
                ExtractRomVersion(modVersion),
                CleanValue(buildDisplayId ?? buildId),
                "Custom ROM");
        }

        /*
         * 4. Generic Android / AOSP fallback.
         */
        if (!string.IsNullOrWhiteSpace(buildDisplayId))
        {
            return (
                "Android / AOSP",
                "Unknown",
                buildDisplayId.Trim(),
                "Detected");
        }

        if (!string.IsNullOrWhiteSpace(buildId))
        {
            return (
                "Android / AOSP",
                "Unknown",
                buildId.Trim(),
                "Detected");
        }

        /*
         * 5. Last fallback.
         */
        if (!string.IsNullOrWhiteSpace(baseOs))
        {
            return (
                "Android",
                "Unknown",
                baseOs.Trim(),
                "Detected");
        }

        return (
            "Unknown",
            "Unknown",
            "Unknown",
            "Unknown");
    }

    private static string ExtractRomName(string value)
    {
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

        /*
         * Keep the complete custom-ROM property if we don't
         * recognize the ROM. This is safer than falsely naming it.
         */
        var separator = value.IndexOf(
            '-',
            StringComparison.Ordinal);

        if (separator > 0)
            return value[..separator].Trim();

        return value;
    }

    private static string ExtractRomVersion(string value)
    {
        if (value.Contains(
                "Infinity-X",
                StringComparison.OrdinalIgnoreCase))
        {
            var marker = "Infinity-X-";

            var index = value.IndexOf(
                marker,
                StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
            {
                var versionStart = index + marker.Length;

                var remainder = value[versionStart..];

                var separator = remainder.IndexOf(
                    '-',
                    StringComparison.Ordinal);

                if (separator > 0)
                    return remainder[..separator].Trim();

                return remainder.Trim();
            }
        }

        return "Unknown";
    }

    private static string? GetConnectedSerial(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return null;

        foreach (var line in output.Split(
                     Environment.NewLine,
                     StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith(
                    "List of devices attached",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = trimmed.Split(
                '\t',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2 &&
                parts[1].Trim().Equals(
                    "device",
                    StringComparison.OrdinalIgnoreCase))
            {
                return parts[0].Trim();
            }
        }

        return null;
    }

    private static string CleanValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "Unknown"
            : value.Trim();
    }
}