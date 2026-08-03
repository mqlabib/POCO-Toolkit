using PocoToolkit.Contracts.Enums;

namespace PocoToolkit.Contracts.Abstractions;

public interface IToolbox
{
    string Name { get; }

    ToolboxType Type { get; }

    string Description { get; }
}