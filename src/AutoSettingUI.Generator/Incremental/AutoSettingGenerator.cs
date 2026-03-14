using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AutoSettingUI.Generator.Incremental;

/// <summary>
/// Incremental source generator for AutoSettingUI.
/// Generates <c>GeneratedSettingProvider</c> that implements <c>ISettingDescriptorProvider</c>
/// and <c>IPropertyValueAccessor</c> without runtime reflection, enabling full AOT compatibility.
/// </summary>
[Generator]
public class AutoSettingGenerator : IIncrementalGenerator
{
    private const string SettingUIAttributeName = "SettingUIAttribute";
    private const string HideAttributeName = "HideAttribute";
    private const string TitleAttributeName = "TitleAttribute";
    private const string MainHeaderAttributeName = "MainHeaderAttribute";
    private const string SubHeaderAttributeName = "SubHeaderAttribute";
    private const string RangeAttributeName = "RangeAttribute";
    private const string ItemsSourceAttributeName = "ItemsSourceAttribute";
    private const string ControlBindingAttributeName = "ControlBindingAttribute";

    // Diagnostic descriptors
    private static readonly DiagnosticDescriptor NonPublicClassWarning = new(
        id: "ASUI001",
        title: "Non-public class with [SettingUI]",
        messageFormat: "Class '{0}' has [SettingUI] but is not public. The source generator may not work correctly.",
        category: "AutoSettingUI",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NestedClassWarning = new(
        id: "ASUI002",
        title: "Nested class with [SettingUI]",
        messageFormat: "Class '{0}' has [SettingUI] but is nested. The source generator may not work correctly.",
        category: "AutoSettingUI",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RangeOnNonNumericWarning = new(
        id: "ASUI003",
        title: "[Range] on non-numeric property",
        messageFormat: "Property '{0}' has [Range] but is of non-numeric type '{1}'. [Range] only works with numeric types.",
        category: "AutoSettingUI",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax c && c.AttributeLists.Count > 0,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        var compilationAndClasses = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        if (symbol == null) return null;

        foreach (var attr in symbol.GetAttributes())
        {
            var attrName = attr.AttributeClass?.Name;
            var attrFullName = attr.AttributeClass?.ToDisplayString();
            if (attrName == SettingUIAttributeName || 
                attrName == "SettingUI" ||
                attrFullName == "AutoSettingUI.Core.Attributes.SettingUIAttribute")
                return symbol;
        }
        return null;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol?> classes,
        SourceProductionContext context)
    {
        // Report diagnostic for debugging
        var debugDiagnostic = Diagnostic.Create(
            new DiagnosticDescriptor(
                id: "ASUI000",
                title: "Generator Debug",
                messageFormat: "Found {0} classes with [SettingUI] attribute",
                category: "AutoSettingUI",
                defaultSeverity: DiagnosticSeverity.Info,
                isEnabledByDefault: true),
            Location.None,
            classes.Length);
        context.ReportDiagnostic(debugDiagnostic);
        
        if (classes.IsDefaultOrEmpty) return;

        // Deduplicate while preserving type
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var distinctClasses = new List<INamedTypeSymbol>();
        foreach (var c in classes)
        {
            if (c == null) continue;
            
            // Report diagnostics for problematic classes
            ReportDiagnostics(c, context);
            
            var key = c.ToDisplayString();
            if (seen.Add(key))
                distinctClasses.Add(c);
        }

        if (distinctClasses.Count == 0) return;

        var code = GenerateProviderCode(distinctClasses, context);
        context.AddSource("GeneratedSettingProvider.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private static void ReportDiagnostics(INamedTypeSymbol classSymbol, SourceProductionContext context)
    {
        // Check for non-public class
        if (classSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            var diagnostic = Diagnostic.Create(NonPublicClassWarning, classSymbol.Locations.FirstOrDefault(), classSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        // Check for nested class
        if (classSymbol.ContainingType != null)
        {
            var diagnostic = Diagnostic.Create(NestedClassWarning, classSymbol.Locations.FirstOrDefault(), classSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string GenerateProviderCode(List<INamedTypeSymbol> classes, SourceProductionContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using AutoSettingUI.Core.Interfaces;");
        sb.AppendLine("using AutoSettingUI.Core.Models;");
        sb.AppendLine("using AutoSettingUI.Core.Registry;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine();
        sb.AppendLine("namespace AutoSettingUI.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Source-generated setting descriptor provider. AOT-safe; no runtime reflection.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public sealed class GeneratedSettingProvider : ISettingDescriptorProvider, IPropertyValueAccessor");
        sb.AppendLine("    {");
        sb.AppendLine("        [ModuleInitializer]");
        sb.AppendLine("        public static void Initialize()");
        sb.AppendLine("        {");
        sb.AppendLine("            var instance = new GeneratedSettingProvider();");
        sb.AppendLine("            AotSettingRegistry.Provider = instance;");
        sb.AppendLine("            AotSettingRegistry.Accessor = instance;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        private readonly Dictionary<string, SettingClassDescriptor> _descriptors");
        sb.AppendLine("            = new Dictionary<string, SettingClassDescriptor>();");
        sb.AppendLine();

        // Constructor
        sb.AppendLine("        public GeneratedSettingProvider()");
        sb.AppendLine("        {");
        foreach (var cls in classes)
            sb.AppendLine($"            Register_{EscapeName(cls.Name)}();");
        sb.AppendLine("        }");


        // ISettingDescriptorProvider
        sb.AppendLine("        public SettingClassDescriptor? GetDescriptor(Type type)");
        sb.AppendLine("            => GetDescriptor(type.FullName ?? type.Name);");
        sb.AppendLine();
        sb.AppendLine("        public SettingClassDescriptor? GetDescriptor(string typeName)");
        sb.AppendLine("        {");
        sb.AppendLine("            _descriptors.TryGetValue(typeName, out var d);");
        sb.AppendLine("            return d;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        public IReadOnlyList<SettingClassDescriptor> GetAllDescriptors()");
        sb.AppendLine("            => new List<SettingClassDescriptor>(_descriptors.Values);");
        sb.AppendLine();
        sb.AppendLine("        public bool HasDescriptor(Type type)");
        sb.AppendLine("            => HasDescriptor(type.FullName ?? type.Name);");
        sb.AppendLine();
        sb.AppendLine("        public bool HasDescriptor(string typeName)");
        sb.AppendLine("            => _descriptors.ContainsKey(typeName);");
        sb.AppendLine();

        // IPropertyValueAccessor – GetValue
        sb.AppendLine("        public object? GetValue(object target, string propertyName)");
        sb.AppendLine("        {");
        foreach (var cls in classes)
        {
            var fqn = cls.ToDisplayString();
            sb.AppendLine($"            if (target is {fqn} t_{EscapeName(cls.Name)})");
            sb.AppendLine("            {");
            sb.AppendLine("                switch (propertyName)");
            sb.AppendLine("                {");
            foreach (var prop in GetPublicInstanceProperties(cls))
                sb.AppendLine($"                    case \"{prop.Name}\": return t_{EscapeName(cls.Name)}.{prop.Name};");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
        }
        sb.AppendLine("            return null;");
        sb.AppendLine("        }");
        sb.AppendLine();

        // IPropertyValueAccessor – SetValue
        sb.AppendLine("        public void SetValue(object target, string propertyName, object? value)");
        sb.AppendLine("        {");
        foreach (var cls in classes)
        {
            var fqn = cls.ToDisplayString();
            var escapedName = EscapeName(cls.Name);
            sb.AppendLine($"            if (target is {fqn} s_{escapedName})");
            sb.AppendLine("            {");
            sb.AppendLine("                switch (propertyName)");
            sb.AppendLine("                {");
            foreach (var prop in GetPublicInstanceProperties(cls))
            {
                if (prop.SetMethod != null && prop.SetMethod.DeclaredAccessibility == Accessibility.Public)
                {
                    var typeFqn = prop.Type.ToDisplayString();
                    sb.AppendLine($"                    case \"{prop.Name}\": s_{escapedName}.{prop.Name} = ({typeFqn})value!; return;");
                }
            }
            sb.AppendLine("                }");
            sb.AppendLine("            }");
        }
        sb.AppendLine("        }");
        sb.AppendLine();

        // IPropertyValueAccessor – GetEnumValue (AOT-safe)
        sb.AppendLine("        public object? GetEnumValue(string typeName, string value)");
        sb.AppendLine("        {");
        sb.AppendLine("            switch (typeName)");
        sb.AppendLine("            {");
        sb.Append(GetEnumParsingCases(classes));
        sb.AppendLine("            }");
        sb.AppendLine("            return null;");
        sb.AppendLine("        }");
        sb.AppendLine();

        // Per-class registration methods
        foreach (var cls in classes)
            AppendRegisterMethod(sb, cls, context);

        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("namespace System.Runtime.CompilerServices");
        sb.AppendLine("{");
        sb.AppendLine("    #if !NET5_0_OR_GREATER");
        sb.AppendLine("    [AttributeUsage(AttributeTargets.Method, Inherited = false)]");
        sb.AppendLine("    internal sealed class ModuleInitializerAttribute : Attribute { }");
        sb.AppendLine("    #endif");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void AppendRegisterMethod(StringBuilder sb, INamedTypeSymbol cls, SourceProductionContext context)
    {
        var fqn = cls.ToDisplayString();
        var safeName = EscapeName(cls.Name);

        var settingAttr = GetAttr(cls, SettingUIAttributeName);
        var mainHeaderAttr = GetAttr(cls, MainHeaderAttributeName);

        var mainHeader = GetConstructorArgString(mainHeaderAttr, 0);

        // Get factory info from SettingUI attribute (ControlFactory is a Type, FactoryMethod is string)
        var factoryTypeArg = settingAttr?.NamedArguments.FirstOrDefault(a => a.Key == "ControlFactory").Value;
        var controlFactoryTypeName = factoryTypeArg?.Value is INamedTypeSymbol factType
            ? $"\"{factType.ToDisplayString()}\"" : "null";
        var factoryMethod = GetNamedArgString(settingAttr, "FactoryMethod");

        sb.AppendLine($"        private void Register_{safeName}()");
        sb.AppendLine("        {");
        sb.AppendLine("            var directProps = new List<PropertyDescriptor>();");
        sb.AppendLine("            var subSections = new List<SubSectionInfo>();");
        sb.AppendLine("            List<PropertyDescriptor>? currentSub = null;");
        sb.AppendLine("            string? currentSubTitle = null;");
        sb.AppendLine();

        foreach (var prop in GetPublicInstanceProperties(cls))
        {
            if (GetAttr(prop, HideAttributeName) != null) continue;

            var subHeaderAttr = GetAttr(prop, SubHeaderAttributeName);
            if (subHeaderAttr != null)
            {
                // Flush previous subsection
                sb.AppendLine("            if (currentSub != null && currentSubTitle != null)");
                sb.AppendLine("                subSections.Add(new SubSectionInfo(currentSubTitle, currentSub));");
                var subTitle = GetConstructorArgString(subHeaderAttr, 0) ?? $"\"{prop.Name}\"";
                sb.AppendLine($"            currentSubTitle = {subTitle};");
                sb.AppendLine("            currentSub = new List<PropertyDescriptor>();");
                // Do NOT continue — fall through to render the property itself into the new sub-section
            }

            var titleAttr = GetAttr(prop, TitleAttributeName);
            var displayName = GetConstructorArgString(titleAttr, 0) ?? $"\"{prop.Name}\"";

            var isEnum = prop.Type.TypeKind == TypeKind.Enum;

            // Range
            var rangeAttr = GetAttr(prop, RangeAttributeName);
            var hasRange = rangeAttr != null;
            
            // Check for [Range] on non-numeric types
            if (hasRange && !IsNumericType(prop.Type))
            {
                var diagnostic = Diagnostic.Create(RangeOnNonNumericWarning, prop.Locations.FirstOrDefault(), prop.Name, prop.Type.ToDisplayString());
                context.ReportDiagnostic(diagnostic);
            }
            
            var minVal = rangeAttr?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? "0";
            var maxVal = rangeAttr?.ConstructorArguments.Length > 1 
                ? rangeAttr.ConstructorArguments[1].Value?.ToString() ?? "0" : "0";

            // Unify property type name to FQN (Avoid 'int' vs 'System.Int32' issues)
            var propTypeFqn = prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "");

            // ItemsSource
            var itemsSrcAttr = GetAttr(prop, ItemsSourceAttributeName);
            string? itemsSrcTypeName = null;
            string? itemsSrcPropName = null;
            if (itemsSrcAttr != null && itemsSrcAttr.ConstructorArguments.Length >= 2)
            {
                if (itemsSrcAttr.ConstructorArguments[0].Value is INamedTypeSymbol srcType)
                    itemsSrcTypeName = srcType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "");
                itemsSrcPropName = itemsSrcAttr.ConstructorArguments[1].Value?.ToString();
            }

            // ControlBinding
            var cbAttr = GetAttr(prop, ControlBindingAttributeName);
            string? cbTypeName = null;
            string? cbBindingProp = null;
            string? cbFactoryMethod = null;
            if (cbAttr != null)
            {
                if (cbAttr.ConstructorArguments.Length > 0 && cbAttr.ConstructorArguments[0].Value is INamedTypeSymbol cbType)
                    cbTypeName = cbType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "");
                cbBindingProp = GetNamedArgRaw(cbAttr, "BindingProperty");
                cbFactoryMethod = GetNamedArgRaw(cbAttr, "FactoryMethod");
            }
            // For enum types, pre-compute the enum values as string array to avoid runtime Type.GetType()
            string? enumValuesExpr = null;
            if (isEnum)
            {
                var enumValues = new List<string>();
                if (prop.Type is INamedTypeSymbol enumType && enumType.TypeKind == TypeKind.Enum)
                {
                    foreach (var member in enumType.GetMembers().OfType<IFieldSymbol>().Where(f => f.IsConst))
                    {
                        enumValues.Add(member.Name);
                    }
                }
                if (enumValues.Count > 0)
                {
                    var valuesArray = string.Join(", ", enumValues.Select(v => $"\"{v}\""));
                    enumValuesExpr = $"new string[] {{ {valuesArray} }}";
                }
            }

            sb.AppendLine($"            var pd_{safeName}_{EscapeName(prop.Name)} = new PropertyDescriptor(");
            sb.AppendLine($"                \"{prop.Name}\",");
            sb.AppendLine($"                {displayName},");
            sb.AppendLine($"                \"{propTypeFqn}\",");
            sb.AppendLine($"                {(isEnum ? "true" : "false")},");
            sb.AppendLine($"                false,");
            sb.AppendLine($"                {(hasRange ? "true" : "false")},");
            sb.AppendLine($"                {minVal},");
            sb.AppendLine($"                {maxVal},");
            sb.AppendLine($"                {(itemsSrcTypeName != null ? $"\"{itemsSrcTypeName}\"" : "null")},");
            sb.AppendLine($"                {(itemsSrcPropName != null ? $"\"{itemsSrcPropName}\"" : "null")},");
            sb.AppendLine($"                {(cbTypeName != null ? $"\"{cbTypeName}\"" : "null")},");
            sb.AppendLine($"                {(cbBindingProp != null ? $"\"{cbBindingProp}\"" : "null")},");
            sb.AppendLine($"                {(cbFactoryMethod != null ? $"\"{cbFactoryMethod}\"" : "null")},");
            sb.AppendLine($"                {(enumValuesExpr ?? "null")});");

            var varName = $"pd_{safeName}_{EscapeName(prop.Name)}";
            sb.AppendLine($"            if (currentSub != null) currentSub.Add({varName}); else directProps.Add({varName});");
            sb.AppendLine();
        }

        // Flush last subsection
        sb.AppendLine("            if (currentSub != null && currentSubTitle != null)");
        sb.AppendLine("                subSections.Add(new SubSectionInfo(currentSubTitle, currentSub));");
        sb.AppendLine();
        sb.AppendLine("            var descriptor = new SettingClassDescriptor(");
        sb.AppendLine($"                \"{fqn}\",");
        sb.AppendLine($"                \"{cls.Name}\",");
        sb.AppendLine($"                {mainHeader},");
        sb.AppendLine("                directProps,");
        sb.AppendLine("                subSections,");
        sb.AppendLine($"                {controlFactoryTypeName},");
        sb.AppendLine($"                {factoryMethod});");
        sb.AppendLine($"            _descriptors[\"{fqn}\"] = descriptor;");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static bool IsNumericType(ITypeSymbol type)
    {
        var typeName = type.ToDisplayString();
        return typeName switch
        {
            "System.Byte" or "System.SByte" => true,
            "System.Int16" or "System.UInt16" => true,
            "System.Int32" or "System.UInt32" => true,
            "System.Int64" or "System.UInt64" => true,
            "System.Single" => true,
            "System.Double" => true,
            "System.Decimal" => true,
            "byte" or "sbyte" => true,
            "short" or "ushort" => true,
            "int" or "uint" => true,
            "long" or "ulong" => true,
            "float" => true,
            "double" => true,
            "decimal" => true,
            _ => false
        };
    }

    private static IEnumerable<IPropertySymbol> GetPublicInstanceProperties(INamedTypeSymbol cls)
        => cls.GetMembers()
              .OfType<IPropertySymbol>()
              .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic);

    private static AttributeData? GetAttr(ISymbol symbol, string name)
    {
        return symbol.GetAttributes().FirstOrDefault(a => 
        {
            var attrName = a.AttributeClass?.Name;
            var attrFullName = a.AttributeClass?.ToDisplayString();
            return attrName == name || 
                   attrName == name.Replace("Attribute", "") ||
                   attrFullName == $"AutoSettingUI.Core.Attributes.{name}";
        });
    }

    private static string? GetConstructorArgString(AttributeData? attr, int index)
    {
        if (attr == null || attr.ConstructorArguments.Length <= index) return null;
        var val = attr.ConstructorArguments[index].Value;
        return val != null ? $"\"{EscapeString(val.ToString()!)}\"" : "null";
    }

    private static string? GetNamedArgString(AttributeData? attr, string key)
    {
        if (attr == null) return null;
        foreach (var na in attr.NamedArguments)
        {
            if (na.Key == key)
            {
                var val = na.Value.Value;
                return val != null ? $"\"{EscapeString(val.ToString()!)}\"" : "null";
            }
        }
        return "null";
    }

    private static string? GetNamedArgRaw(AttributeData? attr, string key)
    {
        if (attr == null) return null;
        foreach (var na in attr.NamedArguments)
        {
            if (na.Key == key)
                return na.Value.Value?.ToString();
        }
        return null;
    }

    /// <summary>Makes a C# identifier safe by replacing special characters with underscores.</summary>
    private static string EscapeName(string name)
        => name.Replace('.', '_').Replace('<', '_').Replace('>', '_').Replace(',', '_').Replace(' ', '_');

    private static string GetEnumParsingCases(IEnumerable<INamedTypeSymbol> classes)
    {
        var sb = new StringBuilder();
        var processedEnums = new HashSet<string>();

        foreach (var cls in classes)
        {
            foreach (var prop in GetPublicInstanceProperties(cls))
            {
                if (prop.Type.TypeKind == TypeKind.Enum)
                {
                    var typeFqn = prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "");
                    if (processedEnums.Add(typeFqn))
                    {
                        sb.AppendLine($"                case \"{typeFqn}\": return Enum.Parse(typeof({prop.Type.ToDisplayString()}), value);");
                    }
                }
            }
        }
        return sb.ToString();
    }

    private static string EscapeString(string s)
        => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
