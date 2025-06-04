using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UniversalCodePatcher.Interfaces;
using UniversalCodePatcher.Models;

namespace UniversalCodePatcher.Modules.CSharpModule
{
    public class CSharpModule : BaseModule, ICodeAnalyzer, IPatcher
    {
        public IEnumerable<PatchType> SupportedPatchTypes => new[]
        {
            PatchType.Replace, PatchType.InsertBefore, PatchType.InsertAfter, PatchType.Delete, PatchType.Modify
        };

        public IEnumerable<CodeElement> AnalyzeCode(string code, string language)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var method in methods)
            {
                yield return new CodeElement
                {
                    Name = method.Identifier.Text,
                    Type = CodeElementType.Method,
                    Language = "CSharp",
                    Content = method.ToFullString(),
                    StartLine = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    EndLine = method.GetLocation().GetLineSpan().EndLinePosition.Line + 1
                };
            }
            // Можно добавить классы, свойства и т.д.
        }

        public PatchResult ApplyPatch(string originalCode, PatchRule rule, string language)
        {
            var result = new PatchResult();
            try
            {
                var tree = CSharpSyntaxTree.ParseText(originalCode);
                var root = tree.GetRoot();
                var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(m => m.Identifier.Text == rule.TargetPattern);
                if (method == null)
                {
                    result.Errors.Add($"Method '{rule.TargetPattern}' not found");
                    result.ModifiedCode = originalCode;
                    return result;
                }
                SyntaxNode newRoot = root;
                switch (rule.PatchType)
                {
                    case PatchType.Replace:
                        var newMethod = SyntaxFactory.ParseMemberDeclaration(rule.NewContent);
                        newRoot = root.ReplaceNode(method, newMethod);
                        break;
                    case PatchType.Delete:
                        newRoot = root.RemoveNode(method, SyntaxRemoveOptions.KeepNoTrivia);
                        break;
                    // InsertBefore/After/Modify — можно реализовать аналогично
                    default:
                        result.Errors.Add("PatchType not implemented");
                        result.ModifiedCode = originalCode;
                        return result;
                }
                result.ModifiedCode = newRoot.ToFullString();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                result.ModifiedCode = originalCode;
            }
            return result;
        }

        public bool CanApplyPatch(string code, PatchRule rule, string language)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            return root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Any(m => m.Identifier.Text == rule.TargetPattern);
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
