using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BundleSink.TestServer.ViewComponents
{
    public class TestEncapsulatedViewComponent : ViewComponent {
        public IViewComponentResult Invoke(string text = "") {
            return View(null, text);
        }
    }
}