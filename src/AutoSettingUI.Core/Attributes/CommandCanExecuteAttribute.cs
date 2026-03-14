namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies a method to determine whether a command (delegate property) can execute.
/// Use this attribute on delegate properties (Action, Func, etc.) to control the enabled state of the generated button.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class CommandCanExecuteAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the method that returns a boolean indicating whether the command can execute.
    /// The method must be parameterless and return bool.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandCanExecuteAttribute"/> class.
    /// </summary>
    /// <param name="methodName">The name of the method to call for CanExecute evaluation.</param>
    public CommandCanExecuteAttribute(string methodName)
    {
        MethodName = methodName;
    }
}
