using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using UniversalCodePatcher.Modules.CSharpModule;
using UniversalCodePatcher.Models;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class CSharpModuleTests
    {
        private const string SampleCode = @"namespace Demo {
    public interface IBase { void Foo(); }
    public class BaseClass { public int X; }
    public class Derived<T> : BaseClass, IBase {
        public T Value { get; set; }
        public void Foo() { }
    }
}";

        private const string PatternSample = @"namespace MyNamespace {
    public class Service {
        public void GetData() {}
        public void SaveData() {}
    }
        }";

        private const string SignatureSample = @"namespace Demo {
    public class Data {
        public int Calc(int x) { return x; }
    }
}";

        [TestMethod]
        public void AnalyzeCode_ExtractsElements()
        {
            var module = new CSharpModule();
            module.Initialize(null);
            var elements = module.AnalyzeCode(SampleCode, "CSharp").ToList();
            Assert.IsTrue(elements.Any(e => e.Type == CodeElementType.Interface && e.Name == "IBase"));
            Assert.IsTrue(elements.Any(e => e.Type == CodeElementType.Class && e.Name == "Derived"));
            Assert.IsTrue(elements.Any(e => e.Type == CodeElementType.Field && e.Name == "X"));
            Assert.IsTrue(elements.Any(e => e.Type == CodeElementType.Property && e.Name == "Value"));
            Assert.IsTrue(elements.Any(e => e.Type == CodeElementType.Method && e.Name == "Foo"));
        }

        [TestMethod]
        public void ApplyPatch_AddPropertyToClass()
        {
            var module = new CSharpModule();
            module.Initialize(null);
            var rule = new PatchRule
            {
                PatchType = PatchType.Modify,
                TargetPattern = "Derived",
                TargetLanguage = "CSharp",
                TargetElementType = CodeElementType.Class,
                NewContent = "public string Added { get; set; }"
            };
            var result = module.ApplyPatch(SampleCode, rule, "CSharp");
            Assert.IsTrue(result.Success, string.Join(";", result.Errors));
            var elements = module.AnalyzeCode(result.ModifiedCode, "CSharp").ToList();
            Assert.IsTrue(elements.Any(e => e.Type == CodeElementType.Property && e.Name == "Added"));
        }

        [TestMethod]
        public void SymbolMatches_WildcardAndRegex()
        {
            var module = new CSharpModule();
            module.Initialize(null);
            var wildcardRule = new PatchRule
            {
                PatchType = PatchType.Delete,
                TargetPattern = "*.Get*",
                TargetLanguage = "CSharp",
                TargetElementType = CodeElementType.Method
            };
            Assert.IsTrue(module.CanApplyPatch(PatternSample, wildcardRule, "CSharp"));

            var regexRule = new PatchRule
            {
                PatchType = PatchType.Delete,
                TargetPattern = "/Save.*Data/",
                TargetLanguage = "CSharp",
                TargetElementType = CodeElementType.Method
            };
            Assert.IsTrue(module.CanApplyPatch(PatternSample, regexRule, "CSharp"));
        }

        [TestMethod]
        public void ApplyPatch_InsertBeforeAndAfter()
        {
            var module = new CSharpModule();
            module.Initialize(null);

            var beforeRule = new PatchRule
            {
                PatchType = PatchType.InsertBefore,
                TargetPattern = "X",
                TargetElementType = CodeElementType.Field,
                TargetLanguage = "CSharp",
                NewContent = "public int Y;"
            };
            var afterRule = new PatchRule
            {
                PatchType = PatchType.InsertAfter,
                TargetPattern = "Foo",
                TargetElementType = CodeElementType.Method,
                TargetLanguage = "CSharp",
                NewContent = "public void Bar() {}"
            };

            var result1 = module.ApplyPatch(SampleCode, beforeRule, "CSharp");
            Assert.IsTrue(result1.Success, string.Join(";", result1.Errors));
            var result2 = module.ApplyPatch(result1.ModifiedCode, afterRule, "CSharp");
            Assert.IsTrue(result2.Success, string.Join(";", result2.Errors));
            Assert.IsTrue(result2.ModifiedCode.Contains("public int Y"));
            Assert.IsTrue(result2.ModifiedCode.Contains("Bar()"));
        }

        [TestMethod]
        public void ApplyPatch_ChangeSignature()
        {
            var module = new CSharpModule();
            module.Initialize(null);

            var sigRule = new PatchRule
            {
                PatchType = PatchType.ChangeSignature,
                TargetPattern = "Calc",
                TargetElementType = CodeElementType.Method,
                TargetLanguage = "CSharp",
                NewContent = "public int Calc(int x, int y)"
            };

            var result = module.ApplyPatch(SignatureSample, sigRule, "CSharp");
            Assert.IsTrue(result.Success, string.Join(";", result.Errors));
            Assert.IsTrue(result.ModifiedCode.Contains("Calc(int x, int y)"));
        }

        [TestMethod]
        public void ApplyPatch_AddAttribute_ChangeVisibility()
        {
            var module = new CSharpModule();
            module.Initialize(null);

            var attrRule = new PatchRule
            {
                PatchType = PatchType.AddAttribute,
                TargetPattern = "Derived",
                TargetElementType = CodeElementType.Class,
                TargetLanguage = "CSharp",
                NewContent = "System.Obsolete"
            };

            var visRule = new PatchRule
            {
                PatchType = PatchType.ChangeVisibility,
                TargetPattern = "X",
                TargetElementType = CodeElementType.Field,
                TargetLanguage = "CSharp",
                NewContent = "private"
            };

            var r1 = module.ApplyPatch(SampleCode, attrRule, "CSharp");
            Assert.IsTrue(r1.Success, string.Join(";", r1.Errors));
            var r2 = module.ApplyPatch(r1.ModifiedCode, visRule, "CSharp");
            Assert.IsTrue(r2.Success, string.Join(";", r2.Errors));

            Assert.IsTrue(r2.ModifiedCode.Contains("[System.Obsolete]"));
            Assert.IsTrue(r2.ModifiedCode.Contains("private int X"));
        }
    }
}
