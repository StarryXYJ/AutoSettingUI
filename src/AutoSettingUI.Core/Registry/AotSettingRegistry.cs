using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Models;

namespace AutoSettingUI.Core.Registry;

public static class AotSettingRegistry
{
    private static readonly List<ISettingDescriptorProvider> _providers = [];
    private static readonly List<IPropertyValueAccessor> _accessors = [];

    private static ISettingDescriptorProvider? _legacyProvider;
    private static IPropertyValueAccessor? _legacyAccessor;

    public static ISettingDescriptorProvider? Provider
    {
        get => _legacyProvider ?? CreateCompositeProvider();
        set
        {
            _legacyProvider = value;
            if (value != null && !_providers.Contains(value))
            {
                _providers.Insert(0, value);
            }
        }
    }

    public static IPropertyValueAccessor? Accessor
    {
        get => _legacyAccessor ?? CreateCompositeAccessor();
        set
        {
            _legacyAccessor = value;
            if (value != null && !_accessors.Contains(value))
            {
                _accessors.Insert(0, value);
            }
        }
    }

    public static void RegisterProvider(ISettingDescriptorProvider provider, IPropertyValueAccessor? accessor = null)
    {
        if (provider == null) return;

        if (!_providers.Contains(provider))
        {
            _providers.Add(provider);
        }

        if (accessor != null && !_accessors.Contains(accessor))
        {
            _accessors.Add(accessor);
        }
        else if (provider is IPropertyValueAccessor propAccessor && !_accessors.Contains(propAccessor))
        {
            _accessors.Add(propAccessor);
        }

        _legacyProvider = null;
        _legacyAccessor = null;
    }

    public static void UnregisterProvider(ISettingDescriptorProvider provider)
    {
        _providers.Remove(provider);
        if (provider is IPropertyValueAccessor accessor)
        {
            _accessors.Remove(accessor);
        }
        _legacyProvider = null;
        _legacyAccessor = null;
    }

    public static IReadOnlyList<ISettingDescriptorProvider> GetAllProviders() => _providers.AsReadOnly();

    public static void Clear()
    {
        _providers.Clear();
        _accessors.Clear();
        _legacyProvider = null;
        _legacyAccessor = null;
    }

    private static ISettingDescriptorProvider? CreateCompositeProvider()
    {
        if (_providers.Count == 0) return null;
        if (_providers.Count == 1) return _providers[0];
        return new CompositeSettingDescriptorProvider(_providers);
    }

    private static IPropertyValueAccessor? CreateCompositeAccessor()
    {
        if (_accessors.Count == 0) return null;
        if (_accessors.Count == 1) return _accessors[0];
        return new CompositePropertyValueAccessor(_accessors);
    }

    private sealed class CompositeSettingDescriptorProvider : ISettingDescriptorProvider
    {
        private readonly IReadOnlyList<ISettingDescriptorProvider> _providers;

        public CompositeSettingDescriptorProvider(IReadOnlyList<ISettingDescriptorProvider> providers)
        {
            _providers = providers;
        }

        public SettingClassDescriptor? GetDescriptor(Type type)
        {
            foreach (var provider in _providers)
            {
                var descriptor = provider.GetDescriptor(type);
                if (descriptor != null) return descriptor;
            }
            return null;
        }

        public SettingClassDescriptor? GetDescriptor(string typeName)
        {
            foreach (var provider in _providers)
            {
                var descriptor = provider.GetDescriptor(typeName);
                if (descriptor != null) return descriptor;
            }
            return null;
        }

        public IReadOnlyList<SettingClassDescriptor> GetAllDescriptors()
        {
            var result = new List<SettingClassDescriptor>();
            foreach (var provider in _providers)
            {
                result.AddRange(provider.GetAllDescriptors());
            }
            return result;
        }

        public bool HasDescriptor(Type type)
        {
            foreach (var provider in _providers)
            {
                if (provider.HasDescriptor(type)) return true;
            }
            return false;
        }

        public bool HasDescriptor(string typeName)
        {
            foreach (var provider in _providers)
            {
                if (provider.HasDescriptor(typeName)) return true;
            }
            return false;
        }
    }

    private sealed class CompositePropertyValueAccessor : IPropertyValueAccessor
    {
        private readonly IReadOnlyList<IPropertyValueAccessor> _accessors;

        public CompositePropertyValueAccessor(IReadOnlyList<IPropertyValueAccessor> accessors)
        {
            _accessors = accessors;
        }

        public object? GetValue(object target, string propertyName)
        {
            foreach (var accessor in _accessors)
            {
                var value = accessor.GetValue(target, propertyName);
                if (value != null) return value;
            }
            return null;
        }

        public void SetValue(object target, string propertyName, object? value)
        {
            foreach (var accessor in _accessors)
            {
                accessor.SetValue(target, propertyName, value);
            }
        }

        public object? GetEnumValue(string typeName, string value)
        {
            foreach (var accessor in _accessors)
            {
                var result = accessor.GetEnumValue(typeName, value);
                if (result != null) return result;
            }
            return null;
        }
    }
}
