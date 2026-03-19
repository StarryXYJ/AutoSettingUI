using System;
using System.Collections.Generic;
using System.Linq;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Generator.Incremental;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace AutoSettingUI.Generator.Tests;

public sealed class GeneratedSettingProviderTests
{
    [Fact]
    public void GeneratesNullMainHeaderWhenMissing()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int Value { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContainsInOrder(
            generated,
            "SettingClassDescriptor(",
            "\"TestNamespace.TestSettings\"",
            "\"TestSettings\"",
            "null");
    }

    [Fact]
    public void GeneratesMainHeaderFromAttribute()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
[MainHeader(""General Settings"")]
public class TestSettings
{
    public int Value { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "\"TestNamespace.TestSettings\"");
        AssertContains(generated, "\"TestSettings\"");
        AssertContains(generated, "General Settings");
    }

    [Fact]
    public void GeneratesPropertyWithCustomTitle()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Title(""Custom Name"")]
    public int Value { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "Custom Name");
        AssertContains(generated, "\"Value\"");
    }

    [Fact]
    public void GeneratesPropertyWithRange()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Range(0, 100)]
    public int Percentage { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "true,");
        AssertContains(generated, "0,");
        AssertContains(generated, "100,");
    }

    [Fact]
    public void GeneratesPropertyWithDescription()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Description(""This is a description"")]
    public int Value { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "This is a description");
    }

    [Fact]
    public void GeneratesPropertyWithPlaceholder()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Placeholder(""Enter value..."")]
    public string Name { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "Enter value...");
    }

    [Fact]
    public void GeneratesPasswordProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Password]
    public string SecretKey { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "true,");
        AssertContains(generated, "'•'");
    }

    [Fact]
    public void GeneratesPasswordPropertyWithCustomMaskChar()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Password('*')]
    public string SecretKey { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "'*'");
    }

    [Fact]
    public void GeneratesReadOnlyProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [ReadOnly(true)]
    public int ReadOnlyValue { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "new PropertyDescriptor(");
        AssertContains(generated, "\"ReadOnlyValue\"");
    }

    [Fact]
    public void GeneratesReadOnlyPropertyWithMethodName()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [ReadOnly(""CheckIfReadOnly"")]
    public int DynamicReadOnlyValue { get; set; }

    public bool CheckIfReadOnly() => false;
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "CheckIfReadOnly");
    }

    [Fact]
    public void HidesPropertyWithHideAttribute()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int VisibleValue { get; set; }

    [Hide]
    public int HiddenValue { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "\"VisibleValue\"");
        AssertDoesNotContain(generated, "pd_TestSettings_HiddenValue");
    }

    [Fact]
    public void GeneratesEnumPropertyWithValues()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

public enum TestEnum
{
    Option1,
    Option2,
    Option3
}

[SettingUI]
public class TestSettings
{
    public TestEnum SelectedOption { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "Option1");
        AssertContains(generated, "Option2");
        AssertContains(generated, "Option3");
    }

    [Fact]
    public void GeneratesItemsSourceProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;
using System.Collections.Generic;

namespace TestNamespace;

public static class ItemProvider
{
    public static List<string> GetItems() => new();
}

[SettingUI]
public class TestSettings
{
    [ItemsSource(typeof(ItemProvider), ""GetItems"")]
    public string SelectedItem { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "TestNamespace.ItemProvider");
        AssertContains(generated, "GetItems");
    }

    [Fact]
    public void GeneratesControlBindingProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

public class CustomControl { }

[SettingUI]
public class TestSettings
{
    [ControlBinding(typeof(CustomControl), ""ValueProperty"", ""Create"")]
    public int CustomBoundValue { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "TestNamespace.CustomControl");
        AssertContains(generated, "ValueProperty");
        AssertContains(generated, "Create");
    }

    [Fact]
    public void GeneratesSubHeader()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int FirstValue { get; set; }

    [SubHeader(""Advanced Options"")]
    public int AdvancedValue { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "SubSectionInfo");
        AssertContains(generated, "Advanced Options");
    }

    [Fact]
    public void GeneratesCollectionProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;
