namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Hides a property from the settings form.
/// Use this for internal properties that should not be exposed in the UI.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class HideAttribute : Attribute
{
}
