using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Esprima;
using Esprima.Ast;
using UniversalCodePatcher.Core;
using UniversalCodePatcher.Interfaces;
using UniversalCodePatcher.Models;

namespace UniversalCodePatcher.Modules.JavaScriptModule
{
    /// <summary>
    /// Модуль для работы с JavaScript/TypeScript кодом
    /// </summary>
    public class JavaScriptModule : BaseModule, ICodeAnalyzer, IPatcher
    {
        public override string ModuleId => "javascript-module";
        public override string Name => "JavaScript/TypeScript Module";
        public override Version Version => new(1, 0, 0);
        public override string Description => "Provides analysis and patching for JavaScript and TypeScript code";
        
        public IEnumerable<string> SupportedLanguages => new[] { "JavaScript", "TypeScript" };
        public IEnumerable<PatchType> SupportedPatchTypes => Enum.GetValues<PatchType>();

        protected override bool OnInitialize()
        {
            
            // Регистрируем сервисы
            ServiceContainer?.RegisterInstance<ICodeAnalyzer>(this);
            ServiceContainer?.RegisterInstance<IPatcher>(this);
            
            OnInfo("JavaScript module initialized successfully");
            return true;
        }
        
        public IEnumerable<CodeElement> AnalyzeCode(string code, string language)
        {
            if (!SupportedLanguages.Contains(language))
                return Enumerable.Empty<CodeElement>();

            var elements = new List<CodeElement>();
            Script script;
            try
            {
                script = new JavaScriptParser(code, new ParserOptions { Tolerant = true }).ParseScript();
            }
            catch (ParserException ex)
            {
                OnError($"Parse error: {ex.Description}");
                return Enumerable.Empty<CodeElement>();
            }

            var nodes = Traverse(script).ToList();
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case FunctionDeclaration func:
                        elements.Add(CreateCodeElement(func, CodeElementType.Function, code, nodes, script));
                        break;
                    case ArrowFunctionExpression arrow:
                        elements.Add(CreateCodeElement(arrow, CodeElementType.Lambda, code, nodes, script));
                        break;
                    case ClassDeclaration cls:
                        elements.Add(CreateCodeElement(cls, CodeElementType.Class, code, nodes, script));
                        foreach (var method in cls.Body.Body.OfType<MethodDefinition>())
                        {
                            elements.Add(CreateCodeElement(method, CodeElementType.Method, code, nodes, script));
                        }
                        break;
                }
            }

