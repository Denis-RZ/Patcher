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
    }
}