using System.Collections.Generic;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public List<string> Items { get; set; } = new();
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "\"Items\"");
    }

    [Fact]
    public void GeneratesCollectionEditorProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;
using System.Collections.Generic;

namespace TestNamespace;

public class CustomEditor { }

[SettingUI]
public class TestSettings
{
    [CollectionEditor(typeof(CustomEditor), AllowAdd = false, AllowRemove = false)]
    public List<string> Items { get; set; } = new();
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "TestNamespace.CustomEditor");
        AssertContains(generated, "false,");
    }

    [Fact]
    public void GeneratesDelegateProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public System.Action SaveCommand { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "\"SaveCommand\"");
    }

    [Fact]
    public void GeneratesDelegatePropertyWithCanExecute()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [CommandCanExecute(""CanSave"")]
    public System.Action SaveCommand { get; set; }

    public bool CanSave() => true;
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "CanSave");
    }

    [Fact]
    public void GeneratesMultipleClasses()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class FirstSettings
{
    public int Value1 { get; set; }
}

[SettingUI]
public class SecondSettings
{
    public string Value2 { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "\"TestNamespace.FirstSettings\"");
        AssertContains(generated, "\"TestNamespace.SecondSettings\"");
        AssertContains(generated, "Register_FirstSettings()");
        AssertContains(generated, "Register_SecondSettings()");
    }

    [Fact]
    public void GeneratesGetValueMethod()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int IntValue { get; set; }
    public string StringValue { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "public object? GetValue(object target, string propertyName)");
        AssertContains(generated, "case \"IntValue\": return t_TestSettings.IntValue;");
        AssertContains(generated, "case \"StringValue\": return t_TestSettings.StringValue;");
    }

    [Fact]
    public void GeneratesSetValueMethod()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int IntValue { get; set; }
    public string StringValue { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "public void SetValue(object target, string propertyName, object? value)");
        AssertContains(generated, "case \"IntValue\": s_TestSettings.IntValue = (int)value!; return;");
        AssertContains(generated, "case \"StringValue\": s_TestSettings.StringValue = (string)value!; return;");
    }

    [Fact]
    public void GeneratesGetEnumValueMethod()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

public enum Status
{
    Active,
    Inactive
}

[SettingUI]
public class TestSettings
{
    public Status CurrentStatus { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "public object? GetEnumValue(string typeName, string value)");
        AssertContains(generated, "TestNamespace.Status");
        AssertContains(generated, "Enum.Parse");
    }

    [Fact]
    public void GeneratesSettingUIWithControlFactory()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

public class MyFactory { }

[SettingUI(ControlFactory = typeof(MyFactory), FactoryMethod = ""CreateControl"")]
public class TestSettings
{
    public int Value { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "TestNamespace.MyFactory");
        AssertContains(generated, "CreateControl");
    }

    [Fact]
    public void GeneratesModuleInitializer()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int Value { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "[ModuleInitializer]");
        AssertContains(generated, "public static void Initialize()");
        AssertContains(generated, "AotSettingRegistry.Provider = instance;");
        AssertContains(generated, "AotSettingRegistry.Accessor = instance;");
    }

    [Fact]
    public void GeneratesCorrectNamespace()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int Value { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "namespace AutoSettingUI.Generated");
    }

    [Fact]
    public void GeneratesCorrectInterfaces()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int Value { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "public sealed class GeneratedSettingProvider : ISettingDescriptorProvider, IPropertyValueAccessor");
    }

    [Fact]
    public void GeneratesGetDescriptorMethods()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int Value { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "public SettingClassDescriptor? GetDescriptor(Type type)");
        AssertContains(generated, "public SettingClassDescriptor? GetDescriptor(string typeName)");
    }

    [Fact]
    public void GeneratesGetAllDescriptorsMethod()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int Value { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "public IReadOnlyList<SettingClassDescriptor> GetAllDescriptors()");
    }

    [Fact]
    public void GeneratesHasDescriptorMethods()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int Value { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "public bool HasDescriptor(Type type)");
        AssertContains(generated, "public bool HasDescriptor(string typeName)");
    }

    [Fact]
    public void GeneratesForNestedNamespace()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace.Nested.Deep;

