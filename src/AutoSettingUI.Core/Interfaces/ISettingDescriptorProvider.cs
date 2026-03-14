using AutoSettingUI.Core.Models;

namespace AutoSettingUI.Core.Interfaces;

/// <summary>
/// Provides setting class descriptors for types marked with [SettingUI].
/// This is implemented by the source generator.
/// </summary>
public interface ISettingDescriptorProvider
{
    /// <summary>
    /// Gets the descriptor for a specific type.
    /// </summary>
    /// <param name="type">The type to get descriptor for.</param>
    /// <returns>The descriptor, or null if the type is not marked with [SettingUI].</returns>
    SettingClassDescriptor? GetDescriptor(Type type);

    /// <summary>
    /// Gets the descriptor for a type by its full name.
    /// </summary>
    /// <param name="typeName">The full type name.</param>
    /// <returns>The descriptor, or null if not found.</returns>
    SettingClassDescriptor? GetDescriptor(string typeName);

    /// <summary>
    /// Gets all registered setting class descriptors.
    /// </summary>
    IReadOnlyList<SettingClassDescriptor> GetAllDescriptors();

    /// <summary>
    /// Checks if a type has a registered descriptor.
    /// </summary>
    bool HasDescriptor(Type type);

    /// <summary>
    /// Checks if a type name has a registered descriptor.
    /// </summary>
    bool HasDescriptor(string typeName);
}
