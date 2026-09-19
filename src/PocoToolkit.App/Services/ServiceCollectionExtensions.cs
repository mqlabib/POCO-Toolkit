using Microsoft.Extensions.DependencyInjection;

using PocoToolkit.App.ViewModels;

using PocoToolkit.Contracts.Abstractions;

using PocoToolkit.Engine;
using PocoToolkit.Engine.Adb;
using PocoToolkit.Engine.Fastboot;

namespace PocoToolkit.App.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPocoToolkit(
        this IServiceCollection services)
    {
        services.AddSingleton<IAdbRuntime, AdbRuntime>();

        services.AddSingleton<IDeviceDiscovery, DeviceDiscovery>();

        services.AddSingleton<IFastbootRuntime, FastbootRuntime>();

        services.AddSingleton<ToolkitEngine>();

        services.AddTransient<MainViewModel>();

        return services;
    }
}