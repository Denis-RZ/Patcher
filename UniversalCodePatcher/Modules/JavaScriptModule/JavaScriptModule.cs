using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        
        private readonly Dictionary<string, Regex> _patterns = new();
        
        protected override bool OnInitialize()
        {
            InitializePatterns();
            
            // Регистрируем сервисы
            ServiceContainer?.RegisterInstance<ICodeAnalyzer>(this);
            ServiceContainer?.RegisterInstance<IPatcher>(this);
            
            OnInfo("JavaScript module initialized successfully");
            return true;
        }
        
        private void InitializePatterns()
        {
            _patterns["function"] = new Regex(
                @"(?:export\s+)?(?:async\s+)?function\s+(\w+)\s*\(([^)]*)\)\s*(?::\s*[^{]+)?\s*\{(?:[^{}]++|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}",
                RegexOptions.Compiled | RegexOptions.Singleline
            );
            
            _patterns["arrow_function"] = new Regex(
                @"(?:const|let|var)\s+(\w+)\s*=\s*(?:async\s+)?\(([^)]*)\)\s*(?::\s*[^=]+)?\s*=>\s*(?:\{(?:[^{}]++|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}|[^;,\n]+)",
                RegexOptions.Compiled | RegexOptions.Singleline
            );
            
            _patterns["class"] = new Regex(
                @"(?:export\s+)?(?:abstract\s+)?class\s+(\w+)(?:\s+extends\s+(\w+))?(?:\s+implements\s+([^{]+))?\s*\{(?:[^{}]++|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}",
                RegexOptions.Compiled | RegexOptions.Singleline
            );
            
            _patterns["method"] = new Regex(
                @"(?:(?:public|private|protected|static|async|get|set)\s+)*(\w+)\s*\(([^)]*)\)\s*(?::\s*[^{]+)?\s*\{(?:[^{}]++|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}",
                RegexOptions.Compiled | RegexOptions.Singleline
            );
        }
        
        public IEnumerable<CodeElement> AnalyzeCode(string code, string language)
        {
            if (!SupportedLanguages.Contains(language))
                return Enumerable.Empty<CodeElement>();
            
            var elements = new List<CodeElement>();
            var lines = code.Split('\n');
            
            // Анализируем функции
            foreach (var match in _patterns["function"].Matches(code).Cast<Match>())
            {
                elements.Add(CreateCodeElement(match, CodeElementType.Function, lines, code));
            }
            
            // Анализируем стрелочные функции
            foreach (var match in _patterns["arrow_function"].Matches(code).Cast<Match>())
            {
                elements.Add(CreateCodeElement(match, CodeElementType.Lambda, lines, code));
            }
            
            // Анализируем классы
            foreach (var match in _patterns["class"].Matches(code).Cast<Match>())
            {
                elements.Add(CreateCodeElement(match, CodeElementType.Class, lines, code));
            }
            
            // Анализируем методы
            foreach (var match in _patterns["method"].Matches(code).Cast<Match>())
            {
                elements.Add(CreateCodeElement(match, CodeElementType.Method, lines, code));
            }
            
            return elements.OrderBy(e => e.StartLine);
        }
        
        private CodeElement CreateCodeElement(Match match, CodeElementType type, string[] lines, string fullCode)
        {
            var element = new CodeElement
            {
                Type = type,
                Content = match.Value,
                Language = "JavaScript"
            };
            
            // Извлекаем имя
            if (match.Groups.Count > 1)
            {
                element.Name = match.Groups[1].Value;
            }
            
            // Вычисляем позицию
            var beforeMatch = fullCode.Substring(0, match.Index);
            element.StartLine = beforeMatch.Split('\n').Length;
            element.EndLine = element.StartLine + match.Value.Split('\n').Length - 1;
            
            // Извлекаем параметры
            if (match.Groups.Count > 2 && type != CodeElementType.Class)
            {
                element.Parameters = ParseParameters(match.Groups[2].Value);
            }
            
            // Генерируем сигнатуру
            element.Signature = $"{type}:{element.Name}({string.Join(",", element.Parameters)})";
            
            return element;
        }
        
        private string[] ParseParameters(string paramString)
        {
            if (string.IsNullOrWhiteSpace(paramString))
                return Array.Empty<string>();
            
            return paramString.Split(',')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray();
        }
        
        public IEnumerable<CodeElement> FindElements(IEnumerable<CodeElement> elements, string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return elements.Where(e => regex.IsMatch(e.Name) || regex.IsMatch(e.Signature));
        }
        
        public SyntaxValidationResult ValidateSyntax(string code, string language)
        {
            // Базовая валидация синтаксиса
            var result = new SyntaxValidationResult { IsValid = true };
            
            // Проверка парности скобок
            var braceCount = 0;
            var parenCount = 0;
            var bracketCount = 0;
            
            for (int i = 0; i < code.Length; i++)
            {
                switch (code[i])
                {
                    case '{': braceCount++; break;
                    case '}': braceCount--; break;
                    case '(': parenCount++; break;
                    case ')': parenCount--; break;
                    case '[': bracketCount++; break;
                    case ']': bracketCount--; break;
                }
            }
            
            if (braceCount != 0)
            {
                result.IsValid = false;
                result.Errors.Add(new SyntaxError { Message = "Unmatched braces", ErrorCode = "JS001" });
            }
            
            if (parenCount != 0)
            {
                result.IsValid = false;
                result.Errors.Add(new SyntaxError { Message = "Unmatched parentheses", ErrorCode = "JS002" });
            }
            
            if (bracketCount != 0)
            {
                result.IsValid = false;
                result.Errors.Add(new SyntaxError { Message = "Unmatched brackets", ErrorCode = "JS003" });
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
