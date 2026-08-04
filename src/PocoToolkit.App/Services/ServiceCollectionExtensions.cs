using Microsoft.Extensions.DependencyInjection;
using PocoToolkit.Contracts.Abstractions;
using PocoToolkit.Engine;
using PocoToolkit.Engine.Adb;
using PocoToolkit.App.ViewModels;

namespace PocoToolkit.App.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPocoToolkit(this IServiceCollection services)
    {
        services.AddSingleton<IAdbRuntime, AdbRuntime>();
        services.AddSingleton<IAdbService, AdbService>();

        services.AddSingleton<ToolkitEngine>();

        services.AddSingleton<MainViewModel>();

        return services;
    }
}