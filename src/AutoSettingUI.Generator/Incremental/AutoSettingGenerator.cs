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
    private const string CommandCanExecuteAttributeName = "CommandCanExecuteAttribute";
    private const string ReadOnlyAttributeName = "ReadOnlyAttribute";
    private const string CollectionEditorAttributeName = "CollectionEditorAttribute";
    private const string PlaceholderAttributeName = "PlaceholderAttribute";
    private const string DescriptionAttributeName = "DescriptionAttribute";
    private const string PasswordAttributeName = "PasswordAttribute";
    private const string NumericUpDownAttributeName = "NumericUpDownAttribute"; // Special handling needed - type depends on property type
    private const string ControlBindingDefaultsAttributeName = "ControlBindingDefaultsAttribute";

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

        var code = GenerateProviderCode(distinctClasses, context, compilation);
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

    private static string GenerateProviderCode(List<INamedTypeSymbol> classes, SourceProductionContext context, Compilation compilation)
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
        sb.AppendLine("using System.Diagnostics.CodeAnalysis;");
        sb.AppendLine();
        sb.AppendLine("namespace AutoSettingUI.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Source-generated setting descriptor provider. AOT-safe; no runtime reflection.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public sealed class GeneratedSettingProvider : ISettingDescriptorProvider, IPropertyValueAccessor");
        sb.AppendLine("    {");
        sb.AppendLine("        [ModuleInitializer]");
        foreach (var typeSymbol in GetTypesWithDynamicMethods(classes))
        {
            var typeFqn = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            sb.AppendLine($"        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof({typeFqn}))]");
        }
        foreach (var typeSymbol in GetTypesWithBindableProperties(classes, compilation))
        {
            var typeFqn = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            sb.AppendLine($"        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof({typeFqn}))]");
        }
        foreach (var typeSymbol in GetControlTypeDependencies(classes, compilation))
        {
            var typeFqn = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            sb.AppendLine($"        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof({typeFqn}))]");
            sb.AppendLine($"        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicFields, typeof({typeFqn}))]");
        }
        foreach (var typeSymbol in GetAttributeTypeDependencies(classes))
        {
            var typeFqn = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            sb.AppendLine($"        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors, typeof({typeFqn}))]");
        }
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
            var fqn = cls.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
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
            var fqn = cls.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var escapedName = EscapeName(cls.Name);
            sb.AppendLine($"            if (target is {fqn} s_{escapedName})");
            sb.AppendLine("            {");
            sb.AppendLine("                switch (propertyName)");
            sb.AppendLine("                {");
            foreach (var prop in GetPublicInstanceProperties(cls))
            {
                if (prop.SetMethod != null && prop.SetMethod.DeclaredAccessibility == Accessibility.Public)
                {
                    var typeFqn = prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
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
        string? controlFactoryTypeName = null;
        if (factoryTypeArg?.Value is INamedTypeSymbol factType)
        {
            // Use AssemblyQualifiedName format for Type.GetType() to work correctly
            var assemblyName = factType.ContainingAssembly?.Name;
            var typeFullName = factType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "");
            controlFactoryTypeName = !string.IsNullOrEmpty(assemblyName) 
                ? $"{typeFullName}, {assemblyName}" 
                : typeFullName;
        }
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
            var cbAttr = GetAttr(prop, ControlBindingAttributeName) ?? 
                         GetAttrInherited(prop, ControlBindingAttributeName);
            string? cbTypeName = null;
            string? cbBindingProp = null;
            string? cbFactoryMethod = null;
            
            if (cbAttr != null)
            {
                // Try to get ControlType from constructor argument first (e.g., [ControlBinding(typeof(CheckBox)])
                if (cbAttr.ConstructorArguments.Length > 0 && cbAttr.ConstructorArguments[0].Value is INamedTypeSymbol cbType)
                {
                    // Use AssemblyQualifiedName format for Type.GetType() to work correctly
                    var assemblyName = cbType.ContainingAssembly?.Name;
                    var typeFullName = cbType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "");
                    cbTypeName = !string.IsNullOrEmpty(assemblyName) 
                        ? $"{typeFullName}, {assemblyName}" 
                        : typeFullName;
                }
                
                // Get BindingProperty from constructor argument (2nd param) or named argument
                if (cbAttr.ConstructorArguments.Length > 1 && cbAttr.ConstructorArguments[1].Value is string bindingPropFromCtor)
                    cbBindingProp = bindingPropFromCtor;
                else
                    cbBindingProp = GetNamedArgRaw(cbAttr, "BindingProperty");
                
                // Get FactoryMethod from constructor argument (3rd param) or named argument
                if (cbAttr.ConstructorArguments.Length > 2 && cbAttr.ConstructorArguments[2].Value is string factoryMethodFromCtor)
                    cbFactoryMethod = factoryMethodFromCtor;
                else
                    cbFactoryMethod = GetNamedArgRaw(cbAttr, "FactoryMethod");

                // If derived attributes pass defaults via base ctor, extract them from syntax.
                if (cbTypeName == null || cbBindingProp == null || cbFactoryMethod == null)
                {
                    var defaults = TryGetControlBindingDefaultsFromAttributeClass(cbAttr);
                    cbTypeName ??= defaults.controlTypeName;
                    cbBindingProp ??= defaults.bindingProperty;
                    cbFactoryMethod ??= defaults.factoryMethod;
                }

                // Special handling for NumericUpDown to map control type by property type when unspecified
                var attrName = cbAttr.AttributeClass?.Name;
                if (attrName == NumericUpDownAttributeName || attrName == "NumericUpDown")
                {
                    cbFactoryMethod ??= "CreateNumericUpDown";
                    cbBindingProp ??= "Value";
                    if (cbTypeName == null)
                    {
                        // Map type based on property type for Ursa / Avalonia
                        // Include assembly name for Type.GetType() to work correctly
                        cbTypeName = propTypeFqn switch
                        {
                            "int" or "System.Int32" => "Ursa.Controls.NumericIntUpDown, Ursa",
                            "uint" or "System.UInt32" => "Ursa.Controls.NumericUIntUpDown, Ursa",
                            "double" or "System.Double" => "Ursa.Controls.NumericDoubleUpDown, Ursa",
                            "float" or "System.Single" => "Ursa.Controls.NumericFloatUpDown, Ursa",
                            "byte" or "System.Byte" => "Ursa.Controls.NumericByteUpDown, Ursa",
                            "sbyte" or "System.SByte" => "Ursa.Controls.NumericSByteUpDown, Ursa",
                            "short" or "System.Int16" => "Ursa.Controls.NumericShortUpDown, Ursa",
                            "ushort" or "System.UInt16" => "Ursa.Controls.NumericUShortUpDown, Ursa",
                            "long" or "System.Int64" => "Ursa.Controls.NumericLongUpDown, Ursa",
                            "ulong" or "System.UInt64" => "Ursa.Controls.NumericULongUpDown, Ursa",
                            _ => "Ursa.Controls.NumericIntUpDown, Ursa"
                        };
                    }
                }
            }

            // Check if property type is a delegate
            var isDelegate = IsDelegateType(prop.Type);

            // CommandCanExecute
            var canExecAttr = GetAttr(prop, CommandCanExecuteAttributeName);
            var canExecuteMethodName = GetConstructorArgRaw(canExecAttr, 0);

            // ReadOnly
            var readOnlyAttr = GetAttr(prop, ReadOnlyAttributeName);
            bool isReadOnly = false;
            string? readOnlyMethodName = null;
            if (readOnlyAttr != null)
            {
                // Check if it's a bool or string constructor argument
                if (readOnlyAttr.ConstructorArguments.Length > 0)
                {
                    var arg = readOnlyAttr.ConstructorArguments[0];
                    if (arg.Value is bool boolVal)
                        isReadOnly = boolVal;
                    else if (arg.Value is string strVal)
                        readOnlyMethodName = strVal;
                }
            }

            // Placeholder / Description / Password
            var placeholderAttr = GetAttr(prop, PlaceholderAttributeName);
            var placeholderText = GetConstructorArgString(placeholderAttr, 0);

            var descriptionAttr = GetAttr(prop, DescriptionAttributeName);
            var descriptionText = GetConstructorArgString(descriptionAttr, 0);

            var passwordAttr = GetAttr(prop, PasswordAttributeName);
            bool isPassword = false;
            char passwordMaskChar = '•';
            if (passwordAttr != null)
            {
                isPassword = true;
                var maskCharArg = passwordAttr.ConstructorArguments.Length > 0
                    ? passwordAttr.ConstructorArguments[0].Value
                    : null;
                if (maskCharArg is char c)
                    passwordMaskChar = c;
                var namedMaskChar = GetNamedArgRaw(passwordAttr, "MaskChar");
                if (!string.IsNullOrEmpty(namedMaskChar) && namedMaskChar.Length > 0)
                    passwordMaskChar = namedMaskChar[0];
                var maskFlag = GetNamedArgRaw(passwordAttr, "Mask");
                if (maskFlag == "False")
                    isPassword = false;
            }

            // NumericUpDown settings (AOT-safe)
            bool isNumericUpDown = false;
            double numericMin = 0;
            double numericMax = 0;
            double numericInc = 0;
            if (cbAttr?.AttributeClass?.Name == NumericUpDownAttributeName ||
                cbAttr?.AttributeClass?.Name == "NumericUpDown")
            {
                isNumericUpDown = true;
                numericMin = TryGetNamedArgDouble(cbAttr, "Minimum") ?? double.MinValue;
                numericMax = TryGetNamedArgDouble(cbAttr, "Maximum") ?? double.MaxValue;
                numericInc = TryGetNamedArgDouble(cbAttr, "Increment") ?? 1.0;
            }

            // CollectionEditor
            var collEditorAttr = GetAttr(prop, CollectionEditorAttributeName);
            string? collEditorTypeName = null;
            string? collEditorFactoryMethod = null;
            bool collAllowAdd = true;
            bool collAllowRemove = true;
            bool collAllowReorder = true;
            bool collAllowEditItems = true;
            string? collElementTypeName = null;

            // Check if collection type
            var isCollection = IsCollectionType(prop.Type);
            if (isCollection)
            {
                var elementType = GetCollectionElementType(prop.Type);
                if (elementType != null)
                {
                    // Use AssemblyQualifiedName format for Type.GetType() to work correctly
                    var assemblyName = elementType.ContainingAssembly?.Name;
                    var typeFullName = elementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "");
                    collElementTypeName = !string.IsNullOrEmpty(assemblyName) 
                        ? $"{typeFullName}, {assemblyName}" 
                        : typeFullName;
                }
            }

            if (collEditorAttr != null)
            {
                if (collEditorAttr.ConstructorArguments.Length > 0 && collEditorAttr.ConstructorArguments[0].Value is INamedTypeSymbol editorType)
                {
                    // Use AssemblyQualifiedName format for Type.GetType() to work correctly
                    var assemblyName = editorType.ContainingAssembly?.Name;
                    var typeFullName = editorType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "");
                    collEditorTypeName = !string.IsNullOrEmpty(assemblyName) 
                        ? $"{typeFullName}, {assemblyName}" 
                        : typeFullName;
                }
                collEditorFactoryMethod = GetNamedArgRaw(collEditorAttr, "FactoryMethod");
                var allowAddVal = GetNamedArgRaw(collEditorAttr, "AllowAdd");
                var allowRemoveVal = GetNamedArgRaw(collEditorAttr, "AllowRemove");
                var allowReorderVal = GetNamedArgRaw(collEditorAttr, "AllowReorder");
                var allowEditItemsVal = GetNamedArgRaw(collEditorAttr, "AllowEditItems");
                if (allowAddVal != null) collAllowAdd = allowAddVal == "True";
                if (allowRemoveVal != null) collAllowRemove = allowRemoveVal == "True";
                if (allowReorderVal != null) collAllowReorder = allowReorderVal == "True";
                if (allowEditItemsVal != null) collAllowEditItems = allowEditItemsVal == "True";
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
            sb.AppendLine($"                typeof({prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}),");
            sb.AppendLine($"                {(isEnum ? "true" : "false")},");
            sb.AppendLine($"                {(isCollection ? "true" : "false")},");
            sb.AppendLine($"                {(hasRange ? "true" : "false")},");
            sb.AppendLine($"                {minVal},");
            sb.AppendLine($"                {maxVal},");
            sb.AppendLine($"                {(itemsSrcTypeName != null ? $"\"{itemsSrcTypeName}\"" : "null")},");
            sb.AppendLine($"                {(itemsSrcPropName != null ? $"\"{itemsSrcPropName}\"" : "null")},");
            sb.AppendLine($"                {(cbTypeName != null ? $"\"{cbTypeName}\"" : "null")},");
            sb.AppendLine($"                {(cbBindingProp != null ? $"\"{cbBindingProp}\"" : "null")},");
            sb.AppendLine($"                {(cbFactoryMethod != null ? $"\"{cbFactoryMethod}\"" : "null")},");
            sb.AppendLine($"                {(enumValuesExpr ?? "null")},");
            sb.AppendLine($"                {(isDelegate ? "true" : "false")},");
            sb.AppendLine($"                {(canExecuteMethodName != null ? $"\"{canExecuteMethodName}\"" : "null")},");
            sb.AppendLine($"                {(isReadOnly ? "true" : "false")},");
            sb.AppendLine($"                {(readOnlyMethodName != null ? $"\"{readOnlyMethodName}\"" : "null")},");
            sb.AppendLine($"                {(collEditorTypeName != null ? $"\"{collEditorTypeName}\"" : "null")},");
            sb.AppendLine($"                {(collEditorFactoryMethod != null ? $"\"{collEditorFactoryMethod}\"" : "null")},");
            sb.AppendLine($"                {(collAllowAdd ? "true" : "false")},");
            sb.AppendLine($"                {(collAllowRemove ? "true" : "false")},");
            sb.AppendLine($"                {(collAllowReorder ? "true" : "false")},");
            sb.AppendLine($"                {(collAllowEditItems ? "true" : "false")},");
            sb.AppendLine($"                {(collElementTypeName != null ? $"\"{collElementTypeName}\"" : "null")},");
            sb.AppendLine($"                {(placeholderText ?? "null")},");
            sb.AppendLine($"                {(descriptionText ?? "null")},");
            sb.AppendLine($"                {(isPassword ? "true" : "false")},");
            sb.AppendLine($"                '{passwordMaskChar}',");
            sb.AppendLine($"                {(isNumericUpDown ? "true" : "false")},");
            sb.AppendLine($"                {numericMin},");
            sb.AppendLine($"                {numericMax},");
            sb.AppendLine($"                {numericInc});");

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
        sb.AppendLine($"                {(controlFactoryTypeName != null ? $"\"{controlFactoryTypeName}\"" : "null")},");
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

    private static AttributeData? GetAttrInherited(ISymbol symbol, string baseName)
    {
        return symbol.GetAttributes().FirstOrDefault(a => 
        {
            var baseType = a.AttributeClass?.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == baseName || 
                    baseType.ToDisplayString() == $"AutoSettingUI.Core.Attributes.{baseName}")
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
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

    private static double? TryGetNamedArgDouble(AttributeData? attr, string key)
    {
        var raw = GetNamedArgRaw(attr, key);
        if (raw == null) return null;
        if (double.TryParse(raw, out var val)) return val;
        return null;
    }

    /// <summary>Gets a named argument value as a Type symbol.</summary>
    private static INamedTypeSymbol? GetNamedArgType(AttributeData? attr, string key)
    {
        if (attr == null) return null;
        foreach (var na in attr.NamedArguments)
        {
            if (na.Key == key && na.Value.Value is INamedTypeSymbol typeSymbol)
                return typeSymbol;
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
                        sb.AppendLine($"                case \"{typeFqn}\": return Enum.Parse(typeof({prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}), value);");
                    }
                }
            }
        }
        return sb.ToString();
    }

    private static string EscapeString(string s)
        => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static IEnumerable<INamedTypeSymbol> GetTypesWithDynamicMethods(IEnumerable<INamedTypeSymbol> classes)
    {
        var set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var cls in classes)
        {
            foreach (var prop in GetPublicInstanceProperties(cls))
            {
                var canExecAttr = GetAttr(prop, CommandCanExecuteAttributeName);
                var canExecuteMethodName = GetConstructorArgRaw(canExecAttr, 0);
                var readOnlyAttr = GetAttr(prop, ReadOnlyAttributeName);
                var readOnlyMethodName = GetConstructorArgRaw(readOnlyAttr, 0);

                var cbAttr = GetAttr(prop, ControlBindingAttributeName) ??
                             GetAttrInherited(prop, ControlBindingAttributeName);
                var factoryMethod = GetConstructorArgRaw(cbAttr, 2) ?? GetNamedArgRaw(cbAttr, "FactoryMethod");

                if (!string.IsNullOrEmpty(canExecuteMethodName) ||
                    !string.IsNullOrEmpty(readOnlyMethodName) ||
                    !string.IsNullOrEmpty(factoryMethod))
                {
                    set.Add(cls);
                    break;
                }
            }
        }
        return set;
    }

    private static IEnumerable<INamedTypeSymbol> GetTypesWithBindableProperties(IEnumerable<INamedTypeSymbol> classes, Compilation compilation)
    {
        var set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var cls in classes)
        {
            set.Add(cls);

            foreach (var prop in GetPublicInstanceProperties(cls))
            {
                if (IsCollectionType(prop.Type))
                {
                    var elementType = GetCollectionElementType(prop.Type) as INamedTypeSymbol;
                    if (elementType != null)
                        set.Add(elementType);
                }
            }
        }
        return set;
    }

    private static IEnumerable<INamedTypeSymbol> GetControlTypeDependencies(IEnumerable<INamedTypeSymbol> classes, Compilation compilation)
    {
        var set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var cls in classes)
        {
            foreach (var prop in GetPublicInstanceProperties(cls))
            {
                var cbAttr = GetAttr(prop, ControlBindingAttributeName) ??
                             GetAttrInherited(prop, ControlBindingAttributeName);
                if (cbAttr != null)
                {
                    if (cbAttr.ConstructorArguments.Length > 0 && cbAttr.ConstructorArguments[0].Value is INamedTypeSymbol cbType)
                        set.Add(cbType);

                    var defaultsAttr = cbAttr.AttributeClass?.GetAttributes().FirstOrDefault(a =>
                        a.AttributeClass?.ToDisplayString() == $"AutoSettingUI.Core.Attributes.{ControlBindingDefaultsAttributeName}");
                    if (defaultsAttr != null && defaultsAttr.ConstructorArguments.Length > 0 &&
                        defaultsAttr.ConstructorArguments[0].Value is INamedTypeSymbol defaultType)
                        set.Add(defaultType);

                    var attrName = cbAttr.AttributeClass?.Name;
                    if (attrName == NumericUpDownAttributeName || attrName == "NumericUpDown")
                    {
                        var typeName = prop.Type.ToDisplayString();
                        var mapped = typeName switch
                        {
                            "int" or "System.Int32" => "Ursa.Controls.NumericIntUpDown",
                            "uint" or "System.UInt32" => "Ursa.Controls.NumericUIntUpDown",
                            "double" or "System.Double" => "Ursa.Controls.NumericDoubleUpDown",
                            "float" or "System.Single" => "Ursa.Controls.NumericFloatUpDown",
                            "byte" or "System.Byte" => "Ursa.Controls.NumericByteUpDown",
                            "sbyte" or "System.SByte" => "Ursa.Controls.NumericSByteUpDown",
                            "short" or "System.Int16" => "Ursa.Controls.NumericShortUpDown",
                            "ushort" or "System.UInt16" => "Ursa.Controls.NumericUShortUpDown",
                            "long" or "System.Int64" => "Ursa.Controls.NumericLongUpDown",
                            "ulong" or "System.UInt64" => "Ursa.Controls.NumericULongUpDown",
                            _ => "Ursa.Controls.NumericIntUpDown"
                        };
                        var symbol = compilation.GetTypeByMetadataName(mapped);
                        if (symbol != null)
                            set.Add(symbol);
                    }
                }

                var collEditorAttr = GetAttr(prop, CollectionEditorAttributeName);
                if (collEditorAttr != null &&
                    collEditorAttr.ConstructorArguments.Length > 0 &&
                    collEditorAttr.ConstructorArguments[0].Value is INamedTypeSymbol editorType)
                {
                    set.Add(editorType);
                }
            }
        }

        return set;
    }

    private static IEnumerable<INamedTypeSymbol> GetAttributeTypeDependencies(IEnumerable<INamedTypeSymbol> classes)
    {
        var set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var cls in classes)
        {
            foreach (var prop in GetPublicInstanceProperties(cls))
            {
                var cbAttr = GetAttr(prop, ControlBindingAttributeName) ??
                             GetAttrInherited(prop, ControlBindingAttributeName);
                if (cbAttr == null) continue;

                var factoryMethod = GetConstructorArgRaw(cbAttr, 2) ?? GetNamedArgRaw(cbAttr, "FactoryMethod");
                if (!string.IsNullOrEmpty(factoryMethod) && cbAttr.AttributeClass != null)
                    set.Add(cbAttr.AttributeClass);
            }
        }

        return set;
    }

    /// <summary>Checks if a type is a delegate type (Action, Func, custom delegate).</summary>
    private static bool IsDelegateType(ITypeSymbol type)
    {
        // Delegate is the base class for all delegates
        // We need to check if it inherits from Delegate but is not Delegate itself
        if (type.TypeKind == TypeKind.Delegate)
            return true;

        // Check if it inherits from System.Delegate
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.ToDisplayString() == "System.Delegate" ||
                baseType.ToDisplayString() == "System.MulticastDelegate")
                return true;
            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>Checks if a type is a collection type.</summary>
    private static bool IsCollectionType(ITypeSymbol type)
    {
        var typeName = type.ToDisplayString();
        // Exclude string as it implements IEnumerable but is not a collection for our purposes
        if (typeName == "string" || typeName == "System.String")
            return false;

        // Check if it implements IEnumerable
        if (type.AllInterfaces.Any(i => i.ToDisplayString().StartsWith("System.Collections.Generic.IEnumerable")))
            return true;

        // Check if it's an array
        if (type.TypeKind == TypeKind.Array)
            return true;

        return false;
    }

    /// <summary>Gets the element type of a collection type.</summary>
    private static ITypeSymbol? GetCollectionElementType(ITypeSymbol type)
    {
        // Handle array types
        if (type is IArrayTypeSymbol arrayType)
            return arrayType.ElementType;

        // Handle generic types (List<T>, IList<T>, IEnumerable<T>, etc.)
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var typeArgs = namedType.TypeArguments;
            if (typeArgs.Length > 0)
                return typeArgs[0];
        }

        // Check interfaces for generic IEnumerable<T>
        foreach (var iface in type.AllInterfaces)
        {
            if (iface.IsGenericType &&
                iface.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>")
            {
                var typeArgs = iface.TypeArguments;
                if (typeArgs.Length > 0)
                    return typeArgs[0];
            }
        }

        return null;
    }

    /// <summary>Gets a raw constructor argument value as string.</summary>
    private static string? GetConstructorArgRaw(AttributeData? attr, int index)
    {
        if (attr == null || attr.ConstructorArguments.Length <= index) return null;
        return attr.ConstructorArguments[index].Value?.ToString();
    }

    private static (string? controlTypeName, string? bindingProperty, string? factoryMethod)
        TryGetControlBindingDefaultsFromAttributeClass(AttributeData attr)
    {
        var attrType = attr.AttributeClass;
        if (attrType == null) return (null, null, null);

        var defaultsAttr = attrType.GetAttributes().FirstOrDefault(a =>
        {
            var name = a.AttributeClass?.Name;
            var fullName = a.AttributeClass?.ToDisplayString();
            return name == ControlBindingDefaultsAttributeName ||
                   name == ControlBindingDefaultsAttributeName.Replace("Attribute", "") ||
                   fullName == $"AutoSettingUI.Core.Attributes.{ControlBindingDefaultsAttributeName}";
        });

        if (defaultsAttr == null) return (null, null, null);

        string? controlTypeName = null;
        string? bindingProperty = null;
        string? factoryMethod = null;

        if (defaultsAttr.ConstructorArguments.Length > 0 &&
            defaultsAttr.ConstructorArguments[0].Value is INamedTypeSymbol typeSymbol)
            controlTypeName = ToAssemblyQualifiedName(typeSymbol);

        if (defaultsAttr.ConstructorArguments.Length > 1 &&
            defaultsAttr.ConstructorArguments[1].Value is string binding)
            bindingProperty = binding;

        if (defaultsAttr.ConstructorArguments.Length > 2 &&
            defaultsAttr.ConstructorArguments[2].Value is string factory)
            factoryMethod = factory;

        return (controlTypeName, bindingProperty, factoryMethod);
    }

    private static string ToAssemblyQualifiedName(INamedTypeSymbol typeSymbol)
    {
        var assemblyName = typeSymbol.ContainingAssembly?.Name;
        var typeFullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "");
        return !string.IsNullOrEmpty(assemblyName)
            ? $"{typeFullName}, {assemblyName}"
            : typeFullName;
    }
}
