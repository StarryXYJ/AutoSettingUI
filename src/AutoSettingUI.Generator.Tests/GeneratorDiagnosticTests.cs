using System;
using System.Linq;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Generator.Incremental;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace AutoSettingUI.Generator.Tests;

public sealed class GeneratorDiagnosticTests
{
    [Fact]
    public void ReportsWarningForNonPublicClass()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
internal class InternalSettings
{
    public int Value { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.Contains(diagnostics, d => 
            d.Id == "ASUI001" && 
            d.Severity == DiagnosticSeverity.Warning &&
            d.GetMessage().Contains("InternalSettings"));
    }

    [Fact]
    public void ReportsWarningForPrivateClass()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

public class OuterClass
{
    [SettingUI]
    private class PrivateSettings
    {
        public int Value { get; set; }
    }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.Contains(diagnostics, d => 
            d.Id == "ASUI001" && 
            d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void ReportsWarningForNestedClass()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

public class OuterClass
{
    [SettingUI]
    public class NestedSettings
    {
        public int Value { get; set; }
    }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.Contains(diagnostics, d => 
            d.Id == "ASUI002" && 
            d.Severity == DiagnosticSeverity.Warning &&
            d.GetMessage().Contains("NestedSettings"));
    }

    [Fact]
    public void ReportsWarningForRangeOnNonNumericProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Range(0, 100)]
    public string NonNumericValue { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.Contains(diagnostics, d => 
            d.Id == "ASUI003" && 
            d.Severity == DiagnosticSeverity.Warning &&
            d.GetMessage().Contains("NonNumericValue"));
    }

    [Fact]
    public void ReportsWarningForRangeOnStringProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Range(1, 10)]
    public string TextValue { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.Contains(diagnostics, d => 
            d.Id == "ASUI003" && 
            d.GetMessage().Contains("string"));
    }

    [Fact]
    public void ReportsWarningForRangeOnBoolProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Range(0, 1)]
    public bool BoolValue { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.Contains(diagnostics, d => 
            d.Id == "ASUI003" && 
            d.GetMessage().Contains("BoolValue"));
    }

    [Fact]
    public void DoesNotReportWarningForRangeOnIntProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Range(0, 100)]
    public int IntValue { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "ASUI003");
    }

    [Fact]
    public void DoesNotReportWarningForRangeOnDoubleProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Range(0.0, 1.0)]
    public double DoubleValue { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "ASUI003");
    }

    [Fact]
    public void DoesNotReportWarningForRangeOnFloatProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Range(0f, 100f)]
    public float FloatValue { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "ASUI003");
    }

    [Fact]
    public void DoesNotReportWarningForRangeOnLongProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Range(0L, 1000000L)]
    public long LongValue { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "ASUI003");
    }

    [Fact]
    public void DoesNotReportWarningForRangeOnShortProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Range(0, 100)]
    public short ShortValue { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "ASUI003");
    }

    [Fact]
    public void DoesNotReportWarningForRangeOnByteProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Range(0, 255)]
    public byte ByteValue { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "ASUI003");
    }

    [Fact]
    public void DoesNotReportWarningForRangeOnDecimalProperty()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class TestSettings
{
    [Range(0m, 100m)]
    public decimal DecimalValue { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "ASUI003");
    }

    [Fact]
    public void DoesNotReportWarningForPublicClass()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class PublicSettings
{
    public int Value { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "ASUI001");
    }

    [Fact]
    public void ReportsMultipleWarningsForSameClass()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

public class OuterClass
{
    [SettingUI]
    internal class ProblematicSettings
    {
        [Range(0, 100)]
        public string BadProperty { get; set; }
    }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.Contains(diagnostics, d => d.Id == "ASUI001");
        Assert.Contains(diagnostics, d => d.Id == "ASUI002");
        Assert.Contains(diagnostics, d => d.Id == "ASUI003");
    }

    [Fact]
    public void ReportsInfoDiagnosticForClassCount()
    {
        var source = @"
using AutoSettingUI.Core.Attributes;

namespace TestNamespace;

[SettingUI]
public class FirstSettings
{
    public int Value { get; set; }
}

[SettingUI]
public class SecondSettings
{
    public string Name { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.Contains(diagnostics, d => 
            d.Id == "ASUI000" && 
            d.Severity == DiagnosticSeverity.Info &&
            d.GetMessage().Contains("2"));
    }

    [Fact]
    public void ReportsInfoDiagnosticForZeroClasses()
    {
        var source = @"
namespace TestNamespace;

public class NoSettingsAttribute
{
    public int Value { get; set; }
}
";

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.Contains(diagnostics, d => 
            d.Id == "ASUI000" && 
            d.Severity == DiagnosticSeverity.Info &&
            d.GetMessage().Contains("0"));
    }

    private static Diagnostic[] RunGeneratorAndGetDiagnostics(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var systemRuntimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .First(a => a.GetName().Name == "System.Runtime");

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(SettingUIAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.GCSettings).Assembly.Location),
            MetadataReference.CreateFromFile(systemRuntimeAssembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "GeneratorDiagnosticTests",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new AutoSettingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var runResult = driver.GetRunResult();
        return runResult.Results
            .SelectMany(r => r.Diagnostics)
            .ToArray();
    }
}
