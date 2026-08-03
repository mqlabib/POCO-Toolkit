using PocoToolkit.Contracts.Enums;

namespace PocoToolkit.Contracts.Abstractions;

public interface IDepartment
{
    string Name { get; }

    DepartmentType Type { get; }

    string Description { get; }
}