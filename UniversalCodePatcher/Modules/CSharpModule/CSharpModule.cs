using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using UniversalCodePatcher.Interfaces;
using UniversalCodePatcher.Models;
using UniversalCodePatcher.Core;

namespace UniversalCodePatcher.Modules.CSharpModule
{
    public class CSharpModule : BaseModule, ICodeAnalyzer, IPatcher
    {
        public override string ModuleId => "csharp-module";
        public override string Name => "CSharp Module";
        public override Version Version => new(1, 0, 0);
        public override string Description => "Provides C# support";

        protected override bool OnInitialize()
        {
            return true;
        }

        public IEnumerable<PatchType> SupportedPatchTypes => new[]
        {
            PatchType.Replace, PatchType.InsertBefore, PatchType.InsertAfter, PatchType.Delete, PatchType.Modify
        };

        public IEnumerable<string> SupportedLanguages => new[] { "CSharp" };

        public IEnumerable<CodeElement> FindElements(IEnumerable<CodeElement> elements, string pattern)
        {
            return elements.Where(e => e.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        public SyntaxValidationResult ValidateSyntax(string code, string language)
        {
            return new SyntaxValidationResult { IsValid = true };
        }

        public ProjectStructure GetProjectStructure(string projectPath)
        {
            return new ProjectStructure { RootPath = projectPath };
        }

        public IEnumerable<CodeElement> AnalyzeCode(string code, string language)
        {
            if (!SupportedLanguages.Contains(language))
                return Enumerable.Empty<CodeElement>();

            var tree = CSharpSyntaxTree.ParseText(code);

            var refs = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                "Analysis",
                new[] { tree },
                refs,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetCompilationUnitRoot();

            var elements = new List<CodeElement>();

            foreach (var node in root.DescendantNodes())
            {
                switch (node)
                {
                    case ClassDeclarationSyntax cls:
                        var clsSym = model.GetDeclaredSymbol(cls);
                        elements.Add(CreateElement(cls, clsSym, CodeElementType.Class));
                        break;
                    case InterfaceDeclarationSyntax iface:
                        var ifaceSym = model.GetDeclaredSymbol(iface);
                        elements.Add(CreateElement(iface, ifaceSym, CodeElementType.Interface));
                        break;
                    case MethodDeclarationSyntax method:
                        var methSym = model.GetDeclaredSymbol(method);
                        elements.Add(CreateElement(method, methSym, CodeElementType.Method));
                        break;
                    case PropertyDeclarationSyntax prop:
                        var propSym = model.GetDeclaredSymbol(prop);
                        elements.Add(CreateElement(prop, propSym, CodeElementType.Property));
                        break;
                    case FieldDeclarationSyntax field:
                        foreach (var v in field.Declaration.Variables)
                        {
                            var sym = model.GetDeclaredSymbol(v);
                            elements.Add(CreateElement(v, sym, CodeElementType.Field, field));
                        }
                        break;
                }
            }

            return elements.OrderBy(e => e.StartLine);
        }

        private CodeElement CreateElement(SyntaxNode node, ISymbol? symbol, CodeElementType type, SyntaxNode? parentField = null)
        {
            var location = node.GetLocation().GetLineSpan();
            var element = new CodeElement
            {
                Name = symbol?.Name ?? string.Empty,
                FullName = symbol?.ToDisplayString() ?? string.Empty,
                Type = type,
                Language = "CSharp",
                Content = (parentField ?? node).ToFullString(),
                StartLine = location.StartLinePosition.Line + 1,
                EndLine = location.EndLinePosition.Line + 1
            };

            if (symbol is IMethodSymbol methodSym)
            {
                element.Parameters = methodSym.Parameters.Select(p => p.Type.ToDisplayString()).ToArray();
                element.ReturnType = methodSym.ReturnType.ToDisplayString();
            }

            if (symbol is IPropertySymbol propSym)
            {
                element.ReturnType = propSym.Type.ToDisplayString();
            }

            if (symbol is IFieldSymbol fieldSym)
            {
                element.ReturnType = fieldSym.Type.ToDisplayString();
            }

            element.Signature = symbol?.ToDisplayString() ?? element.Name;
            return element;
        }

        public PatchResult ApplyPatch(string originalCode, PatchRule rule, string language)
        {
            var result = new PatchResult();
            try
            {
                if (!SupportedLanguages.Contains(language))
                {
                    result.Errors.Add($"Unsupported language: {language}");
                    result.ModifiedCode = originalCode;
                    return result;
                }

                var tree = CSharpSyntaxTree.ParseText(originalCode);
                var refs = new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
                };

                var compilation = CSharpCompilation.Create(
                    "Patch",
                    new[] { tree },
                    refs,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                var model = compilation.GetSemanticModel(tree);
                var root = tree.GetRoot();
                var generator = SyntaxGenerator.GetGenerator(new AdhocWorkspace(), LanguageNames.CSharp);

                var targetNode = FindTargetNode(root, model, rule);
                if (targetNode == null)
                {
                    result.Errors.Add($"Target '{rule.TargetPattern}' not found");
                    result.ModifiedCode = originalCode;
                    return result;
                }

                SyntaxNode newRoot = root;
                switch (rule.PatchType)
                {
                    case PatchType.Replace:
                        var newMember = SyntaxFactory.ParseMemberDeclaration(rule.NewContent);
                        if (newMember == null)
                        {
                            result.Errors.Add("Invalid new content");
                            result.ModifiedCode = originalCode;
                            return result;
                        }
                        newRoot = root.ReplaceNode(targetNode, newMember);
                        break;
                    case PatchType.Delete:
                        newRoot = root.RemoveNode(targetNode, SyntaxRemoveOptions.KeepNoTrivia);
                        break;
                    case PatchType.Modify when rule.TargetElementType == CodeElementType.Class:
                        var classNode = targetNode as ClassDeclarationSyntax;
                        var memberToAdd = SyntaxFactory.ParseMemberDeclaration(rule.NewContent);
                        if (classNode != null && memberToAdd != null)
                        {
                            var updatedClass = generator.AddMembers(classNode, memberToAdd);
                            newRoot = root.ReplaceNode(classNode, updatedClass);
                        }
                        else
                        {
                            result.Errors.Add("Invalid class modification");
                            result.ModifiedCode = originalCode;
                            return result;
                        }
                        break;
                    default:
                        result.Errors.Add("PatchType not implemented");
                        result.ModifiedCode = originalCode;
                        return result;
                }

                // format and verify compilation
                newRoot = Formatter.Format(newRoot, new AdhocWorkspace());
                var newCode = newRoot.ToFullString();
                var newTree = CSharpSyntaxTree.ParseText(newCode);
                var newCompilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(newTree);
                var diagnostics = newCompilation.GetDiagnostics();
                if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    result.Errors.AddRange(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.ToString()));
                    result.ModifiedCode = originalCode;
                    return result;
                }

                result.ModifiedCode = newCode;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                result.ModifiedCode = originalCode;
            }
            return result;
        }

