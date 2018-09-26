using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinMonitor.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet, Route("")]
        public async Task<ActionResult> Index()
        {
            return View();
        }
    }
}