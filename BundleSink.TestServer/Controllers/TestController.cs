using Microsoft.AspNetCore.Mvc;

namespace BundleSink.TestServer.Controllers
{
    public class TestController : Controller {
        public IActionResult DuplicateEntriesOnSameSinkTest() {
            return View();
        }
        public IActionResult SameEntryDifferentKeyTest() {
            return View();
        }

        public IActionResult DifferentEntriesOnDifferentSinksTest() {
            return View();
        }

        public IActionResult SameEntryDifferentSinksTest() {
            return View();
        }

        public IActionResult OneEntryWithOneDependencyTest() {
            return View();
        }

        public IActionResult OneEntryWithOneAlreadyDeclaredDependencyTest()
        {
            return View();
        }

        public IActionResult OneEntryWithOneAlreadyDeclaredDependencyInPreviousSinkTest()
        {
            return View();
        }

        public IActionResult OneEntryWithOneAlreadyDeclaredDependencyInNextSinkTest()
        {
            return View();
        }

        public IActionResult OneDependencyWithNoDependantsTest()
        {
            return View();
        }

        public IActionResult SameEntryWithinAPartialTest() {
            return View();
        }

        public IActionResult SameEntryWithinNestedPartialsTest()
        {
            return View();
        }

        public IActionResult SinkBeforeNestedContentWithEntryTest() {
            return View();
        }

        public IActionResult SinkBeforeNestedContentWithNestedEntriesTest() {
            return View();
        }

        public IActionResult WebpackEntryWithCSSOnlyAttributeTest() {
            return View();
        }

        public IActionResult WebpackEntryWithJSOnlyAttributeTest()
        {
            return View();
        }

        public IActionResult SimpleLiteralEntryTest() {
            return View();
        }

        public IActionResult LiteralEntryWithDependencyTest() {
            return View();
        }

        public IActionResult DuplicateLiteralEntriesTest() {
            return View();
        }
    }
}