        private SyntaxNode? FindTargetNode(SyntaxNode root, SemanticModel model, PatchRule rule)
        {
            switch (rule.TargetElementType)
            {
                case CodeElementType.Method:
                    return root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                        .FirstOrDefault(m => SymbolMatches(model.GetDeclaredSymbol(m), rule.TargetPattern));
                case CodeElementType.Property:
                    return root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                        .FirstOrDefault(p => SymbolMatches(model.GetDeclaredSymbol(p), rule.TargetPattern));
                case CodeElementType.Field:
                    return root.DescendantNodes().OfType<FieldDeclarationSyntax>()
                        .FirstOrDefault(f => f.Declaration.Variables.Any(v => SymbolMatches(model.GetDeclaredSymbol(v), rule.TargetPattern)));
                case CodeElementType.Class:
                    return root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                        .FirstOrDefault(c => SymbolMatches(model.GetDeclaredSymbol(c), rule.TargetPattern));
                case CodeElementType.Interface:
                    return root.DescendantNodes().OfType<InterfaceDeclarationSyntax>()
                        .FirstOrDefault(c => SymbolMatches(model.GetDeclaredSymbol(c), rule.TargetPattern));
                default:
                    return null;
            }
        }

        private static bool SymbolMatches(ISymbol? symbol, string pattern)
        {
            if (symbol == null) return false;
            return symbol.Name == pattern || symbol.ToDisplayString() == pattern;
        }

        public bool CanApplyPatch(string code, PatchRule rule, string language)
        {
            if (!SupportedLanguages.Contains(language)) return false;

            var tree = CSharpSyntaxTree.ParseText(code);
            var refs = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
            };
            var compilation = CSharpCompilation.Create(
                "Check",
                new[] { tree },
                refs,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();
            return FindTargetNode(root, model, rule) != null;
        }

        public string PreviewPatch(string originalCode, PatchRule rule, string language)
        {
            var result = ApplyPatch(originalCode, rule, language);
            return result.Success ? result.ModifiedCode : originalCode;
        }

        public ValidationResult ValidateRule(PatchRule rule)
        {
            var result = new ValidationResult { IsValid = true };
            if (string.IsNullOrWhiteSpace(rule.TargetPattern))
            {
                result.IsValid = false;
                result.Errors.Add("Target pattern is required");
            }
            if (rule.PatchType == PatchType.Replace && string.IsNullOrWhiteSpace(rule.NewContent))
            {
                result.IsValid = false;
                result.Errors.Add("New content is required for Replace");
            }
            return result;
        }
    }
}
