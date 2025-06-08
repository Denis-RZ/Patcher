using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.Modules.JavaScriptModule;
using UniversalCodePatcher.Models;
using System.Linq;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class JavaScriptModuleTests
    {
        [TestMethod]
        public void AnalyzeCode_ParsesFunctionsAndClasses()
        {
            var code = @"const add = (a, b) => a + b;
function foo(x){return x;}
class MyClass{bar(y){return y;}}";
            var module = new JavaScriptModule();
            module.Initialize(null);
            var elements = module.AnalyzeCode(code, "JavaScript").ToList();
            Assert.AreEqual(4, elements.Count);
            Assert.IsTrue(elements.Any(e => e.Type == CodeElementType.Lambda && e.Name == "add"));
            Assert.IsTrue(elements.Any(e => e.Type == CodeElementType.Function && e.Name == "foo"));
            Assert.IsTrue(elements.Any(e => e.Type == CodeElementType.Class && e.Name == "MyClass"));
            Assert.IsTrue(elements.Any(e => e.Type == CodeElementType.Method && e.Name == "bar"));
        }

        [TestMethod]
        public void ValidateSyntax_InvalidCode_ReturnsError()
        {
            var code = "function test( {"; // missing closing parenthesis and body
            var module = new JavaScriptModule();
            module.Initialize(null);
            var result = module.ValidateSyntax(code, "JavaScript");
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any());
        }

        [TestMethod]
        public void ValidateSyntax_ValidCode_ReturnsSuccess()
        {
            var code = "function ok(){return 1;}";
            var module = new JavaScriptModule();
            module.Initialize(null);
            var result = module.ValidateSyntax(code, "JavaScript");
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }
    }
}