            return elements.OrderBy(e => e.StartLine);
        }
        
        private CodeElement CreateCodeElement(Node node, CodeElementType type, string fullCode, IEnumerable<Node> allNodes, Script root)
        {
            var element = new CodeElement
            {
                Type = type,
                Content = fullCode.Substring(node.Range.Start, node.Range.End - node.Range.Start),
                Language = "JavaScript",
                StartLine = node.Location.Start.Line,
                EndLine = node.Location.End.Line,
                StartColumn = node.Location.Start.Column,
                EndColumn = node.Location.End.Column
            };

            switch (node)
            {
                case FunctionDeclaration func:
                    element.Name = func.Id?.Name ?? string.Empty;
                    element.Parameters = func.Params.Select(p => ExtractParameterName(p, fullCode)).ToArray();
                    break;
                case ArrowFunctionExpression arrow:
                    element.Name = GetArrowFunctionName(arrow, allNodes);
                    element.Parameters = arrow.Params.Select(p => ExtractParameterName(p, fullCode)).ToArray();
                    break;
                case ClassDeclaration cls:
                    element.Name = cls.Id?.Name ?? string.Empty;
                    break;
                case MethodDefinition method:
                    element.Name = ExtractPropertyName(method.Key, fullCode);
                    if (method.Value is FunctionExpression fe)
                        element.Parameters = fe.Params.Select(p => ExtractParameterName(p, fullCode)).ToArray();
                    break;
            }

            element.Signature = $"{type}:{element.Name}({string.Join(",", element.Parameters)})";
            return element;
        }

        private static string ExtractParameterName(Expression expr, string code)
        {
            return expr switch
            {
                Identifier id => id.Name,
                AssignmentPattern ap => ExtractParameterName(ap.Left, code),
                RestElement rest => "..." + ExtractParameterName(rest.Argument, code),
                _ => code.Substring(expr.Range.Start, expr.Range.End - expr.Range.Start)
            };
        }

        private static string ExtractPropertyName(Expression expr, string code)
        {
            return expr switch
            {
                Identifier id => id.Name,
                Literal lit => lit.Value?.ToString() ?? string.Empty,
                _ => code.Substring(expr.Range.Start, expr.Range.End - expr.Range.Start)
            };
        }

        private static string GetArrowFunctionName(ArrowFunctionExpression arrow, IEnumerable<Node> nodes)
        {
            var vd = nodes.OfType<VariableDeclarator>().FirstOrDefault(d => object.ReferenceEquals(d.Init, arrow));
            if (vd != null && vd.Id is Identifier vid)
                return vid.Name;

            var assign = nodes.OfType<AssignmentExpression>().FirstOrDefault(a => object.ReferenceEquals(a.Right, arrow));
            if (assign != null && assign.Left is Identifier aid)
                return aid.Name;

            return string.Empty;
        }

        private static IEnumerable<Node> Traverse(Node root)
        {
            var stack = new Stack<Node>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;
                foreach (var child in current.ChildNodes)
                {
                    if (child != null)
                        stack.Push(child);
                }
            }
        }
        
        public IEnumerable<CodeElement> FindElements(IEnumerable<CodeElement> elements, string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return elements.Where(e => regex.IsMatch(e.Name) || regex.IsMatch(e.Signature));
        }
        
        public SyntaxValidationResult ValidateSyntax(string code, string language)
        {
            var result = new SyntaxValidationResult { IsValid = true };

            if (!SupportedLanguages.Contains(language))
            {
                result.IsValid = false;
                result.Errors.Add(new SyntaxError { Message = $"Unsupported language: {language}", ErrorCode = "JS_UNSUPPORTED" });
                return result;
            }

            try
            {
                new JavaScriptParser(code, new ParserOptions { Tolerant = false }).ParseScript();
            }
            catch (ParserException ex)
            {
                result.IsValid = false;
                result.Errors.Add(new SyntaxError
                {
                    Message = ex.Description,
                    Line = ex.LineNumber,
                    Column = ex.Column,
                    ErrorCode = "JS_PARSE"
                });
            }

            return result;
        }
        
        public ProjectStructure GetProjectStructure(string projectPath)
        {
            var structure = new ProjectStructure
            {
                RootPath = projectPath,
                Languages = SupportedLanguages.ToList()
            };
            
            if (System.IO.Directory.Exists(projectPath))
            {
                var jsFiles = System.IO.Directory.GetFiles(projectPath, "*.js", System.IO.SearchOption.AllDirectories);
                var tsFiles = System.IO.Directory.GetFiles(projectPath, "*.ts", System.IO.SearchOption.AllDirectories);
                
                foreach (var file in jsFiles.Concat(tsFiles))
                {
                    structure.Files.Add(new ProjectFile
                    {
                        FilePath = file,
                        Language = file.EndsWith(".ts") ? "TypeScript" : "JavaScript",
                        Size = new System.IO.FileInfo(file).Length,
                        LastModified = System.IO.File.GetLastWriteTime(file)
                    });
                }
            }
            
            return structure;
        }
        
        public PatchResult ApplyPatch(string originalCode, PatchRule rule, string language)
        {
            var result = new PatchResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                if (!SupportedLanguages.Contains(language))
                {
                    result.Errors.Add($"Unsupported language: {language}");
                    return result;
                }
                
                var elements = AnalyzeCode(originalCode, language);
                var targets = FindTargetElements(elements, rule);
                
                if (!targets.Any())
                {
                    result.Warnings.Add("No target elements found for the rule");
                    result.ModifiedCode = originalCode;
                    return result;
                }
                
                var modifiedCode = originalCode;
                
                // Применяем патч к каждому найденному элементу
                foreach (var target in targets.OrderByDescending(t => t.StartLine))
                {
                    modifiedCode = ApplyPatchToElement(modifiedCode, target, rule);
                    result.ElementsModified++;
                }
                
                result.ModifiedCode = modifiedCode;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error applying patch: {ex.Message}");
            }
            finally
            {
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
            }
            
            return result;
        }
        
        private IEnumerable<CodeElement> FindTargetElements(IEnumerable<CodeElement> elements, PatchRule rule)
        {
            var targets = elements.AsEnumerable();
            
            // Фильтр по типу элемента
            if (rule.TargetElementType != CodeElementType.Unknown)
            {
                targets = targets.Where(e => e.Type == rule.TargetElementType);
            }
            
            // Фильтр по паттерну
            if (!string.IsNullOrEmpty(rule.TargetPattern))
            {
                targets = FindElements(targets, rule.TargetPattern);
            }
            
            return targets;
        }
        
        private string ApplyPatchToElement(string code, CodeElement element, PatchRule rule)
        {
            var lines = code.Split('\n').ToList();
            
            switch (rule.PatchType)
            {
                case PatchType.Replace:
                    return ReplaceElement(code, element, rule.NewContent);
                    
                case PatchType.InsertBefore:
                    lines.Insert(element.StartLine - 1, rule.NewContent);
                    break;
                    
                case PatchType.InsertAfter:
                    lines.Insert(element.EndLine, rule.NewContent);
                    break;
                    
                case PatchType.Delete:
                    for (int i = element.EndLine - 1; i >= element.StartLine - 1; i--)
                    {
                        if (i < lines.Count) lines.RemoveAt(i);
                    }
                    break;
                    
                case PatchType.Wrap:
                    var wrapped = rule.NewContent.Replace("{ORIGINAL}", element.Content);
                    return ReplaceElement(code, element, wrapped);
            }
            
            return string.Join("\n", lines);
        }
        
        private string ReplaceElement(string code, CodeElement element, string newContent)
        {
            var lines = code.Split('\n').ToList();
            
            // Удаляем старые строки
            for (int i = element.EndLine - 1; i >= element.StartLine - 1; i--)
            {
                if (i < lines.Count) lines.RemoveAt(i);
            }
            
            // Вставляем новый контент
            var newLines = newContent.Split('\n');
            lines.InsertRange(element.StartLine - 1, newLines);
            
            return string.Join("\n", lines);
        }
        
        public bool CanApplyPatch(string code, PatchRule rule, string language)
        {
            if (!SupportedLanguages.Contains(language)) return false;
            
            var elements = AnalyzeCode(code, language);
            var targets = FindTargetElements(elements, rule);
            
            return targets.Any();
        }
        
        public string PreviewPatch(string originalCode, PatchRule rule, string language)
        {
            var result = ApplyPatch(originalCode, rule, language);
            return result.Success ? result.ModifiedCode : originalCode;
        }
        
        public ValidationResult ValidateRule(PatchRule rule)
        {
            var result = new ValidationResult { IsValid = true };
            
            if (string.IsNullOrWhiteSpace(rule.Name))
            {
                result.IsValid = false;
                result.Errors.Add("Rule name is required");
            }
            
            if (!SupportedLanguages.Contains(rule.TargetLanguage) && rule.TargetLanguage != "All")
            {
                result.IsValid = false;
                result.Errors.Add($"Unsupported target language: {rule.TargetLanguage}");
            }
            
            if (string.IsNullOrWhiteSpace(rule.TargetPattern))
            {
                result.Warnings.Add("Target pattern is empty - rule will match all elements of the specified type");
            }
            
            return result;
        }
    }
}