[SettingUI]
public class TestSettings
{
    public int Value { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "\"TestNamespace.Nested.Deep.TestSettings\"");
    }

    [Fact]
    public void GeneratesForGenericPropertyTypes()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;
using System.Collections.Generic;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public Dictionary<string, int> Lookup { get; set; } = new();
    public List<int> Numbers { get; set; } = new();
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "\"Lookup\"");
        AssertContains(generated, "\"Numbers\"");
    }

    [Fact]
    public void GeneratesForNullablePropertyTypes()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int? NullableInt { get; set; }
    public string? NullableString { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "\"NullableInt\"");
        AssertContains(generated, "\"NullableString\"");
    }

    [Fact]
    public void SkipsPropertyWithoutPublicSetter()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int ReadOnlyProperty { get; }
    public int ReadWriteProperty { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "\"ReadOnlyProperty\"");
        AssertContains(generated, "\"ReadWriteProperty\"");
        
        Assert.DoesNotContain("case \"ReadOnlyProperty\": s_TestSettings.ReadOnlyProperty", generated);
        AssertContains(generated, "case \"ReadWriteProperty\": s_TestSettings.ReadWriteProperty");
    }

    [Fact]
    public void GeneratesForClassWithNoProperties()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class EmptySettings
{
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "\"TestNamespace.EmptySettings\"");
        AssertContains(generated, "Register_EmptySettings()");
    }

    [Fact]
    public void GeneratesCorrectPropertyTypeNames()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    public int IntValue { get; set; }
    public string StringValue { get; set; }
    public double DoubleValue { get; set; }
    public bool BoolValue { get; set; }
}
";

        var generated = RunGenerator(source);

        AssertContains(generated, "\"IntValue\"");
        AssertContains(generated, "\"StringValue\"");
        AssertContains(generated, "\"DoubleValue\"");
        AssertContains(generated, "\"BoolValue\"");
    }

    private static string RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var systemRuntimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .First(a => a.GetName().Name == "System.Runtime");

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(SettingUIAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(TitleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(MainHeaderAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(RangeAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DescriptionAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(PlaceholderAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(PasswordAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ReadOnlyAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(HideAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ItemsSourceAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ControlBindingAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(SubHeaderAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(CollectionEditorAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(CommandCanExecuteAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.GCSettings).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Action).Assembly.Location),
            MetadataReference.CreateFromFile(systemRuntimeAssembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "GeneratorTests",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new AutoSettingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var runResult = driver.GetRunResult();
        var generated = runResult.Results
            .SelectMany(r => r.GeneratedSources)
            .Single(s => s.HintName == "GeneratedSettingProvider.g.cs")
            .SourceText
            .ToString();

        return generated;
    }

    private static void AssertContainsInOrder(string text, params string[] parts)
    {
        var index = 0;
        foreach (var part in parts)
        {
            var next = text.IndexOf(part, index, StringComparison.Ordinal);
            Assert.True(next >= 0, $"Expected to find '{part}' after index {index}.");
            index = next + part.Length;
        }
    }

    private static void AssertContains(string text, string expected)
    {
        Assert.True(text.Contains(expected, StringComparison.Ordinal), 
            $"Expected to find '{expected}' in generated code.");
    }

    private static void AssertDoesNotContain(string text, string notExpected)
    {
        Assert.False(text.Contains(notExpected, StringComparison.Ordinal), 
            $"Expected NOT to find '{notExpected}' in generated code.");
    }
}
